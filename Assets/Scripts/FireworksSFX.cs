using UnityEngine;

public class FireworkSFX : MonoBehaviour
{
    public ParticleSystem explosionParticles;
    public AudioClip launchSFX;
    public AudioClip explosionSFX;

    private AudioSource audioSource;
    private bool explosionPlayed;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (launchSFX != null)
            audioSource.PlayOneShot(launchSFX);
    }

    void Update()
    {
        if (!explosionPlayed &&
            explosionParticles != null &&
            explosionParticles.particleCount > 0)
        {
            explosionPlayed = true;
            audioSource.PlayOneShot(explosionSFX);
        }
    }
}