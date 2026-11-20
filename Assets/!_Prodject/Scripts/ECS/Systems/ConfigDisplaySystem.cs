// Systems/ConfigDisplaySystem.cs
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using TMPro;

/// <summary>
/// Показывает загруженный параметр на экране — шаг 5 практики.
/// Надпись ищем строго по имени «ConfigLabel». У нас она TextMeshPro,
/// поэтому работаем с типом TMP_Text (обычный legacy Text под него
/// тоже подходит — TMP_Text их обеих наследует).
/// Применением параметра эта система НЕ занимается — это работа
/// ConfigApplySystem. Разделение то же: одна система — один вопрос.
/// </summary>
public class ConfigDisplaySystem : ComponentSystem
{
    private EntityQuery _query;

    // Ссылка на надпись. Находим один раз и запоминаем — поиск каждый кадр дорог
    private TMP_Text _label;

    protected override void OnCreate()
    {
        _query = GetEntityQuery(ComponentType.ReadOnly<PlayerConfigData>());
    }

    protected override void OnUpdate()
    {
        // Надпись ещё не найдена? Ищем по имени объекта,
        // а не «первый попавшийся Text» — в Canvas надписей несколько
        if (_label == null)
        {
            var labelObject = GameObject.Find("ConfigLabel");
            if (labelObject == null) return; // нет надписи — тихо ждём
            _label = labelObject.GetComponent<TMP_Text>();
        }

        // Данные из запроса достаём напрямую — игрок один, ForEach не нужен
        var configs = _query.ToComponentDataArray<PlayerConfigData>(Allocator.Temp);
        if (configs.Length > 0)
        {
            var cfg = configs[0];

            // Пишем только когда данные реально загружены:
            // до этого на экране честно висит «ожидание»
            _label.text = cfg.Loaded
                ? $"Скорость из облака: {cfg.Speed}"
                : "Загрузка настроек...";
        }
        configs.Dispose();
    }
}
