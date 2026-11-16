// Authoring/AbilitiesAuthoring.cs
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Вешается на персонажа и собирает все способности (MonoBehaviour : IAbility),
/// которые перетащили в инспектор, в один компонент AbilitiesData.
/// Новая способность = повесить скрипт на персонажа и перетащить его в этот список.
/// Ни одной новой системы для этого писать не нужно.
/// </summary>
public class AbilitiesAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("Перетащите сюда объекты со скриптами-способностями (реализуют IAbility).")]
    [SerializeField] private MonoBehaviour[] _abilities;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new AbilitiesData();

        // Отбираем только те компоненты, которые реализуют IAbility.
        // Если случайно перетащили что-то другое — просто пропустим
        foreach (var component in _abilities)
        {
            if (component is IAbility ability)
                data.Add(ability);
        }

        // AbilitiesData — managed-компонент (класс), поэтому AddComponentObject
        dstManager.AddComponentObject(entity, data);
    }
}
