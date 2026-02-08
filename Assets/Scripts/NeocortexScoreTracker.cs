using Neocortex.Data;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Stressz mérõ rendszer ami a Neocortex SmartAgent válaszaira reagál.
/// 
/// NEM kódból iratkozik fel — az Inspector-ban kösd össze a SmartAgent event-jeivel:
///   - SmartAgent "OnChatResponseReceived" -> NeocortexScoreTracker.OnChatResponse
///   - SmartAgent "OnAudioResponseReceived" -> NeocortexScoreTracker.OnAudioResponse
/// 
/// Szabályok:
///   - Minden válasz (chat vagy audio): +stressPerResponse (alapból +2)
///   - POSITIVE action (chat response-ból): -stressPositive (alapból -20)
///   - NEGATIVE action (chat response-ból): +stressNegative (alapból +20)
///   - Ha a stressz eléri a 100-at: Game Over (StressManager kezeli)
/// </summary>
public class NeocortexScoreTracker : MonoBehaviour
{
    [Header("Stressz beállítások")]
    [Tooltip("Ennyi stresszt kap minden válaszra (alap emelkedés)")]
    [SerializeField] private float stressPerResponse = 2f;

    [Tooltip("Ennyi stresszt csökkent POSITIVE action")]
    [SerializeField] private float stressPositive = 20f;

    [Tooltip("Ennyi stresszt növel NEGATIVE action")]
    [SerializeField] private float stressNegative = 20f;

    [Header("Események")]
    [Tooltip("POSITIVE action érkezésekor meghívódik")]
    public UnityEvent onPositiveReceived;

    [Tooltip("NEGATIVE action érkezésekor meghívódik")]
    public UnityEvent onNegativeReceived;

    /// <summary>
    /// Összesen hány POSITIVE action érkezett.
    /// </summary>
    public int TotalPositives { get; private set; }

    /// <summary>
    /// Összesen hány NEGATIVE action érkezett.
    /// </summary>
    public int TotalNegatives { get; private set; }

    /// <summary>
    /// Összesen hány válasz érkezett (chat + audio).
    /// </summary>
    public int TotalResponses { get; private set; }

    private void Awake()
    {
        TotalPositives = 0;
        TotalNegatives = 0;
        TotalResponses = 0;
    }

    // ============================================================
    // Publikus metódusok — ezeket kösd a SmartAgent UnityEvent-jeihez
    // ============================================================

    /// <summary>
    /// Kösd a SmartAgent "OnChatResponseReceived" UnityEvent-jéhez az Inspector-ban.
    /// Kezeli a stressz emelkedést és a POSITIVE/NEGATIVE action-öket.
    /// </summary>
    public void OnChatResponse(ChatResponse response)
    {
        if (response == null) return;

        StressManager sm = StressManager.Instance;
        if (sm == null || sm.IsGameOver) return;

        TotalResponses++;

        // Minden chat válasz növeli a stresszt
        sm.AddStress(stressPerResponse);
        Debug.Log($"[ScoreTracker] Chat válasz -> +{stressPerResponse} stressz (össz válasz: {TotalResponses})");

        // Action alapú stressz módosítás
        if (!string.IsNullOrEmpty(response.action))
        {
            string action = response.action.Trim().ToUpper();

            switch (action)
            {
                case "POSITIVE":
                    TotalPositives++;
                    sm.ReduceStress(stressPositive);
                    onPositiveReceived?.Invoke();
                    Debug.Log($"[ScoreTracker] POSITIVE! -{stressPositive} stressz (össz: {TotalPositives})");
                    break;

                case "NEGATIVE":
                    TotalNegatives++;
                    sm.AddStress(stressNegative);
                    onNegativeReceived?.Invoke();
                    Debug.Log($"[ScoreTracker] NEGATIVE! +{stressNegative} stressz (össz: {TotalNegatives})");
                    break;
            }
        }
    }

    /// <summary>
    /// Kösd a SmartAgent "OnAudioResponseReceived" UnityEvent-jéhez az Inspector-ban.
    /// Minden audio válasz növeli a stresszt.
    /// </summary>
    public void OnAudioResponse(AudioClip clip)
    {
        if (clip == null) return;

        StressManager sm = StressManager.Instance;
        if (sm == null || sm.IsGameOver) return;

        TotalResponses++;

        sm.AddStress(stressPerResponse);
        Debug.Log($"[ScoreTracker] Audio válasz -> +{stressPerResponse} stressz (össz válasz: {TotalResponses})");
    }
}