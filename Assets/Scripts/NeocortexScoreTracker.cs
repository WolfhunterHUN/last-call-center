using Neocortex;
using Neocortex.API;
using Neocortex.Data;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Neocortex válaszok alapján pontszámot kezelõ komponens.
/// Tedd ezt arra a GameObject-re ahol a NeocortexSmartAgent van.
/// Figyeli a ChatResponse action mezõjét:
///   - "POSITIVE" -> pontszám nõ
///   - "NEGATIVE" -> pontszám csökken
/// A pontszám csak az aktuális session-ben él (scene reload reseteli).
/// </summary>
public class NeocortexScoreTracker : MonoBehaviour
{
    [Header("Referenciák")]
    [Tooltip("A NeocortexSmartAgent amit figyelünk. Ha üres, a saját GameObject-en keresi.")]
    [SerializeField] private NeocortexSmartAgent smartAgent;

    [Header("Pontszám beállítások")]
    [Tooltip("Kezdõ pontszám")]
    [SerializeField] private int startingScore = 0;

    [Tooltip("Ennyi pontot kap POSITIVE action-re")]
    [SerializeField] private int positivePoints = 1;

    [Tooltip("Ennyi pontot veszít NEGATIVE action-re")]
    [SerializeField] private int negativePoints = 1;

    [Tooltip("Minimum pontszám (nem mehet ez alá)")]
    [SerializeField] private int minScore = 0;

    [Tooltip("Maximum pontszám (nem mehet ez fölé). 0 = nincs limit.")]
    [SerializeField] private int maxScore = 0;

    [Header("Események")]
    [Tooltip("Minden pontszám változáskor meghívódik az aktuális pontszámmal")]
    public UnityEvent<int> onScoreChanged;

    [Tooltip("POSITIVE action érkezésekor meghívódik")]
    public UnityEvent onPositiveReceived;

    [Tooltip("NEGATIVE action érkezésekor meghívódik")]
    public UnityEvent onNegativeReceived;

    /// <summary>
    /// Az aktuális pontszám. Csak olvasható kívülrõl.
    /// </summary>
    public int CurrentScore { get; private set; }

    /// <summary>
    /// Összesen hány POSITIVE action érkezett ebben a sessionben.
    /// </summary>
    public int TotalPositives { get; private set; }

    /// <summary>
    /// Összesen hány NEGATIVE action érkezett ebben a sessionben.
    /// </summary>
    public int TotalNegatives { get; private set; }

    // Belsõ referencia az ApiRequest-re a feliratkozáshoz/leiratkozáshoz
    private ApiRequest apiRequest;

    private void Awake()
    {
        CurrentScore = startingScore;
        TotalPositives = 0;
        TotalNegatives = 0;
    }

    private void Start()
    {
        // SmartAgent keresése ha nincs megadva
        if (smartAgent == null)
        {
            smartAgent = GetComponent<NeocortexSmartAgent>();
        }

        if (smartAgent == null)
        {
            Debug.LogError("[ScoreTracker] NeocortexSmartAgent nem található! Tedd ezt a scriptet a SmartAgent GameObject-jére.");
            return;
        }

        // Feliratkozás a ChatResponse eseményre
        SubscribeToChatResponse();
    }

    private void OnDestroy()
    {
        // Leiratkozás a memória szivárgás elkerülése érdekében
        UnsubscribeFromChatResponse();
    }

    // ============================================================
    // Feliratkozás a Neocortex ChatResponse eseményre
    // ============================================================

