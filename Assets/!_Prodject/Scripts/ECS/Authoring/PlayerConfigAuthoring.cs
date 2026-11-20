// Authoring/PlayerConfigAuthoring.cs — новый файл
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Статическое хранилище ссылки на конфиг. Нужно, потому что:
/// 1) строка не влезает в IComponentData (компоненты — только числа/структуры);
/// 2) сам объект-авторинг Unity может удалить после конвертации в сущность,
///    а статическое поле живёт всю сессию игры.
/// </summary>
public static class ConfigLink
{
    /// <summary>Прямая ссылка на JSON на Google Drive. Заполняет авторинг.</summary>
    public static string Url;
}

/// <summary>
/// Вешается на того же персонажа, где стоит CharacterAuthoring.
/// Добавляет компонент PlayerConfigData и запоминает ссылку на конфиг.
/// </summary>
public class PlayerConfigAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Прямая ссылка на файл player_config.json на Google Drive.")]
    [SerializeField] private string _configUrl = "";

    public void Convert(Entity entity, EntityManager dstManager,
        GameObjectConversionSystem conversionSystem)
    {
        // Ссылку отдаём в статическое хранилище — её заберёт система загрузки
        ConfigLink.Url = _configUrl;

        // Компонент с настройками: Speed = -1 значит «данных ещё нет».
        // Скорость по умолчанию останется из MoveData, пока файл не придёт
        dstManager.AddComponentData(entity, new PlayerConfigData
        {
            Speed = -1f,
            Loaded = false,
            Applied = false
        });
    }
}
