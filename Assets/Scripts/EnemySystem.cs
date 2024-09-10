using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct EnemySystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerComponent>();
    }

    private EntityManager _entityManager;
    private Entity _playerEntity;
    
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        var playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
        
        var allEntities = _entityManager.GetAllEntities();

        foreach (var entity in allEntities)
        {
            if(!_entityManager.HasComponent<EnemyComponent>(entity)) continue;
            var enemyTransform = _entityManager.GetComponentData<LocalTransform>(entity);
            var enemyComponent = _entityManager.GetComponentData<EnemyComponent>(entity);
            
            var direction = math.normalize(playerTransform.Position - enemyTransform.Position);
            enemyTransform.Position += direction * enemyComponent.EnemySpeed * SystemAPI.Time.DeltaTime;
            
            var lookDirection = math.normalize(playerTransform.Position - enemyTransform.Position);
            var angle = math.atan2(lookDirection.y, lookDirection.x);
            angle -= math.radians(90.0f);
            var lookRotation = quaternion.AxisAngle(new float3(0,0,1), angle);
            enemyTransform.Rotation = lookRotation;
            
            _entityManager.SetComponentData(entity, enemyTransform);
        }
    }
}