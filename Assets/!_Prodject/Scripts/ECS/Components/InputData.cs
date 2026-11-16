// Components/InputData.cs — полная замена
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// Актуальное состояние ввода игрока.
/// В follow-камере взгляд отдельно не вводится: куда повернулся персонаж,
/// туда и смотрит камера. Поэтому поля Look здесь не нужны.
/// </summary>
public struct InputData : IComponentData
{
    /// <summary>Направление движения (x — влево/вправо, y — вперёд/назад). Диапазон -1..1.</summary>
    public float2 Move;

    /// <summary>Кнопка действия (стрельба). 1 — зажата, 0 — отпущена.</summary>
    public float Shoot;

    /// <summary>Кнопка спринта. 1 — зажата, 0 — отпущена.</summary>
    public float Sprint;
}
