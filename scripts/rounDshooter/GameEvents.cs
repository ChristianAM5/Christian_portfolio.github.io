using UnityEngine;
using System;

/// <summary>
/// Bus de eventos estático. Centraliza la comunicación entre sistemas sin
/// que ninguno necesite referencia directa al otro.
///
/// USO:
///   Disparar: GameEvents.EnemyKilled();
///   Escuchar: GameEvents.OnEnemyKilled += MiMetodo;
///   Limpiar:  GameEvents.OnEnemyKilled -= MiMetodo;  ← siempre en OnDestroy
/// </summary>

public class GameEvents : MonoBehaviour
{
    // ── Combate ──────────────────────────────────────────────────────────────
    /// <summary>Se dispara cuando muere un enemigo. RoundManager lo usa para contar bajas.</summary>
    public static event Action OnEnemyKilled;
    public static void EnemyKilled() => OnEnemyKilled?.Invoke();
 
    // ── Rondas ───────────────────────────────────────────────────────────────
    /// <summary>Se dispara al inicio de cada ronda. Parámetro: número de ronda.</summary>
    public static event Action<int> OnRoundStarted;
    public static void RoundStarted(int round) => OnRoundStarted?.Invoke(round);
 
    /// <summary>Se dispara cuando se agotan los 30 segundos de ronda.</summary>
    public static event Action<int> OnRoundEnded;
    public static void RoundEnded(int round) => OnRoundEnded?.Invoke(round);
 
    // ── Armas ────────────────────────────────────────────────────────────────
    /// <summary>Se dispara al desbloquear un arma nueva. Parámetro: índice en el array de WeaponManager.</summary>
    public static event Action<int> OnWeaponUnlocked;
    public static void WeaponUnlocked(int index) => OnWeaponUnlocked?.Invoke(index);
 
    // ── Puntos de habilidad ───────────────────────────────────────────────────
    /// <summary>Se dispara cada vez que cambia el total de puntos de habilidad disponibles.</summary>
    public static event Action<int> OnSkillPointsChanged;
    public static void SkillPointsChanged(int total) => OnSkillPointsChanged?.Invoke(total);

    public static event Action OnPlayerDeath;
    public static void PlayerDeath() => OnPlayerDeath?.Invoke();
}
