using UnityEngine;
using Oxide.Core;
using Oxide.Core.Plugins;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("Swimming Drops", "YourName", "1.1.0")]
    [Description("Keeps supply drops swimming on water after they touch the water surface and ensures parachutes detach")]
    public class SwimmingDrops : RustPlugin
    {
        private const float FloatHeight = 0.1f;
        private HashSet<SupplyDrop> activeDrops = new HashSet<SupplyDrop>();

        void OnEntitySpawned(SupplyDrop drop)
        {
            if (drop == null) return;
            drop.gameObject.AddComponent<DropMonitor>().Init(this);
        }

        void OnEntityKill(SupplyDrop drop)
        {
            if (drop == null) return;
            activeDrops.Remove(drop);
        }

        void Unload()
        {
            foreach (var drop in activeDrops)
            {
                if (drop != null && drop.gameObject != null)
                {
                    UnityEngine.Object.Destroy(drop.gameObject.GetComponent<DropMonitor>());
                }
            }
            activeDrops.Clear();
        }

        public void AddActiveDrops(SupplyDrop drop)
        {
            activeDrops.Add(drop);
        }

        private class DropMonitor : MonoBehaviour
        {
            private SupplyDrop drop;
            private Rigidbody rb;
            private bool isInWater = false;
            private SwimmingDrops pluginInstance;

            public void Init(SwimmingDrops plugin)
            {
                pluginInstance = plugin;
            }

            void Awake()
            {
                drop = GetComponent<SupplyDrop>();
                rb = GetComponent<Rigidbody>();
            }

            void FixedUpdate()
            {
                if (drop == null || drop.IsDestroyed) 
                {
                    Destroy(this);
                    return;
                }

                var waterInfo = WaterLevel.GetWaterInfo(transform.position, true, true);

                if (!isInWater && waterInfo.isValid && transform.position.y <= waterInfo.surfaceLevel)
                {
                    isInWater = true;
                    pluginInstance.AddActiveDrops(drop);
                    DetachParachute();
                }

                if (isInWater)
                {
                    FloatSupplyDrop(waterInfo.surfaceLevel);
                }
            }

            private void FloatSupplyDrop(float waterLevel)
            {
                var targetY = waterLevel + FloatHeight;
                var currentY = transform.position.y;

                if (currentY < targetY)
                {
                    var force = (targetY - currentY) * rb.mass * 20f;
                    rb.AddForce(Vector3.up * force, ForceMode.Force);
                }
                else
                {
                    rb.AddForce(Vector3.down * rb.mass * 9.81f, ForceMode.Force);
                }

                rb.drag = 0.5f;
                rb.angularDrag = 0.5f;
            }

            private void DetachParachute()
            {
                // Attempt to detach the parachute
                var parachute = GetComponentInChildren<Parachute>();
                if (parachute != null)
                {
                    parachute.Kill(BaseNetworkable.DestroyMode.None);
                }
            }
        }
    }
}