using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Központi stressz kezelõ rendszer (Singleton).
/// Minden NeocortexScoreTracker ide küldi a stressz változásokat.
/// Kezeli a Game Over-t ha a stressz eléri a 100-at.
/// 
/// Setup:
/// 1. Hozz létre egy üres GameObject-et a scene-ben: "StressManager"
/// 2. Tedd rá ezt a scriptet
/// 3. A StressBarUI script automatikusan megtalálja
/// 4. A NeocortexScoreTracker-ek automatikusan megtalálják
/// </summary>
public class StressManager : MonoBehaviour
{
    // ============================================================
    // Singleton
    // ============================================================

    public static StressManager Instance { get; private set; }

    [Header("Stressz beállítások")]
    [Tooltip("Kezdõ stressz szint (0-100)")]
    [SerializeField] private float startingStress = 0f;

    [Tooltip("Maximum stressz — ennél Game Over")]
    [SerializeField] private float maxStress = 100f;

    [Tooltip("Stressz szint ami felett a bar piros lesz (veszélyzóna)")]
    [SerializeField] private float dangerThreshold = 80f;

    [Header("Események")]
    [Tooltip("Minden stressz változáskor — paraméter: aktuális stressz (0-100)")]
    public UnityEvent<float> onStressChanged;

    [Tooltip("Amikor a stressz eléri a veszélyzónát (dangerThreshold felett)")]
    public UnityEvent onDangerZoneEntered;

    [Tooltip("Amikor a stressz visszacsökken a veszélyzóna alá")]
    public UnityEvent onDangerZoneExited;

    [Tooltip("Game Over — stressz elérte a maximumot")]
    public UnityEvent onGameOver;

    /// <summary>
    /// Aktuális stressz szint (0-100)
    /// </summary>
    public float CurrentStress { get; private set; }

    /// <summary>
    /// Maximum stressz érték
    /// </summary>
    public float MaxStress => maxStress;

    /// <summary>
    /// Veszélyzóna küszöb
    /// </summary>
    public float DangerThreshold => dangerThreshold;

    /// <summary>
    /// Game Over megtörtént-e már
    /// </summary>
    public bool IsGameOver { get; private set; }

    /// <summary>
    /// Veszélyzónában vagyunk-e
    /// </summary>
    public bool IsInDangerZone => CurrentStress >= dangerThreshold;

    // Belsõ állapot
    private bool wasInDangerZone = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[StressManager] Duplikált StressManager! Csak egy lehet a scene-ben.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        CurrentStress = startingStress;
        IsGameOver = false;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // ============================================================
    // Publikus metódusok — ezeket hívják a tracker-ek és item-ek
    // ============================================================

    /// <summary>
    /// Stressz növelése. Ha eléri a max-ot, Game Over.
    /// </summary>
    public void AddStress(float amount)
    {
        if (IsGameOver) return;

        float previous = CurrentStress;
        CurrentStress = Mathf.Clamp(CurrentStress + amount, 0f, maxStress);

        if (CurrentStress != previous)
        {
            onStressChanged?.Invoke(CurrentStress);
            CheckDangerZone();
        }

        // Game Over check
        if (CurrentStress >= maxStress)
        {
            TriggerGameOver();
        }

        Debug.Log($"[StressManager] Stressz: {CurrentStress:F0}/{maxStress:F0} (+{amount:F0})");
    }

    /// <summary>
    /// Stressz csökkentése (energiaital, cigaretta, POSITIVE válasz, stb.)
    /// </summary>
    public void ReduceStress(float amount)
    {
        if (IsGameOver) return;

        float previous = CurrentStress;
        CurrentStress = Mathf.Clamp(CurrentStress - amount, 0f, maxStress);

        if (CurrentStress != previous)
        {
            onStressChanged?.Invoke(CurrentStress);
            CheckDangerZone();
        }

        Debug.Log($"[StressManager] Stressz: {CurrentStress:F0}/{maxStress:F0} (-{amount:F0})");
    }

    /// <summary>
    /// Stressz teljes resetelése
    /// </summary>
    public void ResetStress()
    {
        CurrentStress = startingStress;
        IsGameOver = false;
        wasInDangerZone = false;
        onStressChanged?.Invoke(CurrentStress);
        Debug.Log($"[StressManager] Stressz resetelve: {CurrentStress:F0}");
    }

    // ============================================================
    // Belsõ logika
    // ============================================================

    private void CheckDangerZone()
    {
        bool inDanger = IsInDangerZone;

        if (inDanger && !wasInDangerZone)
        {
            onDangerZoneEntered?.Invoke();
            Debug.Log("[StressManager] VESZÉLYZÓNA!");
        }
        else if (!inDanger && wasInDangerZone)
        {
            onDangerZoneExited?.Invoke();
            Debug.Log("[StressManager] Veszélyzóna elhagyva.");
        }

        wasInDangerZone = inDanger;
    }

    private void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        onGameOver?.Invoke();
        Debug.LogWarning("[StressManager] === GAME OVER === Stressz elérte a maximumot!");
    }
}