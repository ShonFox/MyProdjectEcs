// Perks/RicochetPerkComponent.cs
using UnityEngine;

/// <summary>
/// Перк «отскок пуль от стен».
/// Это просто маркер: внутри нет логики. Пуля спрашивает у владельца
/// «есть ли у тебя RicochetPerkComponent?» — и если есть, отскакивает
/// (см. CollisionReactionComponent). Выдаётся подбором предмета с ReactionType.RicochetPerk.
/// </summary>
public class RicochetPerkComponent : MonoBehaviour
{
    // Маркер перка. Логики нет — вся логика на стороне пули.
    // Такой подход хорош: пуля ничего не знает о «перках» вообще,
    // она просто проверяет наличие компонента у владельца
}
