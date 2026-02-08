using System.Collections;
using UnityEngine;

public class RandomDistantSounds : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("Az AudioSource komponens, ami lejátssza a hangokat")]
    public AudioSource audioSource;

    [Tooltip("A lejátszható hangok listája")]
    public AudioClip[] soundClips;

    [Header("Timing Settings")]
    [Tooltip("Minimum várakozási idõ két hang között (másodpercben)")]
    public float minTimeBetweenSounds = 5f;

    [Tooltip("Maximum várakozási idõ két hang között (másodpercben)")]
    public float maxTimeBetweenSounds = 15f;

    [Header("Volume Settings")]
    [Tooltip("Minimum hangerõ (0-1)")]
    [Range(0f, 1f)]
    public float minVolume = 0.3f;

    [Tooltip("Maximum hangerõ (0-1)")]
    [Range(0f, 1f)]
    public float maxVolume = 0.6f;

    [Header("Spatial Settings")]
    [Tooltip("A hangforrás távolsága a középponttól")]
    public float soundDistance = 15f;

    [Tooltip("A hangforrás magassága")]
    public float soundHeight = 2f;

    [Header("Options")]
    [Tooltip("Automatikusan elinduljon a script?")]
    public bool playOnStart = true;

    private bool isPlaying = false;
    private Transform listenerTransform;

    void Start()
    {
        // AudioListener megkeresése (általában a kamerán van)
        AudioListener listener = FindObjectOfType<AudioListener>();
        if (listener != null)
        {
            listenerTransform = listener.transform;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 1f; // 3D hang
            }
        }

        if (soundClips.Length == 0)
        {
            Debug.LogWarning("Nincsenek hangok hozzárendelve a RandomDistantSounds scripthez!");
            return;
        }

        if (playOnStart)
        {
            StartPlayingSounds();
        }
    }

    public void StartPlayingSounds()
    {
        if (!isPlaying)
        {
            isPlaying = true;
            StartCoroutine(PlayRandomSoundsCoroutine());
        }
    }

    public void StopPlayingSounds()
    {
        isPlaying = false;
        StopAllCoroutines();
    }

    private IEnumerator PlayRandomSoundsCoroutine()
    {
        while (isPlaying)
        {
            float waitTime = Random.Range(minTimeBetweenSounds, maxTimeBetweenSounds);
            yield return new WaitForSeconds(waitTime);

            if (soundClips.Length > 0)
            {
                AudioClip selectedClip = soundClips[Random.Range(0, soundClips.Length)];

                // Random irány generálás (360 fok)
                float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;

                // Új pozíció számítás a listener körül
                Vector3 newPosition;
                if (listenerTransform != null)
                {
                    newPosition = listenerTransform.position + new Vector3(
                        Mathf.Cos(randomAngle) * soundDistance,
                        soundHeight,
                        Mathf.Sin(randomAngle) * soundDistance
                    );
                }
                else
                {
                    // Ha nincs listener, a jelenet középpontja körül
                    newPosition = new Vector3(
                        Mathf.Cos(randomAngle) * soundDistance,
                        soundHeight,
                        Mathf.Sin(randomAngle) * soundDistance
                    );
                }

                // AudioSource pozíciójának frissítése
                transform.position = newPosition;

                audioSource.volume = Random.Range(minVolume, maxVolume);
                audioSource.PlayOneShot(selectedClip);

                Debug.Log($"Lejátszva: {selectedClip.name} - Pozíció: {newPosition} - Hangerõ: {audioSource.volume:F2}");
            }
        }
    }
}