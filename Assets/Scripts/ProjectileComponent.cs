using Unity.Entities;
using Unity.Mathematics;

public class ProjectileComponent : IComponentData
{
    public float Speed;
    public float Damage;
    public float ColliderSize;
    public quaternion Rotation;
    public float3 initialPosition;
    public bool bWasInitialized;
}

public class ProjectileLifetimeComponent : IComponentData
{
    public float RemainingLifetime;
}