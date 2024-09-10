using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
    public int NumOfEnemiesToSpawnPerSecond = 50;
    public int NumOfEnemiesToSpawnIncrementAmount = 2;
    public int MaxNumOfEnemiesToSpawnPerSecond = 200;
    public float EnemySpawnRadius = 40f;
    public float MinimumDistanceFromPlayer = 5f;
    public float TimeBeforeNextSpawn = 2f;

    class SpawnerBaker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            Entity _entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(_entity, new SpawnerComponent
            {
                Prefab = GetEntity(authoring.Prefab, TransformUsageFlags.Dynamic),
                NumOfEnemiesToSpawnPerSecond = authoring.NumOfEnemiesToSpawnPerSecond,
                NumOfEnemiesToSpawnIncrementAmount = authoring.NumOfEnemiesToSpawnIncrementAmount,
                MaxNumOfEnemiesToSpawnPerSecond = authoring.MaxNumOfEnemiesToSpawnPerSecond,
                EnemySpawnRadius = authoring.EnemySpawnRadius,
                MinimumDistanceFromPlayer = authoring.MinimumDistanceFromPlayer,
                TimeBeforeNextSpawn = authoring.TimeBeforeNextSpawn,
            });
        }
    }
}

public struct SpawnerComponent : IComponentData
{
    public Entity Prefab;
    public int NumOfEnemiesToSpawnPerSecond;
    public int NumOfEnemiesToSpawnIncrementAmount;
    public int MaxNumOfEnemiesToSpawnPerSecond;
    public float EnemySpawnRadius;
    public float MinimumDistanceFromPlayer;
    public float TimeBeforeNextSpawn;
    public float CurrentTimeBeforeNextSpawn;
}