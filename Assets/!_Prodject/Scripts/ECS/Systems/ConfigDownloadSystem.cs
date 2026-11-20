// Systems/ConfigDownloadSystem.cs — новый файл
using System;
using System.IO;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Достаёт JSON с настройками игрока. Два источника, по порядку:
/// 1) системная папка (если файл уже скачивали раньше);
/// 2) Google Drive через UnityWebRequest.
/// Скачанный файл сохраняется в системную папку, чтобы в следующий запуск
/// не качать снова. Сама система НЕ блокирует игру: запрос проверяем
/// в OnUpdate, пока он не завершится — как и положено ECS.
/// </summary>
public class ConfigDownloadSystem : ComponentSystem
{
    private EntityQuery _configQuery;

    // Активный запрос к сети. null — мы не в процессе скачивания
    private UnityWebRequest _request;

    // Полный путь к файлу в системной папке
    private string _filePath;

    // Файл уже получен (локально или из сети)? Дальше ничего делать не надо
    private bool _finished;

    // Имя файла — константа, чтобы нигде не ошибиться в написании
    private const string FileName = "player_config.json";

    protected override void OnCreate()
    {
        _configQuery = GetEntityQuery(ComponentType.ReadOnly<PlayerConfigData>());
    }

    protected override void OnStartRunning()
    {
        // Application.persistentDataPath — «системная папка» из практики:
        // Windows: %userprofile%\AppData\LocalLow\<Компания>\<Проект>
        _filePath = Path.Combine(Application.persistentDataPath, FileName);
        Debug.Log(_filePath);
        // --- ШАГ 4: файл уже есть локально? Читаем его, сеть не трогаем ---
        if (File.Exists(_filePath))
        {
            LoadFromFile();
            return;
        }

        // --- ШАГ 3: файла нет — качаем с Google Drive ---
        if (string.IsNullOrEmpty(ConfigLink.Url))
        {
            Debug.Log("[Config] Ссылка не задана. Играем с настройками по умолчанию.");
            _finished = true;
            return;
        }

        // Создаём запрос GET и запускаем. ВАЖНО: не ждём его здесь —
        // проверка завершения живёт в OnUpdate, кадры не «замирают»
        _request = UnityWebRequest.Get(ConfigLink.Url);
        _request.SendWebRequest();
    }

    protected override void OnUpdate()
    {
        // Нечего ждать — выходим мгновенно
        if (_finished || _request == null || !_request.isDone) return;

        if (_request.isNetworkError || _request.isHttpError)
        {
            // Сеть подвела — это не крах: играем с базовой скоростью из MoveData
            Debug.Log($"[Config] Не удалось скачать файл: {_request.error}");
        }
        else
        {
            // Сохраняем в системную папку — при следующем запуске шаг 3 пропустится
            File.WriteAllBytes(_filePath, _request.downloadHandler.data);

            string json = System.Text.Encoding.UTF8.GetString(_request.downloadHandler.data);
            ApplyJsonToEntity(json);
            Debug.Log($"[Config] Файл скачан и сохранён: {_filePath}");
        }

        // Запрос израсходован — освобождаем нативные ресурсы
        _request.Dispose();
        _request = null;
        _finished = true;
    }

    /// <summary>Читает JSON из системной папки и пишет данные в компонент.</summary>
    private void LoadFromFile()
    {
        try
        {
            string json = File.ReadAllText(_filePath);
            ApplyJsonToEntity(json);
            Debug.Log($"[Config] Файл загружен из системной папки: {_filePath}");
        }
        catch (Exception e)
        {
            // Повреждённый файл — не беда: играем с настройками по умолчанию
            Debug.Log($"[Config] Не удалось прочитать файл: {e.Message}");
        }
        _finished = true;
    }

    /// <summary>Разбирает JSON и записывает результат в PlayerConfigData игрока.</summary>
    private void ApplyJsonToEntity(string json)
    {
        // JsonUtility сопоставляет поля JSON с полями класса ПО ИМЕНАМ.
        // Класс должен повторять структуру файла: {"Speed": 8} → public float Speed
        var parsed = JsonUtility.FromJson<ConfigJson>(json);
        if (parsed == null)
        {
            Debug.Log("[Config] JSON не разобрался, играем с настройками по умолчанию.");
            return;
        }

        // Находим сущность игрока через запрос и записываем данные напрямую
        var entities = _configQuery.ToEntityArray(Allocator.TempJob);
        if (entities.Length > 0)
        {
            EntityManager.SetComponentData(entities[0], new PlayerConfigData
            {
                Speed = parsed.Speed,
                Loaded = true,
                Applied = false // применение сделает следующая система
            });
        }
        entities.Dispose();
    }

    /// <summary>Класс-«слепок» JSON-файла. Имена полей должны совпадать с файлом!</summary>
    [Serializable]
    private class ConfigJson
    {
        public float Speed;
    }

    protected override void OnDestroy()
    {
        // Если игра закрылась посреди скачивания — не оставляем утечку
        _request?.Dispose();
    }
}
