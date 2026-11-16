// Systems/ThirdPersonCameraSystem.cs
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Follow-камера третьего лица: стоит за спиной персонажа и смотрит на него.
/// Как работает: персонаж поворачивается от стика движения (это делает
/// CharacterMoveSystem), а камера каждый кадр плавно «догоняет» его спину.
/// Чем меньше FollowSpeed — тем заметнее камера отстаёт на поворотах.
/// Здесь НЕТ ForEach: данные достаём из запроса напрямую.
/// Для одного игрока это проще и надёжнее — кроме того, ForEach в этой
/// версии Entities капризничает с параметрами, а нам его заменитель
/// как раз и нужен.
/// </summary>
public class ThirdPersonCameraSystem : ComponentSystem
{
    private EntityQuery _lookQuery;

    // Сглаженный горизонтальный угол камеры. Храним в системе, потому что
    // он нужен только камере и переживает кадры — плавность именно тут
    private float _smoothYaw;

    // Привязалась ли камера к персонажу. Флаг надёжнее сравнения с нулём:
    // угол 0 — это тоже легальный поворот персонажа
    private bool _isInitialized;

    protected override void OnCreate()
    {
        // Запрос: сущности, у которых есть настройки камеры и Transform
        // (Transform — гибридный компонент, Unity его создаёт сама)
        _lookQuery = GetEntityQuery(
            ComponentType.ReadOnly<LookData>(),
            ComponentType.ReadOnly<Transform>());
    }

    protected override void OnUpdate()
    {
        var camera = Camera.main;
        if (camera == null) return;

        // Достаём из запроса списки сущностей и их данных.
        // TempJob — временная память на этот кадр, в конце вернём
        var entities = _lookQuery.ToEntityArray(Allocator.TempJob);

        if (entities.Length == 0)
        {
            // Сущностей нет (сцена пустая) — только вернём память и выйдем
            entities.Dispose();
            return;
        }

        var lookData = _lookQuery.ToComponentDataArray<LookData>(Allocator.TempJob);
        var transforms = _lookQuery.ToComponentArray<Transform>();

        // Игрок у нас один — берём первую сущность запроса.
        // (Когда появятся два игрока — пройдём циклом по всем)
        var transform = transforms[0];
        var look = lookData[0];

        // Куда повёрнут персонаж сейчас, в градусах по оси Y
        float characterYaw = transform.eulerAngles.y;

        // Первый кадр: мгновенно встаём за спину, чтобы камера
        // не «протаранивала» сцену, закручиваясь с нуля до персонажа
        if (!_isInitialized)
        {
            _smoothYaw = characterYaw;
            _isInitialized = true;
        }

        // Плавно подтягиваем угол камеры к углу персонажа.
        // LerpAngle понимает переход через 360 градусов (359 -> 1
        // пойдёт коротким путём, а не через всю окружность)
        _smoothYaw = Mathf.LerpAngle(
            _smoothYaw, characterYaw, look.FollowSpeed * Time.DeltaTime);

        // Точка, на которую смотрим: центр персонажа + высота взгляда
        Vector3 focusPoint = transform.position + Vector3.up * look.Height;

        // LookRotation сам наклонит камеру вниз, потому что она теперь
        // выше точки, на которую смотрит
        Vector3 offset = Quaternion.Euler(0f, _smoothYaw, 0f)
                         * Vector3.back * look.Distance
                         + Vector3.up * look.Elevation;

        camera.transform.position = focusPoint + offset;

        // Смотрим из точки камеры в точку фокуса
        camera.transform.rotation = Quaternion.LookRotation(
            focusPoint - camera.transform.position);

        // Вернули временные массивы — без Dispose память утекёт
        entities.Dispose();
        lookData.Dispose();
    }
}
