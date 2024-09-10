using Unity.Entities;
using UnityEngine;

public partial class InputSystem : SystemBase
{
    private GameInput _gameInput;
    protected override void OnCreate()
    {
        if(!SystemAPI.TryGetSingleton(out InputComponent input))
            EntityManager.CreateEntity(typeof(InputComponent));

        _gameInput = new GameInput();
        _gameInput.Enable();
    }

    protected override void OnUpdate()
    {
        var moveVector = _gameInput.Gameplay.Move.ReadValue<Vector2>();
        var mousePos = _gameInput.Gameplay.MousePos.ReadValue<Vector2>();
        var shoot = _gameInput.Gameplay.Shoot.IsPressed();
        
        SystemAPI.SetSingleton(new InputComponent
        {
            Movement = moveVector,
            MousePosition = mousePos,
            Shoot = shoot
        });
    }
}