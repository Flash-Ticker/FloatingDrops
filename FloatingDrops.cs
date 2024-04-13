using Oxide.Core.Plugins;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("FloatingDrops", "RustFlash", "1.0.0")]
    public class FloatingDrops : RustPlugin
    {
        private void OnExplosiveThrown(BasePlayer player, BaseEntity entity)
        {
            Puts("Explosive thrown: " + entity.ShortPrefabName);
            if (entity.ShortPrefabName == "grenade.supplysignal.deployed")
            {
                Puts("Detected a supply signal thrown, checking if it will land in water.");
                CheckAndFloat(entity);
            }
        }

        private void CheckAndFloat(BaseEntity entity)
        {
            timer.Once(1.0f, () =>
            {
                if (entity == null)
                {
                    Puts("Entity is null, it might have been destroyed.");
                    return;
                }
                if (entity.IsDestroyed)
                {
                    Puts("Entity is already destroyed.");
                    return;
                }

                Vector3 position = entity.transform.position;
                Puts("Checking water level for entity at position: " + position);

                float waterHeight = TerrainMeta.WaterMap.GetHeight(position);
                Puts("Water height at entity position: " + waterHeight);
                if (waterHeight > position.y)
                {
                    position.y = waterHeight;
                    entity.transform.position = position;
                    Puts("Entity landed in water, adjusted position to water surface.");

                    var rigidbody = entity.gameObject.GetComponent<Rigidbody>();
                    if (rigidbody != null)
                    {
                        rigidbody.useGravity = false;
                        rigidbody.isKinematic = false; 
                        rigidbody.drag = 1f;
                        rigidbody.angularDrag = 0.5f; 
                        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                        Puts("Adjusted Rigidbody settings for floating in water.");
                    }
                    else
                    {
                        Puts("No Rigidbody found on the entity.");
                    }
                }
                else
                {
                    Puts("Entity is not in water.");
                }
            });
        }
    }
}
