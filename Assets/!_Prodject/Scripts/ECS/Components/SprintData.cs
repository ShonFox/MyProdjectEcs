// Components/SprintData.cs
using Unity.Entities;

/// <summary>
/// Компонент спринта. Сам ничего не делает — просто хранит множитель.
/// SprintSystem читает кнопку из InputData и перезаписывает MoveData.Speed.
/// Правило: если у сущности НЕТ компонента SprintData — спринт к ней не применится.
/// Так можно включать/выключать поведение у конкретных персонажей, не меняя код.
/// </summary>
public struct SprintData : IComponentData
{
    /// <summary>Во сколько раз быстрее персонаж бежит при зажатом спринте. 2 = вдвое быстрее.</summary>
    public float Multiplier;
}
