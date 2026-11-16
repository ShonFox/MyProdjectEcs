// Systems/SprintSystem.cs
using Unity.Entities;

/// <summary>
/// Спринт.
/// Пока зажата кнопка спринта — записывает в MoveData.Speed базовую скорость,
/// умноженную на множитель. Кнопку отпустили — возвращает обычную скорость.
/// Система НИЧЕГО не двигает — она только готовит данные.
/// Двигает CharacterMoveSystem, читая move.Speed.
/// По этому же шаблону можно сделать прыжок, замедление в воде, бафы и т.д.
/// </summary>
public class SprintSystem : ComponentSystem
{
    // Ищем сущности, у которых есть ВСЕ три компонента сразу:
    // без любого из них сущность просто не попадёт в обработку
    private EntityQuery _sprintQuery;

    protected override void OnCreate()
    {
        _sprintQuery = GetEntityQuery(
            ComponentType.ReadOnly<InputData>(),
            ComponentType.ReadOnly<MoveData>(),
            ComponentType.ReadOnly<SprintData>());
    }

    protected override void OnUpdate()
    {
        Entities.With(_sprintQuery).ForEach(
            (ref InputData input, ref MoveData move, ref SprintData sprint) =>
            {
                // Спринт зажат -> умножаем базовую скорость; отпущен -> обычная
                move.Speed = input.Sprint > 0f
                    ? move.BaseSpeed * sprint.Multiplier
                    : move.BaseSpeed;
            });
    }
}
