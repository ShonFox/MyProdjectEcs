// Systems/AbilitySystem.cs
using Unity.Entities;

/// <summary>
/// Универсальная система способностей. Заменяет CharterShootSystem.
/// Не знает, какие способности у сущности: просто каждый кадр передаёт им
/// состояние кнопки. Нажата ли кнопка — решает сама способность (см. ShootAbility).
/// Одна система обслуживает ВСЕ способности сразу:
/// стрельбу, рывок, лечение и любые будущие — код менять не придётся.
/// </summary>
public class AbilitySystem : ComponentSystem
{
    private EntityQuery _abilityQuery;

    protected override void OnCreate()
    {
        _abilityQuery = GetEntityQuery(
            ComponentType.ReadOnly<InputData>(),
            ComponentType.ReadOnly<AbilitiesData>());
    }

    protected override void OnUpdate()
    {
        Entities.With(_abilityQuery).ForEach(
            (AbilitiesData abilities, ref InputData input) =>
            {
                // Состояние кнопки одним числом: 1 — зажата, 0 — отпущена
                bool pressed = input.Shoot > 0f;


                // Отдаём кнопку каждой способности сущности
                for (int i = 0; i < abilities.Abilities.Count; i++)
                    abilities.Abilities[i]?.Execute(pressed);
            });
    }
}
