using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial struct PlayerSystem : ISystem
{
    private EntityManager _entityManager;
    private Entity _playerEntity;
    private Entity _inputEntity;
    
    private PlayerComponent _playerComponent;
    private InputComponent _inputComponent;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<InputComponent>();
        state.RequireForUpdate<PlayerComponent>();
    }
    
    public void OnUpdate(ref SystemState state)
    {
        _entityManager = state.EntityManager;
        _playerEntity = SystemAPI.GetSingletonEntity<PlayerComponent>();
        _inputEntity = SystemAPI.GetSingletonEntity<InputComponent>();
        
        _playerComponent = _entityManager.GetComponentData<PlayerComponent>(_playerEntity);
        _inputComponent = _entityManager.GetComponentData<InputComponent>(_inputEntity);

        Move(ref state);
        Shoot(ref state);
    }

    private void Move(ref SystemState state)
    {
        var playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
        playerTransform.Position += new float3(_inputComponent.Movement * _playerComponent.MoveSpeed * SystemAPI.Time.DeltaTime, 0f);

        if (Camera.main == null) return;
        
        var dir = (Vector2)_inputComponent.MousePosition - (Vector2)Camera.main.WorldToScreenPoint(playerTransform.Position);
        var angle = math.degrees(math.atan2(dir.y, dir.x) - _playerComponent.AngleOffset);
        playerTransform.Rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        _entityManager.SetComponentData(_playerEntity, playerTransform);
    }

    private void Shoot(ref SystemState state)
    {
        if (!_inputComponent.Shoot)
            return;

        var playerTransform = _entityManager.GetComponentData<LocalTransform>(_playerEntity);
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var projectileEntity = ecb.Instantiate(_playerComponent.ProjectilePrefab);
        
        ecb.AddComponent(projectileEntity, new ProjectileComponent
        {
            Speed = 25f,
            Damage = 10.0f,
            ColliderSize = 0.05f,
            Rotation = _entityManager.GetComponentData<LocalTransform>(_playerEntity).Rotation,
            initialPosition = playerTransform.Position + playerTransform.Right() * 0.5f,
            bWasInitialized = false
        });
        
        ecb.AddComponent(projectileEntity, new ProjectileLifetimeComponent
        {
            RemainingLifetime = 1.5f
        });
        
        ecb.Playback(_entityManager);
        ecb.Dispose();
    }
}