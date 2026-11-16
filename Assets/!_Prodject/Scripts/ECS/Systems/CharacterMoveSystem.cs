// Systems/CharacterMoveSystem.cs
using Unity.Entities;
using UnityEngine;

/// <summary>
/// Движение в третьем лице.
/// Направление считается ОТНОСИТЕЛЬНО КАМЕРЫ: толкнул стик вправо —
/// персонаж идёт вправо НА ЭКРАНЕ. Камера следом догоняет его за спину,
/// поэтому удержание «вправо» даёт движение по кругу — это норма.
/// Персонаж плавно поворачивается в сторону движения.
/// </summary>
public class CharacterMoveSystem : ComponentSystem
{
    private EntityQuery _moveQuery;

    protected override void OnCreate()
    {
        _moveQuery = GetEntityQuery(
            ComponentType.ReadOnly<InputData>(),
            ComponentType.ReadOnly<MoveData>(),
            ComponentType.ReadOnly<Transform>());
    }

    protected override void OnUpdate()
    {
        // Камера нужна как «система координат» движения
        var camera = Camera.main;
        if (camera == null) return;

        // Оси камеры, спроецированные на горизонталь (обнулили Y и нормализовали).
        // Без этого при наклонённой камере персонаж полз бы «в землю»/«в небо»
        Vector3 camForward = camera.transform.forward;
        camForward.y = 0f;
        camForward.Normalize();

        Vector3 camRight = camera.transform.right;
        camRight.y = 0f;
        camRight.Normalize();

        Entities.With(_moveQuery).ForEach(
            (Transform transform, ref InputData input, ref MoveData move) =>
            {
                // КЛЮЧЕВОЕ: направление от камеры, а не от осей мира.
                // «Вперёд» = от камеры вглубь экрана, «вправо» = вправо на экране
                Vector3 dir = camForward * input.Move.y + camRight * input.Move.x;

                // Мёртвая зона: гасим микросдвиги стика
                if (dir.sqrMagnitude < 0.0001f) return;
                dir.Normalize();

                var step = dir * (move.Speed * Time.DeltaTime);

                // --- ПРОВЕРКА СТЕН (SphereCast, как раньше) ---
                Vector3 center = transform.position + Vector3.up * 1f;
                float distance = step.magnitude + 0.4f;

                if (Physics.SphereCast(center, 0.4f, dir, out var hit, distance,
                        ~0, QueryTriggerInteraction.Ignore))
                {
                    float allowed = Mathf.Max(0f, hit.distance - 0.4f - 0.05f);
                    step = dir * Mathf.Min(step.magnitude, allowed);
                }

                var pos = transform.position;
                pos += step;
                transform.position = pos;

                // Поворачиваемся в сторону движения (10 — резкость поворота)
                var targetRotation = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, targetRotation, Time.DeltaTime * 10f);
            });
    }
}
