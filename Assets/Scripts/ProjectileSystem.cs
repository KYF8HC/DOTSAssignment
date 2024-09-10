using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial struct ProjectileSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        var stateEntityManager = state.EntityManager;

        foreach (var entity in stateEntityManager.GetAllEntities())
        {
            if (!stateEntityManager.HasComponent<ProjectileComponent>(entity) && !stateEntityManager.HasComponent<ProjectileLifetimeComponent>(entity)) continue;
            
            
            
            var localTransform = stateEntityManager.GetComponentData<LocalTransform>(entity);
            var projectileComponent = stateEntityManager.GetComponentData<ProjectileComponent>(entity);
            var projectileLifetimeComponent = stateEntityManager.GetComponentData<ProjectileLifetimeComponent>(entity);
            
            if (!projectileComponent.bWasInitialized)
            {
                localTransform.Position = projectileComponent.initialPosition;
                localTransform.Rotation = projectileComponent.Rotation;
                projectileComponent.bWasInitialized = true;
                stateEntityManager.SetComponentData(entity, projectileComponent);
            }
            
            localTransform.Position += projectileComponent.Speed * deltaTime * localTransform.Right();
            stateEntityManager.SetComponentData(entity, localTransform);
                
            projectileLifetimeComponent.RemainingLifetime -= deltaTime;
            if (projectileLifetimeComponent.RemainingLifetime <= 0.0f)
            {
                stateEntityManager.DestroyEntity(entity);
                continue;
            }
                
            stateEntityManager.SetComponentData(entity, projectileLifetimeComponent);
                
            var hits = new NativeList<ColliderCastHit>(Allocator.Temp);
            var point1 = new float3(localTransform.Position - localTransform.Right() * 0.15f);
            var point2 = new float3(localTransform.Position + localTransform.Right() * 0.15f);
            
            physicsWorldSingleton.CapsuleCastAll(point1, point2, projectileComponent.ColliderSize / 2, float3.zero,
                1.0f, ref hits, new CollisionFilter
                {
                    BelongsTo = (uint)CollisionLayer.Default,
                    CollidesWith = (uint)CollisionLayer.Enemy
                });
            
            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    var hitEntity = hit.Entity;
                    if (!stateEntityManager.HasComponent<EnemyComponent>(hitEntity)) continue;
                    
                    var enemyComponent = stateEntityManager.GetComponentData<EnemyComponent>(hitEntity);
                    enemyComponent.CurrentHealth -= projectileComponent.Damage;
                    stateEntityManager.SetComponentData(hitEntity, enemyComponent);
                    
                    if (enemyComponent.CurrentHealth <= 0.0f)
                    {
                        stateEntityManager.DestroyEntity(hitEntity);
                    }
                }

                stateEntityManager.DestroyEntity(entity);
            }
            
            hits.Dispose();
        }
    }
}