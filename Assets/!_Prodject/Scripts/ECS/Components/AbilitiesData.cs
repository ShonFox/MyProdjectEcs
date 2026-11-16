// Components/AbilitiesData.cs
using System.Collections.Generic;
using Unity.Entities;

/// <summary>
/// Список способностей сущности.
/// Это managed-компонент (внутри список — сложный тип), поэтому это класс, а не struct.
/// Такие компоненты добавляются через AddComponentObject, а не AddComponentData.
///
/// AbilitySystem каждый кадр спрашивает у этого списка все способности
/// и передаёт им состояние кнопки. Система НЕ знает, какие именно способности
/// у сущности — поэтому новая способность добавляется без правки старого кода.
/// </summary>
public class AbilitiesData : IComponentData
{
    /// <summary>Все способности, которые собрали AbilitiesAuthoring с инспектора.</summary>
    public readonly List<IAbility> Abilities = new List<IAbility>();

    /// <summary>Добавляет способность. Защищаемся от null и дублей, чтобы не стрелять дважды.</summary>
    public void Add(IAbility ability)
    {
        if (ability != null && !Abilities.Contains(ability))
            Abilities.Add(ability);
    }
}
