// Systems/UserInputSystem.cs
using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Единственная система, которая знает про устройства ввода.
/// Собирает клавиатуру, геймпад и экранный джойстик в один InputData
/// и раздаёт его всем сущностям с компонентом InputData.
/// Все остальные системы читают только InputData — им всё равно,
/// откуда пришли данные: с клавиатуры, стика или пальца по экрану.
/// Это и делает систему переносимой.
/// </summary>
public class UserInputSystem : ComponentSystem
{
    // Запрос: найти все сущности с компонентом InputData — им и раздаём ввод
    private EntityQuery _inputQuery;

    // Три действия: движение, действие (стрельба), спринт
    private InputAction _moveAction;
    private InputAction _shootAction;
    private InputAction _sprintAction;

    // Актуальное состояние ввода. Обновляется событиями действий
    private float2 _moveInput;
    private float _shootInput;
    private float _sprintInput;

    protected override void OnCreate()
    {
        // Готовим запрос один раз при создании системы — так дешевле
        _inputQuery = GetEntityQuery(ComponentType.ReadOnly<InputData>());
    }

    protected override void OnStartRunning()
    {
        // --- ДВИЖЕНИЕ ---------------------------------------------------
        // Одно действие сразу на стик геймпада И на WASD.
        // Экранный джойстик (On-Screen Stick) тоже пишет в <Gamepad>/leftStick,
        // поэтому мобильный джойстик заработает без правок кода.
        _moveAction = new InputAction("move", binding: "<Gamepad>/leftStick");
        _moveAction.AddCompositeBinding("Dpad") // WASD превращаем в «виртуальный стик»
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        Bind(_moveAction, ctx => _moveInput = ctx.ReadValue<Vector2>());

        // --- СТРЕЛЬБА (шаг 3) -------------------------------------------
        // Клавиша RightMouseButton + кнопка B геймпада. Захотели другую клавишу — меняете строку ниже.
        _shootAction = new InputAction("shoot", binding: "<Mouse>/rightButton");
        _shootAction.AddBinding("<Gamepad>/buttonWest");
        Bind(_shootAction, ctx => _shootInput = ctx.ReadValue<float>());

        // --- СПРИНТ (шаг 2) ---------------------------------------------
        // Левый shift + кнопка A геймпада.
        _sprintAction = new InputAction("sprint", binding: "<Keyboard>/leftShift");
        _sprintAction.AddBinding("<Gamepad>/buttonSouth");
        Bind(_sprintAction, ctx => _sprintInput = ctx.ReadValue<float>());
    }

    /// <summary>
    /// Подписывает действие на все три события (нажатие, удержание, отпускание)
    /// и включает его. Нужна, чтобы не копировать три подписки для каждой кнопки.
    /// </summary>
    private void Bind(InputAction action, Action<InputAction.CallbackContext> handler)
    {
        action.performed += handler; // значение изменилось (нажали или сдвинули стик)
        action.started += handler; // только начали нажимать
        action.canceled += handler; // отпустили — важно, иначе кнопка «залипнет»
        action.Enable();             // без Enable действие не работает
    }

    protected override void OnUpdate()
    {
        // Раздаём собранный ввод всем сущностям с InputData
        Entities.With(_inputQuery).ForEach(
            (ref InputData input) =>
            {
                input.Move = _moveInput;
                input.Shoot = _shootInput;
                input.Sprint = _sprintInput;
            });
    }

    protected override void OnStopRunning()
    {
        // Системы остановились (пауза, выход из Play Mode) — отключаем действия
        _moveAction?.Disable();
        _shootAction?.Disable();
        _sprintAction?.Disable();
    }

    protected override void OnDestroy()
    {
        // InputAction держит нативные ресурсы. Без Dispose они утекут,
        // если система пересоздастся (выход/вход в Play Mode и т.п.)
        _moveAction?.Dispose();
        _shootAction?.Dispose();
        _sprintAction?.Dispose();
    }
}
