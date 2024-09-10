using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial struct SpawnerSystem : ISystem
{
    private EntityManager _entityManager;
    private Entity _enemySpawnerEntity;
    private SpawnerComponent _enemySpawnerComponent;
    private Entity _playerEntity;
    
    private Unity.Mathematics.Random _random;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
        state.RequireForUpdate<SpawnerComponent>();

        _random = Unity.Mathematics.Random.CreateFromIndex((uint)_enemySpawnerComponent.GetHashCode());
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _enemySpawnerEntity = SystemAPI.GetSingletonEntity<SpawnerComponent>();
        _enemySpawnerComponent = _entityManager.GetComponentData<SpawnerComponent>(_enemySpawnerEntity);
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        
        SpawnEnemies(ref state);
    }

    private void SpawnEnemies(ref SystemState state)
    {
        _enemySpawnerComponent.CurrentTimeBeforeNextSpawn -= SystemAPI.Time.DeltaTime;
        if (_enemySpawnerComponent.CurrentTimeBeforeNextSpawn <= 0.0f)
        {
            for (int i = 0; i < _enemySpawnerComponent.NumOfEnemiesToSpawnPerSecond; i++)
            {
                var ecb = new EntityCommandBuffer(Allocator.Temp);
                var enemyEntity = _entityManager.Instantiate(_enemySpawnerComponent.Prefab);
                
                var playerLocalTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
                var enemyLocalTransform = _entityManager.GetComponentData<LocalTransform>(enemyEntity);
                
                var minDistanceSquared = _enemySpawnerComponent.MinimumDistanceFromPlayer * _enemySpawnerComponent.MinimumDistanceFromPlayer;
                var randomOffset = _random.NextFloat2Direction() * _random.NextFloat(_enemySpawnerComponent.MinimumDistanceFromPlayer, _enemySpawnerComponent.EnemySpawnRadius);
                var playerPosition = new float2(playerLocalTransform.Position.x, playerLocalTransform.Position.y);
                var spawnPosition = playerPosition + randomOffset;
                var distanceSquared = math.lengthsq(spawnPosition - playerPosition);
                
                if (distanceSquared < minDistanceSquared)
                {
                    spawnPosition = playerPosition + math.normalize(randomOffset) * math.sqrt(minDistanceSquared);
                }
                enemyLocalTransform.Position = new float3(spawnPosition.x, spawnPosition.y, 0.0f);
                
                var direction = math.normalize(playerLocalTransform.Position - enemyLocalTransform.Position);
                var angle = math.atan2(direction.y, direction.x);
                angle -= math.radians(90.0f);
                var lookRotation = quaternion.AxisAngle(new float3(0,0,1), angle);
                enemyLocalTransform.Rotation = lookRotation;
                
                ecb.SetComponent(enemyEntity, enemyLocalTransform);
                
                ecb.AddComponent(enemyEntity, new EnemyComponent
                {
                    CurrentHealth = 30.0f,
                    EnemySpeed = 3.0f
                });
                
                ecb.Playback(_entityManager);
                ecb.Dispose();
            }
            
            var newNumOfEnemiesToSpawnPerSecond = _enemySpawnerComponent.NumOfEnemiesToSpawnPerSecond + _enemySpawnerComponent.NumOfEnemiesToSpawnIncrementAmount;
            var enemiesPerWave = math.min(newNumOfEnemiesToSpawnPerSecond, _enemySpawnerComponent.MaxNumOfEnemiesToSpawnPerSecond);
            _enemySpawnerComponent.NumOfEnemiesToSpawnPerSecond = enemiesPerWave;
            
            _enemySpawnerComponent.CurrentTimeBeforeNextSpawn = _enemySpawnerComponent.TimeBeforeNextSpawn;
        }
        
        _entityManager.SetComponentData(_enemySpawnerEntity, _enemySpawnerComponent);
    }
}