    /// <summary>
    /// Feliratkozik az ApiRequest OnChatResponseReceived eseményére.
    /// A NeocortexSmartAgent belsõleg ApiRequest-et használ,
    /// ezért reflection-nel vagy GetComponentInChildren-nel keressük.
    /// </summary>
    private void SubscribeToChatResponse()
    {
        // Az ApiRequest a SmartAgent gyerekében vagy a saját GameObject-jén lehet
        apiRequest = smartAgent.GetComponent<ApiRequest>();
        if (apiRequest == null)
        {
            apiRequest = smartAgent.GetComponentInChildren<ApiRequest>();
        }

        if (apiRequest != null)
        {
            apiRequest.OnChatResponseReceived += HandleChatResponse;
            Debug.Log("[ScoreTracker] Feliratkozva a Neocortex ChatResponse eseményre");
        }
        else
        {
            Debug.LogWarning("[ScoreTracker] ApiRequest nem található a SmartAgent-en. " +
                "Ha a SmartAgent más módon kezeli a válaszokat, használd a HandleChatResponse() metódust manuálisan.");
        }
    }

    private void UnsubscribeFromChatResponse()
    {
        if (apiRequest != null)
        {
            apiRequest.OnChatResponseReceived -= HandleChatResponse;
        }
    }

    // ============================================================
    // ChatResponse feldolgozás
    // ============================================================

    /// <summary>
    /// ChatResponse feldolgozása. Az action mezõ alapján pontoz.
    /// Ha az ApiRequest feliratkozás nem mûködik, ezt a metódust
    /// manuálisan is hívhatod UnityEvent-bõl.
    /// </summary>
    public void HandleChatResponse(ChatResponse response)
    {
        if (response == null || string.IsNullOrEmpty(response.action))
        {
            return;
        }

        string action = response.action.Trim().ToUpper();

        switch (action)
        {
            case "POSITIVE":
                TotalPositives++;
                ChangeScore(positivePoints);
                onPositiveReceived?.Invoke();
                Debug.Log($"[ScoreTracker] POSITIVE akció! +{positivePoints} pont -> Összpontszám: {CurrentScore} (Össz positive: {TotalPositives})");
                break;

            case "NEGATIVE":
                TotalNegatives++;
                ChangeScore(-negativePoints);
                onNegativeReceived?.Invoke();
                Debug.Log($"[ScoreTracker] NEGATIVE akció! -{negativePoints} pont -> Összpontszám: {CurrentScore} (Össz negative: {TotalNegatives})");
                break;

            default:
                // Egyéb action-ök - nem pontozzuk, de logoljuk debug-hoz
                Debug.Log($"[ScoreTracker] Ismeretlen action: \"{response.action}\" - nem pontozva");
                break;
        }
    }

    // ============================================================
    // Pontszám kezelés
    // ============================================================

    /// <summary>
    /// Módosítja a pontszámot a megadott értékkel (pozitív vagy negatív).
    /// Betartja a min/max korlátokat.
    /// </summary>
    private void ChangeScore(int amount)
    {
        int previousScore = CurrentScore;
        CurrentScore += amount;

        // Min korlát
        if (CurrentScore < minScore)
        {
            CurrentScore = minScore;
        }

        // Max korlát (csak ha be van állítva, azaz > 0)
        if (maxScore > 0 && CurrentScore > maxScore)
        {
            CurrentScore = maxScore;
        }

        // Csak akkor hívjuk az eventet ha tényleg változott
        if (CurrentScore != previousScore)
        {
            onScoreChanged?.Invoke(CurrentScore);
        }
    }

    // ============================================================
    // Publikus segédmetódusok
    // ============================================================

    /// <summary>
    /// Visszaállítja a pontszámot a kezdõ értékre.
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = startingScore;
        TotalPositives = 0;
        TotalNegatives = 0;
        onScoreChanged?.Invoke(CurrentScore);
        Debug.Log($"[ScoreTracker] Pontszám resetelve: {CurrentScore}");
    }

    /// <summary>
    /// Manuális pontszám módosítás (pl. bónusz pontokhoz).
    /// </summary>
    public void AddScore(int amount)
    {
        ChangeScore(amount);
        Debug.Log($"[ScoreTracker] Manuális pont módosítás: {(amount >= 0 ? "+" : "")}{amount} -> Összpontszám: {CurrentScore}");
    }
}