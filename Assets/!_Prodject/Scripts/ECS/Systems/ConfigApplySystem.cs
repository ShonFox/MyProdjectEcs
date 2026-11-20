// Systems/ConfigApplySystem.cs
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Переносит скорость из PlayerConfigData в MoveData — ровно один раз.
/// Параметр из JSON реально меняет значение у игрока (требование практики).
/// Пишем в BaseSpeed, а не в Speed: Speed каждый кадр пересчитывает
/// SprintSystem из BaseSpeed, запись в Speed стиралась бы мгновенно.
/// </summary>
public class ConfigApplySystem : ComponentSystem
{
    private EntityQuery _query;

    protected override void OnCreate()
    {
        // Оба компонента нужны одновременно: откуда берём и куда кладём
        _query = GetEntityQuery(
            ComponentType.ReadOnly<PlayerConfigData>(),
            ComponentType.ReadOnly<MoveData>());
    }

    protected override void OnUpdate()
    {
        Entities.With(_query).ForEach(
            (ref PlayerConfigData cfg, ref MoveData move) =>
            {
                // ПРАВИЛЬНОЕ условие: применяем, когда данные ПРИШЛИ (Loaded)
                // и ещё НЕ применялись (Applied = false).
                // После применения поднимаем Applied = true — и больше не трогаем
                if (!cfg.Loaded || cfg.Applied) return;

                // Страховка: JSON мог прийти с нулём или отрицательным числом —
                // такой «скорости» доверять нельзя
                if (cfg.Speed <= 0f)
                {
                    Debug.Log($"[Config] Подозрительная скорость {cfg.Speed}, игнорирую.");
                    cfg.Applied = true; // пометили обработанным, чтобы не спамить логом
                    return;
                }

                // Меняем ИСТОЧНИК скорости: конфиг ставит базу,
                // спринт честно умножает её на свой множитель
                move.BaseSpeed = cfg.Speed;
                cfg.Applied = true;

                Debug.Log($"[Config] Применено: базовая скорость теперь {cfg.Speed}");
            });
    }
}
