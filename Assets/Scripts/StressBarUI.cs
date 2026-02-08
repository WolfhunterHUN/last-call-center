using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Stressz bar UI megjelenítés.
/// A játékos kamerájára rakott Canvas-on él.
/// Automatikusan frissül a StressManager-ből.
/// 
/// Setup:
/// 1. Hozz létre egy Canvas-t (Screen Space - Overlay)
/// 2. A Canvas alá:
///    - "StressBarBG" (Image) — háttér
///    - "StressBarFill" (Image) — kitöltés (Image Type: Filled, Fill Method: Horizontal)
///    - "StressText" (TMP_Text) — opcionális szöveg kijelző
/// 3. Tedd rá ezt a scriptet a Canvas-ra vagy egy üres gyerekre
/// 4. Rendeld hozzá a referenciákat az Inspector-ban
/// 
/// VAGY: Használd a CreateDefaultUI() metódust ami automatikusan létrehozza!
/// Ehhez csak hozz létre egy üres GameObject-et, tedd rá ezt a scriptet,
/// és jelöld be a "Auto Create UI" checkbox-ot.
/// </summary>
public class StressBarUI : MonoBehaviour
{
    [Header("Referenciák")]
    [Tooltip("A kitöltő Image (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] private Image fillImage;

    [Tooltip("A háttér Image")]
    [SerializeField] private Image backgroundImage;

    [Tooltip("Opcionális szöveg kijelző (pl. 'Stress: 45%')")]
    [SerializeField] private TMP_Text stressText;

    [Header("Auto Setup")]
    [Tooltip("Ha igaz, automatikusan létrehozza a UI elemeket Start()-ban ha nincsenek hozzárendelve")]
    [SerializeField] private bool autoCreateUI = true;

    [Header("Megjelenés")]
    [Tooltip("Normál stress szín (alacsony)")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.8f, 0.2f, 0.9f); // Zöld

    [Tooltip("Közepes stress szín")]
    [SerializeField] private Color warningColor = new Color(1f, 0.8f, 0f, 0.9f); // Sárga

    [Tooltip("Veszélyes stress szín (80+ felett)")]
    [SerializeField] private Color dangerColor = new Color(1f, 0.15f, 0.15f, 0.9f); // Piros

    [Tooltip("Háttér szín")]
    [SerializeField] private Color bgColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);

    [Header("Animáció")]
    [Tooltip("Milyen gyorsan változzon a bar (lerp sebesség)")]
    [SerializeField] private float lerpSpeed = 5f;

    [Tooltip("Pulzáljon-e veszélyzónában")]
    [SerializeField] private bool pulseInDanger = true;

    [Tooltip("Pulzálás sebesség")]
    [SerializeField] private float pulseSpeed = 3f;

    // Belső állapot
    private StressManager stressManager;
    private float displayedStress = 0f;
    private float targetStress = 0f;
    private Canvas canvas;

    private void Start()
    {
        stressManager = StressManager.Instance;

        if (stressManager == null)
        {
            Debug.LogError("[StressBarUI] StressManager nem található! Hozz létre egyet a scene-ben.");
            return;
        }

        // Auto UI létrehozás ha szükséges
        if (autoCreateUI && fillImage == null)
        {
            CreateDefaultUI();
        }

        // Feliratkozás a stressz változásra
        stressManager.onStressChanged.AddListener(OnStressChanged);

        // Kezdő érték beállítása
        targetStress = stressManager.CurrentStress;
        displayedStress = targetStress;
        UpdateVisuals(displayedStress);
    }

    private void OnDestroy()
    {
        if (stressManager != null)
        {
            stressManager.onStressChanged.RemoveListener(OnStressChanged);
        }
    }

    private void Update()
    {
        // Smooth lerp a megjelenített stressz felé
        if (Mathf.Abs(displayedStress - targetStress) > 0.01f)
        {
            displayedStress = Mathf.Lerp(displayedStress, targetStress, Time.deltaTime * lerpSpeed);
            UpdateVisuals(displayedStress);
        }

        // Pulzálás veszélyzónában
        if (pulseInDanger && stressManager != null && stressManager.IsInDangerZone && fillImage != null)
        {
            float pulse = Mathf.Sin(Time.time * pulseSpeed) * 0.15f + 0.85f; // 0.7 - 1.0 között
            Color c = fillImage.color;
            c.a = pulse;
            fillImage.color = c;
        }
    }

    // ============================================================
    // Stressz változás kezelése
    // ============================================================

    private void OnStressChanged(float newStress)
    {
        targetStress = newStress;
    }

    private void UpdateVisuals(float stress)
    {
        if (stressManager == null) return;

        float normalizedStress = stress / stressManager.MaxStress;

        // Fill bar frissítés
        if (fillImage != null)
        {
            fillImage.fillAmount = normalizedStress;

            // Szín interpoláció: zöld -> sárga -> piros
            Color barColor;
            float dangerNormalized = stressManager.DangerThreshold / stressManager.MaxStress;

            if (normalizedStress < dangerNormalized * 0.5f)
            {
                // 0% - 40%: Zöld -> Sárga
                barColor = Color.Lerp(normalColor, warningColor, normalizedStress / (dangerNormalized * 0.5f));
            }
            else if (normalizedStress < dangerNormalized)
            {
                // 40% - 80%: Sárga -> Piros felé
                float t = (normalizedStress - dangerNormalized * 0.5f) / (dangerNormalized * 0.5f);
                barColor = Color.Lerp(warningColor, dangerColor, t);
            }
            else
            {
                // 80%+: Piros
                barColor = dangerColor;
            }

            fillImage.color = barColor;
        }

        // Szöveg frissítés
        if (stressText != null)
        {
            stressText.text = $"Stress: {Mathf.RoundToInt(stress)}%";

            // Szöveg szín is változik
            if (stress >= stressManager.DangerThreshold)
            {
                stressText.color = dangerColor;
            }
            else
            {
                stressText.color = Color.white;
            }
        }
    }

    // ============================================================
    // Automatikus UI létrehozás
    // ============================================================

    /// <summary>
    /// Létrehozza a teljes stressz bar UI-t kódból.
    /// Screen Space Overlay Canvas-t hoz létre ami mindig látható.
    /// </summary>
    private void CreateDefaultUI()
    {
        Debug.Log("[StressBarUI] Automatikus UI létrehozás...");

        // Canvas (ha még nincs)
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Minden felett

            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            gameObject.AddComponent<GraphicRaycaster>();
        }

        // Bar konténer (jobb felső sarok)
        GameObject barContainer = new GameObject("StressBarContainer");
        barContainer.transform.SetParent(canvas.transform, false);
        RectTransform containerRect = barContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.7f, 0.92f);   // Jobb felső
        containerRect.anchorMax = new Vector2(0.95f, 0.96f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;

        // Háttér
        GameObject bgObj = new GameObject("StressBarBG");
        bgObj.transform.SetParent(barContainer.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        backgroundImage = bgObj.AddComponent<Image>();
        backgroundImage.color = bgColor;

        // Keret (vékony border)
        // A háttér az egész terület, a fill belül lesz pár pixellel

        // Fill bar
        GameObject fillObj = new GameObject("StressBarFill");
        fillObj.transform.SetParent(barContainer.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0.01f, 0.1f);  // Kis padding
        fillRect.anchorMax = new Vector2(0.99f, 0.9f);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillImage = fillObj.AddComponent<Image>();
        fillImage.color = normalColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 0f;

        // Szöveg (a bar felett)
        GameObject textObj = new GameObject("StressText");
        textObj.transform.SetParent(barContainer.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0f, 1f);
        textRect.anchorMax = new Vector2(1f, 1.8f);
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        stressText = textObj.AddComponent<TextMeshProUGUI>();
        stressText.text = "Stress: 0%";
        stressText.fontSize = 16;
        stressText.color = Color.white;
        stressText.alignment = TextAlignmentOptions.Center;

        Debug.Log("[StressBarUI] UI létrehozva (jobb felső sarok)");
    }
}
