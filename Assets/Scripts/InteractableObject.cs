using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Interaktálható objektum komponens.
/// Tedd ezt rá minden objektumra amivel interaktálni akarsz (telefon, monitor, stb.)
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Interakció beállítások")]
    [Tooltip("Az objektum neve ami megjelenik amikor ránézel")]
    public string interactableName = "Objektum";

    [Tooltip("Az objektum leírása/használati útmutató")]
    public string interactHint = "Nyomj E-t vagy kattints";

    [Tooltip("Maximum távolság ahonnan interaktálhatsz vele")]
    public float interactionDistance = 3f;

    [Tooltip("Aktiválható-e jelenleg ez az objektum")]
    public bool isInteractable = true;

    [Header("Vizuális visszajelzés")]
    [Tooltip("Változzon-e a szín amikor ránézel")]
    public bool highlightOnLook = true;

    [Tooltip("Highlight szín")]
    public Color highlightColor = new Color(1f, 0.92f, 0.016f, 1f); // Sárga

    [Header("UI Mód")]
    [Tooltip("Ha igaz, az interakció automatikusan UI módba vált (kurzor megjelenik, kamera leáll). Hasznos monitorokhoz, input field-ekhez.")]
    public bool enterUIModeOnInteract = false;

    [Header("Események")]
    [Tooltip("Ez hívódik meg amikor aktiválod az objektumot")]
    public UnityEvent onInteract;

    // [ADDED v1.1.0] - Hatótávból kilépés esemény
    [Tooltip("Ez hívódik meg amikor elhagyod az interakciós hatótávot vagy elnézel az objektumról. Használd UI mód visszavonásához, animációk leállításához, stb.")]
    public UnityEvent onExitRange;

    // Privát változók
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isHighlighted = false;

    void Start()
    {
        // Próbáljuk megszerezni a Renderer komponenst a highlighting-hoz
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
    }

    /// <summary>
    /// Aktiválja az interakciót
    /// </summary>
    public void Interact()
    {
        if (!isInteractable) return;

        Debug.Log($"Interakció: {interactableName}");

        // Ha UI módba kell váltani, keressük meg a FirstPersonController-t
        if (enterUIModeOnInteract)
        {
            FirstPersonController fpc = FindAnyObjectByType<FirstPersonController>();
            if (fpc != null)
            {
                fpc.EnterUIMode();
            }
        }

        onInteract?.Invoke();
    }

    /// <summary>
    /// Kiemeli az objektumot amikor ránézel
    /// </summary>
    public void Highlight()
    {
        if (!highlightOnLook || isHighlighted || objectRenderer == null) return;

        objectRenderer.material.color = highlightColor;
        isHighlighted = true;
    }

    /// <summary>
    /// Eltávolítja a kiemelést amikor már nem nézel rá
    /// </summary>
    public void RemoveHighlight()
    {
        if (!isHighlighted || objectRenderer == null) return;

        objectRenderer.material.color = originalColor;
        isHighlighted = false;
    }

    // [ADDED v1.1.0] - Hatótávból kilépés
    /// <summary>
    /// Meghívódik amikor a játékos elhagyja az interakciós hatótávot vagy elnéz az objektumról.
    /// Invoke-olja az onExitRange eseményt.
    /// </summary>
    public void ExitRange()
    {
        Debug.Log($"[Interaction] Hatótáv elhagyva: {interactableName}"); // [ADDED v1.1.0] - Debug
        onExitRange?.Invoke();
    }

    /// <summary>
    /// Be/kikapcsolja az interaktálhatóságot
    /// </summary>
    public void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        if (!interactable)
        {
            RemoveHighlight();
        }
    }
}