// Combat/HealthComponent.cs
using UnityEngine;

/// <summary>
/// Здоровье персонажа. Обычный MonoBehaviour, не ECS:
/// к нему обращаются пули, аптечки, перки и зоны урона — классическая логика тут проще.
/// Вешается и на игрока, и на врагов.
/// </summary>
public class HealthComponent : MonoBehaviour
{
    [Tooltip("Максимальное здоровье.")]
    public float MaxHealth = 100f;

    [Tooltip("Текущее здоровье. Заполняется автоматически при старте.")]
    public float CurrentHealth;

    [Tooltip("Уничтожать ли объект при нуле здоровья (для пуль и врагов — да).")]
    public bool DestroyOnZero = true;

    private void Awake()
    {
        // Стартуем с полного здоровья
        CurrentHealth = MaxHealth;
    }

    /// <summary>
    /// Единая точка изменения здоровья: и урон, и лечение идут через неё.
    /// amount > 0 — лечение, amount < 0 — урон.
    /// Clamp не даёт уйти ниже нуля или выше максимума.
    /// </summary>
    public void ChangeHealth(float amount)
    {
        
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0f, MaxHealth);
        Debug.Log($"HP: {CurrentHealth}/{MaxHealth}");
        // Здоровье кончилось — уничтожаем объект (игрока или врага)
        if (CurrentHealth <= 0f && DestroyOnZero)
            Destroy(gameObject);
    }
}
