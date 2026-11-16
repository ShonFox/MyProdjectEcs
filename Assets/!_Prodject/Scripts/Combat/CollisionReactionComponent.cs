// Combat/CollisionReactionComponent.cs
using UnityEngine;

/// <summary>
/// Что происходит при столкновении (пуля, аптечка, перк-предмет).
/// Один скрипт на три случая — тип выбирается в инспекторе:
///   Damage        — пуля: нанести урон тому, во что попала;
///   Heal          — аптечка: вылечить подобравшего и исчезнуть;
///   RicochetPerk  — предмет перка: выдать отскок и исчезнуть.
///
/// Пуля летит с Rigidbody, поэтому её удары приходят в OnCollisionEnter.
/// Поднималки — триггеры, персонаж «входит» в них: OnTriggerEnter.
/// </summary>
public class CollisionReactionComponent : MonoBehaviour
{
    [Header("Настройка реакции")]
    [Tooltip("Что делает объект при столкновении.")]
    public ReactionType Reaction = ReactionType.Damage;

    [Tooltip("С каким тегом срабатывает реакция: у пули — Enemy, у аптечек и перков — Player.")]
    public string TargetTag = "Enemy";

    [Tooltip("Сила эффекта: сколько урона или лечения.")]
    public float Value = 20f;

    [HideInInspector] public GameObject Owner; // кто выпустил пулю (заполняет ShootAbility)

    // Пуля летит с Rigidbody — все её удары приходят сюда
    private void OnCollisionEnter(Collision collision)
    {
        // Реакция на удар нужна только пуле. У поднималок OnCollision не бывает
        if (Reaction != ReactionType.Damage) return;

        HandleBulletHit(collision);
    }

    // Поднималки: персонаж вошёл в зону
    private void OnTriggerEnter(Collider other)
    {
        // Поднимаемся от коллайдера к корню персонажа:
        // коллайдер может висеть на части тела, а тег и HealthComponent — на корне
        var target = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        // СВЕТОФОР: вошли ли мы вообще в зону и кого там встретили
        Debug.Log($"[Zone] {Reaction}: вошёл {target.name}, тег {target.tag}", this);

        if (!target.CompareTag(TargetTag)) return;

        switch (Reaction)
        {
            case ReactionType.Damage:
                // Зона урона: снимаем здоровье у того, кто вошёл
                var victim = target.GetComponent<HealthComponent>();
                if (victim != null)
                {
                    victim.ChangeHealth(-Value);
                }
                break;

            case ReactionType.Heal:
                var hp = other.GetComponent<HealthComponent>();
                if (hp != null)
                {
                    hp.ChangeHealth(+Value); // лечим
                    Destroy(gameObject);     // аптечка израсходована
                }
                break;

            case ReactionType.RicochetPerk:
                // Добавляем перк, только если его ещё нет — иначе будет дубль
                if (other.GetComponent<RicochetPerkComponent>() == null)
                    other.gameObject.AddComponent<RicochetPerkComponent>();
                Debug.Log("Перк отскока подобран!");
                Destroy(gameObject);
                break;
        }
    }

    /// <summary>Логика попадания пули.</summary>
    private void HandleBulletHit(Collision collision)
    {
        var hit = collision.gameObject;

        // Не стреляем в самого себя: пуля спавнится рядом с владельцем,
        // без этой проверки она может засчитать попадание в стрелка
        if (Owner != null && hit == Owner) return;

        // Попали в персонажа с нужным тегом — наносим урон и исчезаем
        if (hit.CompareTag(TargetTag))
        {
            var hp = hit.GetComponent<HealthComponent>();
            if (hp != null) hp.ChangeHealth(-Value);
            Destroy(gameObject);
            return;
        }

        // Попали во что-то другое (стена). Есть перк у владельца — отскакиваем
        bool hasRicochet = Owner != null &&
                           Owner.GetComponent<RicochetPerkComponent>() != null;
        if (!hasRicochet)
        {
            Destroy(gameObject);
            return;
        }

        // Reflect: отражаем вектор скорости от нормали поверхности —
        // так пуля летит под тем же углом, под которым влетела
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.velocity = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
    }
}

/// <summary>Тип реакции на столкновение. Описан рядом со скриптом для наглядности.</summary>
public enum ReactionType
{
    Damage,       // пуля: нанести урон тому, во что попала
    Heal,         // аптечка: вылечить подобравшего
    RicochetPerk  // перк: выдать отскок снарядов
}
