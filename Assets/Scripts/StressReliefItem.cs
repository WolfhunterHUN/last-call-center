using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Stressz csökkentő tárgy (energiaital, cigaretta, kávé, stb.)
/// Az InteractableObject onInteract eventjéhez kösd a UseItem() metódust.
/// 
/// Setup:
/// 1. Hozd létre a 3D objektumot (pl. energiaital doboz)
/// 2. Adj hozzá Collider-t és InteractableObject-et
/// 3. Tedd rá ezt a scriptet
/// 4. Az InteractableObject onInteract -> StressReliefItem.UseItem()
/// 5. Állítsd be a stressReduction értéket és a használatok számát
/// </summary>
public class StressReliefItem : MonoBehaviour
{
    [Header("Stressz csökkentés")]
    [Tooltip("Ennyi stresszt csökkent használatkor")]
    [SerializeField] private float stressReduction = 15f;

    [Tooltip("Hányszor használható (-1 = végtelen)")]
    [SerializeField] private int maxUses = 1;

    [Tooltip("Eltűnjön-e az objektum az utolsó használat után")]
    [SerializeField] private bool destroyOnEmpty = true;

    [Tooltip("Hány másodperc cooldown két használat között")]
    [SerializeField] private float cooldownTime = 0f;

    [Header("Vizuális visszajelzés")]
    [Tooltip("Opcionális particle effect ami lejátszódik használatkor")]
    [SerializeField] private ParticleSystem useEffect;

    [Tooltip("Opcionális hang ami lejátszódik használatkor")]
    [SerializeField] private AudioClip useSound;

    [Header("Események")]
    [Tooltip("Sikeres használatkor meghívódik")]
    public UnityEvent onItemUsed;

    [Tooltip("Amikor elfogy a tárgy")]
    public UnityEvent onItemEmpty;

    // Belső állapot
    private int remainingUses;
    private float lastUseTime = -999f;
    private AudioSource audioSource;

    /// <summary>
    /// Hátralévő használatok száma (-1 = végtelen)
    /// </summary>
    public int RemainingUses => remainingUses;

    /// <summary>
    /// Használható-e jelenleg
    /// </summary>
    public bool CanUse => (maxUses < 0 || remainingUses > 0) && Time.time >= lastUseTime + cooldownTime;

    private void Awake()
    {
        remainingUses = maxUses;

        // AudioSource keresése vagy létrehozás
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && useSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D hang
        }
    }

    // ============================================================
    // Publikus metódus — kösd az InteractableObject onInteract-hoz
    // ============================================================

    /// <summary>
    /// Használja a tárgyat és csökkenti a stresszt.
    /// Kösd ezt az InteractableObject onInteract() eseményéhez.
    /// </summary>
    public void UseItem()
    {
        if (!CanUse)
        {
            if (remainingUses == 0)
            {
                Debug.Log($"[StressRelief] {gameObject.name}: elfogyott!");
            }
            else
            {
                Debug.Log($"[StressRelief] {gameObject.name}: cooldown ({cooldownTime - (Time.time - lastUseTime):F1}s hátra)");
            }
            return;
        }

        // StressManager keresése
        StressManager stressManager = StressManager.Instance;
        if (stressManager == null)
        {
            Debug.LogError("[StressRelief] StressManager nem található!");
            return;
        }

        if (stressManager.IsGameOver)
        {
            Debug.Log("[StressRelief] Game Over, nem használható.");
            return;
        }

        // Stressz csökkentés
        stressManager.ReduceStress(stressReduction);
        lastUseTime = Time.time;
        Debug.Log($"[StressRelief] {gameObject.name} használva! -{stressReduction} stressz (maradék: {stressManager.CurrentStress:F0})");

        // Használatok csökkentése
        if (maxUses > 0)
        {
            remainingUses--;
        }

        // Effektek
        PlayEffects();

        // Event
        onItemUsed?.Invoke();

        // Elfogyott?
        if (maxUses > 0 && remainingUses <= 0)
        {
            Debug.Log($"[StressRelief] {gameObject.name} elfogyott!");
            onItemEmpty?.Invoke();

            // InteractableObject letiltása ha van
            InteractableObject interactable = GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.SetInteractable(false);
                interactable.interactHint = "Elfogyott";
            }

            if (destroyOnEmpty)
            {
                // Kis késleltetés hogy az effektek még lejátsszák
                Destroy(gameObject, useEffect != null ? useEffect.main.duration : 0.5f);
            }
        }
        else if (maxUses > 0)
        {
            // Hint frissítése a maradék használatokkal
            InteractableObject interactable = GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.interactHint = $"Használd ({remainingUses} maradt)";
            }
        }
    }

    // ============================================================
    // Effektek
    // ============================================================

    private void PlayEffects()
    {
        // Particle effect
        if (useEffect != null)
        {
            useEffect.Play();
        }

        // Hang
        if (useSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(useSound);
        }
    }

    // ============================================================
    // Segédmetódusok
    // ============================================================

    /// <summary>
    /// Feltölti a tárgyat az eredeti használatok számára
    /// </summary>
    public void Refill()
    {
        remainingUses = maxUses;
        
        InteractableObject interactable = GetComponent<InteractableObject>();
        if (interactable != null)
        {
            interactable.SetInteractable(true);
            interactable.interactHint = $"Használd ({remainingUses} maradt)";
        }

        Debug.Log($"[StressRelief] {gameObject.name} feltöltve: {remainingUses} használat");
    }
}
