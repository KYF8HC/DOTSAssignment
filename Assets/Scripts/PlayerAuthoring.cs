using Unity.Entities;
using UnityEngine;

public class PlayerAuthoring : MonoBehaviour
{
    public float AngleOffset;
    public float MoveSpeed;
    public GameObject ProjectilePrefab;

    private class PlayerAuthoringBaker : Baker<PlayerAuthoring>
    {
        public override void Bake(PlayerAuthoring authoring)
        {
            var playerEntity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<PlayerTag>(playerEntity);
            AddComponent(playerEntity, new PlayerComponent
            {
                MoveSpeed = authoring.MoveSpeed,
                ProjectilePrefab = GetEntity(authoring.ProjectilePrefab, TransformUsageFlags.Dynamic),
                AngleOffset = authoring.AngleOffset
            });
        }
    }
}

public struct PlayerComponent : IComponentData
{
    public float MoveSpeed;
    public Entity ProjectilePrefab;
    public float AngleOffset;
}

public struct PlayerTag : IComponentData
{
}