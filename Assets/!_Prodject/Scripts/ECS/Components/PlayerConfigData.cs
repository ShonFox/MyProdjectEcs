// Components/PlayerConfigData.cs — новый файл
using Unity.Entities;

/// <summary>
/// Данные игрока, которые приходят из файла JSON (Google Drive или системная папка).
/// Пока считаем, что компонент есть только у игрока — вешается авторингом.
/// </summary>
public struct PlayerConfigData : IComponentData
{
    /// <summary>
    /// Параметр из JSON: скорость игрока.
    /// Значение -1 означает «данные ещё не пришли, играем с базовой скоростью».
    /// </summary>
    public float Speed;

    /// <summary>Файл прочитан и Speed заполнен? Системы смотрят на этот флаг.</summary>
    public bool Loaded;

    /// <summary>Скорость уже перенесена в MoveData? Защита от повторного применения.</summary>
    public bool Applied;
}
