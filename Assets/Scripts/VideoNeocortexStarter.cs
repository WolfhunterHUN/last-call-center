using Neocortex;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Neocortex AI ügynök indító, véletlenszerû prompt választással.
/// Az InteractableObject onInteract eseményéhez kösd a StartConversation() metódust.
/// Minden interakció egy új, még nem használt promptot választ a listából.
/// Ha elfogynak a promptok, a session resetelõdik.
/// </summary>
public class VideoNeocortexStarter : MonoBehaviour
{
    [SerializeField] private NeocortexSmartAgent smartAgent;
    [Header("Prompt beállítások")]
    [Tooltip("Hány másodperc késleltetés az interakció után mielõtt elküldi a promptot")]
    [SerializeField] private float sendDelay = 1f;

    // ============================================================
    // Prompt rendszer - véletlenszerû, ismétlés nélküli választás
    // ============================================================

    /// <summary>
    /// Az összes elérhetõ prompt név + forgatókönyv.
    /// A prompt sablon: "You are [NAME]... give your situation to the customer service operator..."
    /// </summary>
    private readonly string[] allPromptNames = new string[]
    {
        "Alex Static",
        "River Coil",
        "Sam Nightshift",
        "Rowan Hale",
        "Taylor Lockwood",
        "Casey Forklift",
        "Quinn Patch",
        "Morgan Shield",
        "Jules Backup",
        "SOMEONE",
        "Jordan Payroll",
        "Avery Postmark",
        "Casey Cradle",
        "Morgan Headset",
        "Riley Beacon",
        "Alex Duplicate",
        "Quinn Lockdown",
        "Rowan Presspass",
        "Jules Archive"
    };

    /// <summary>
    /// A prompt sablon. A {0} helyére kerül a választott név.
    /// </summary>
    private const string PROMPT_TEMPLATE =
        "You are \"{0}\" give your situation to the customer service operator, " +
        "so he can help you with the problem. Tell them your goal too what you want. " +
        "Stay in character, you are asking for help, don't let it go unless you get negative responses.";

    /// <summary>
    /// A még fel nem használt promptok indexei ebben a sessionben.
    /// Ha kiürül, újratöltõdik (reshuffle).
    /// </summary>
    private List<int> availableIndices = new List<int>();

    private void Awake()
    {
        // Induláskor feltöltjük az elérhetõ indexeket
        ResetAvailablePrompts();
    }

    private void Start()
    {
        // Get the component if not assigned
        if (smartAgent == null)
        {
            smartAgent = GetComponent<NeocortexSmartAgent>();
        }
    }

    // ============================================================
    // Publikus metódusok - ezeket kösd az InteractableObject onInteract-hoz
    // ============================================================

    /// <summary>
    /// Indít egy új beszélgetést véletlenszerû prompttal.
    /// Kösd ezt az InteractableObject onInteract eseményéhez.
    /// </summary>
    public void StartConversation()
    {
        if (smartAgent == null)
        {
            Debug.LogError("[VideoNeocortexStarter] NeocortexSmartAgent nincs hozzárendelve!");
            return;
        }

        Invoke(nameof(SendRandomPrompt), sendDelay);
    }


    // ============================================================
    // Belsõ logika
    // ============================================================

    /// <summary>
    /// Véletlenszerû, még nem használt promptot választ és elküldi a Neocortex agentnek.
    /// </summary>
    private void SendRandomPrompt()
    {
        // Ha elfogytak a promptok, újratöltjük
        if (availableIndices.Count == 0)
        {
            Debug.Log("[VideoNeocortexStarter] Minden prompt felhasználva, session reset...");
            ResetAvailablePrompts();
        }

        // Véletlenszerû index választás a maradékból
        int randomPick = Random.Range(0, availableIndices.Count);
        int chosenIndex = availableIndices[randomPick];
        availableIndices.RemoveAt(randomPick);

        // Prompt összeállítása
        string chosenName = allPromptNames[chosenIndex];
        string prompt = string.Format(PROMPT_TEMPLATE, chosenName);

        Debug.Log($"[VideoNeocortexStarter] Prompt küldve ({availableIndices.Count} maradt): {chosenName}");
        smartAgent.TextToAudio(prompt);
    }

    /// <summary>
    /// Újratölti az elérhetõ promptok listáját (az összes index elérhetõ lesz).
    /// </summary>
    private void ResetAvailablePrompts()
    {
        availableIndices.Clear();
        for (int i = 0; i < allPromptNames.Length; i++)
        {
            availableIndices.Add(i);
        }
        Debug.Log($"[VideoNeocortexStarter] Prompt pool resetelve: {allPromptNames.Length} prompt elérhetõ");
    }
}