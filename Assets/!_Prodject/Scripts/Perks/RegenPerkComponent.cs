// Perks/RegenPerkComponent.cs
using UnityEngine;

/// <summary>
/// Перк «восстановление здоровья».
/// После подбора (CollisionReactionComponent добавляет его персонажу)
/// здоровье капает само, по HealthPerSecond каждую секунду.
/// Лечение идёт через HealthComponent.ChangeHealth — единая точка правды:
/// перк не может вылечить выше максимума и не трогает здоровье напрямую.
/// </summary>
public class RegenPerkComponent : MonoBehaviour
{
    [Tooltip("Сколько здоровья восстанавливается в секунду.")]
    public float HealthPerSecond = 5f;

    // Ссылку на HealthComponent находим один раз — каждый кадр GetComponent дорого
    private HealthComponent _health;

    private void Awake()
    {
        // Перк висит на том же объекте, что и HealthComponent
        _health = GetComponent<HealthComponent>();
    }

    private void Update()
    {
        if (_health == null) return;

        // Умножаем на Time.deltaTime: столько здоровья «капает» за этот кадр
        _health.ChangeHealth(HealthPerSecond * Time.deltaTime);
    }
}
