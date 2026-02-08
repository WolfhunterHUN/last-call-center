using UnityEngine;

public class Mobilephonizer : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Az AudioSource ami a hangot játssza")]
    public AudioSource audioSource;

    [Header("Phone Effect Settings")]
    [Range(300, 3000)]
    [Tooltip("Alsó frekvencia levágás (telefon általában 300-1000 Hz)")]
    public int lowFrequencyCutoff = 800;

    [Range(3000, 8000)]
    [Tooltip("Felsõ frekvencia levágás (telefon általában 3000-5000 Hz)")]
    public int highFrequencyCutoff = 4000;

    [Range(0f, 1f)]
    [Tooltip("Torzítás mértéke (0.4-0.7 realisztikus)")]
    public float distortionLevel = 0.5f;

    [Range(0f, 1f)]
    [Tooltip("Eredeti hangerõ (általában 0.7-0.9)")]
    public float volume = 0.8f;

    [Header("Optional: Noise")]
    [Tooltip("Statikus zaj effekt (opcionális)")]
    public AudioClip staticNoise;

    [Range(0f, 0.3f)]
    [Tooltip("Statikus zaj hangereje")]
    public float noiseVolume = 0.05f;

    private AudioHighPassFilter highPassFilter;
    private AudioLowPassFilter lowPassFilter;
    private AudioDistortionFilter distortionFilter;
    private AudioSource noiseSource;

    void Start()
    {
        SetupPhoneEffect();
    }

    public void SetupPhoneEffect()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        // High Pass Filter hozzáadása (eltávolítja a mély hangokat)
        highPassFilter = gameObject.AddComponent<AudioHighPassFilter>();
        highPassFilter.cutoffFrequency = lowFrequencyCutoff;
        highPassFilter.highpassResonanceQ = 1.0f;

        // Low Pass Filter hozzáadása (eltávolítja a magas hangokat)
        lowPassFilter = gameObject.AddComponent<AudioLowPassFilter>();
        lowPassFilter.cutoffFrequency = highFrequencyCutoff;
        lowPassFilter.lowpassResonanceQ = 1.0f;

        // Distortion Filter hozzáadása (telefon torzítás)
        distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        distortionFilter.distortionLevel = distortionLevel;

        // Hangerõ beállítása
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }

        // Opcionális: statikus zaj
        if (staticNoise != null)
        {
            SetupStaticNoise();
        }

        Debug.Log("Telefonos hang effekt beállítva!");
    }

    private void SetupStaticNoise()
    {
        // Második AudioSource a statikus zajhoz
        noiseSource = gameObject.AddComponent<AudioSource>();
        noiseSource.clip = staticNoise;
        noiseSource.loop = true;
        noiseSource.volume = noiseVolume;
        noiseSource.Play();
    }

    // Runtime módosítás
    public void EnablePhoneEffect(bool enable)
    {
        if (highPassFilter != null) highPassFilter.enabled = enable;
        if (lowPassFilter != null) lowPassFilter.enabled = enable;
        if (distortionFilter != null) distortionFilter.enabled = enable;
        if (noiseSource != null) noiseSource.enabled = enable;
    }

    // Paraméterek frissítése runtime alatt
    void Update()
    {
        if (highPassFilter != null)
            highPassFilter.cutoffFrequency = lowFrequencyCutoff;

        if (lowPassFilter != null)
            lowPassFilter.cutoffFrequency = highFrequencyCutoff;

        if (distortionFilter != null)
            distortionFilter.distortionLevel = distortionLevel;

        if (audioSource != null)
            audioSource.volume = volume;

        if (noiseSource != null)
            noiseSource.volume = noiseVolume;
    }
}