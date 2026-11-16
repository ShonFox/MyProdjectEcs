// Abilities/ShootAbility.cs
using UnityEngine;

/// <summary>
/// Способность «Стрельба».
/// Поведение по твоему заданию:
///   - короткое нажатие        -> ОДНА пуля сразу;
///   - зажатая кнопка          -> пули вылетают с задержкой ShootDelay.
///
/// Вся логика живёт здесь, а не в отдельной ECS-системе:
/// ability-классу нужны инспектор и Instantiate — в MonoBehaviour это удобнее.
/// ECS-система (AbilitySystem) просто передаёт способности кнопку.
/// </summary>
public class ShootAbility : MonoBehaviour, IAbility
{
    [Header("Пуля")]
    [Tooltip("Префаб пули (сфера с Rigidbody и CollisionReactionComponent).")]
    [SerializeField] private GameObject _bullet;

    [Tooltip("Скорость полёта пули, юнитов в секунду.")]
    [SerializeField] private float _bulletSpeed = 10f;

    [Tooltip("Сколько секунд живёт пуля, потом уничтожается — иначе сцена забьётся.")]
    [SerializeField] private float _bulletLifetime = 5f;

    [Header("Темп стрельбы")]
    [Tooltip("Задержка между пулями при ЗАЖАТОЙ кнопке, секунд.")]
    [SerializeField] private float _shootDelay = 0.5f;

    [Tooltip("Насколько далеко перед персонажем спавнить пулю (чтобы не попасть в себя).")]
    [SerializeField] private float _spawnOffset = 0.7f;

    [Tooltip("На какой высоте над ногами спавнить пул. 1.5 — примерно уровень груди.")]
    [SerializeField] private float _spawnHeight = 1.5f;

    // Время последнего выстрела. float.MinValue — чтобы можно было стрелять сразу после старта
    private float _lastShotTime = float.MinValue;

    // Была ли кнопка зажата в ПРОШЛОМ кадре. По разнице с текущим кадром
    // понимаем: игрок только что нажал кнопку или уже держит её
    private bool _wasPressedLastFrame;

    // Вызывается AbilitySystem каждый кадр.
    // isPressed: true — кнопка зажата сейчас, false — отпущена
    public void Execute(bool isPressed)
    {
        // Кнопку отпустили — сбрасываем состояние,
        // чтобы СЛЕДУЮЩЕЕ нажатие снова считалось «первым кадром»
        if (!isPressed)
        {
            _wasPressedLastFrame = false;
            return;
        }

        // Только что нажали? Тогда это первый кадр нажатия
        bool firstFrameOfPress = !_wasPressedLastFrame;
        _wasPressedLastFrame = true;

        if (firstFrameOfPress)
        {
            // Однократное нажатие — стреляем мгновенно и запоминаем время
            Shoot();
            _lastShotTime = Time.time;
            return;
        }

        // Кнопка держится — стреляем только когда прошла задержка
        if (Time.time >= _lastShotTime + _shootDelay)
        {
            Shoot();
            _lastShotTime = Time.time;
        }
    }

    /// <summary>Спавнит пулю перед персонажем и задаёт ей скорость.</summary>
    private void Shoot()
    {

        if (_bullet == null)
        {
            Debug.LogError("[ShootAbility] Не задан префаб пули", this);
            return;
        }

        var self = transform;

        // Спавним пулю чуть впереди персонажа, по направлению взгляда
        var spawnPos = self.position + self.forward * _spawnOffset + Vector3.up * _spawnHeight;
        var bullet = Instantiate(_bullet, spawnPos, self.rotation);

        // Задаём пуле скорость по направлению взгляда.
        // В Unity 6+ поле называется linearVelocity — на 2021.3 работает velocity
        var rigidbody = bullet.GetComponent<Rigidbody>();
        if (rigidbody != null)
            rigidbody.velocity = self.forward * _bulletSpeed;

        // Передаём пуле владельца — CollisionReactionComponent проверит по нему
        // перк отскока и не даст пуле попасть в самого стрелка
        var reaction = bullet.GetComponent<CollisionReactionComponent>();
        if (reaction != null)
            reaction.Owner = gameObject;

        // Уничтожаем пулю через заданное время
        Destroy(bullet, _bulletLifetime);
    }
}
