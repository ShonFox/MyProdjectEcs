// Authoring/CharacterAuthoring.cs
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Вешается на префаб/объект персонажа.
/// При старте сцены Unity «конвертирует» GameObject в сущность ECS
/// и вешает на неё компоненты. Всё, что настроено в инспекторе,
/// попадает в компоненты здесь, в методе Convert.
/// Способности настраиваются отдельно — через AbilitiesAuthoring.
/// </summary>
public class CharacterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Header("Движение")]
    [Tooltip("Базовая скорость, юнитов в секунду. Делить ни на что не нужно.")]
    [SerializeField] private float _baseSpeed = 5f;

    [Header("Спринт")]
    [Tooltip("Во сколько раз быстрее персонаж при зажатом спринте.")]
    [SerializeField] private float _sprintMultiplier = 2f;

    [Header("Камера (follow, за спиной)")]
    [Tooltip("Расстояние камеры от персонажа, юниты.")]
    [SerializeField] private float _cameraDistance = 5f;

    [Tooltip("Высота точки обзора, юниты.")]
    [SerializeField] private float _cameraHeight = 1.5f;

    [Tooltip("Скорость догоняния поворота персонажа. Больше — камера ближе к спине.")]
    [SerializeField] private float _cameraFollowSpeed = 5f;


    [Tooltip("Подъём камеры над точкой обзора. 0 — горизонтально, 2 — сверху.")]
    [SerializeField] private float _cameraElevation = 1.5f;

    // Unity вызывает этот метод при создании сущности из GameObject
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        // InputData — по сути маркер «у этой сущности есть ввод игрока»
        dstManager.AddComponentData(entity, new InputData());

        // Движение: актуальная скорость = базовая.
        // Спринт-система сама перепишет Speed, если понадобится
        dstManager.AddComponentData(entity, new MoveData
        {
            BaseSpeed = _baseSpeed,
            Speed = _baseSpeed
        });

        dstManager.AddComponentData(entity, new LookData
        {
            Distance = _cameraDistance,
            Height = _cameraHeight,
            FollowSpeed = _cameraFollowSpeed,
            Elevation = _cameraElevation
        });


        // Спринт. Уберёшь эту строку — персонаж не сможет спринтовать,
        // но всё остальное продолжит работать
        dstManager.AddComponentData(entity, new SprintData { Multiplier = _sprintMultiplier });
    }
}
