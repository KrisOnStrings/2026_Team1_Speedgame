using UnityEngine;

public class MusicController : MonoBehaviour
{
    public AudioClip DayMusic;
    public AudioClip NightMusic;
    public AudioClip BossMusic;

    public float FadeSpeed = 1.5f;
    public float MaxVolume;

    private MusicType curMusicType = MusicType.None;
    private MusicType nextMusicType = MusicType.None;

    private AudioSource m_Audio;

    public enum MusicType
    {
        None,
        Day,
        Night,
        Boss
    }

    private void Awake()
    {
        m_Audio = GetComponent<AudioSource>();
    }

    public void SetMusic(MusicType mt)
    {
        nextMusicType = mt;
    }

    private void Update()
    {
        // Nothing to do if we're already playing the desired music.
        if (curMusicType == nextMusicType)
        {
            // Ensure we're faded in.
            if (m_Audio.volume < MaxVolume)
            {
                m_Audio.volume += FadeSpeed * Time.deltaTime;
                if (m_Audio.volume > MaxVolume)
                    m_Audio.volume = MaxVolume;
            }

            return;
        }

        // Fade out current music.
        if (m_Audio.volume > 0f)
        {
            m_Audio.volume -= FadeSpeed * Time.deltaTime;

            if (m_Audio.volume < 0f)
                m_Audio.volume = 0f;

            return;
        }

        // Change tracks once fully faded out.
        curMusicType = nextMusicType;

        switch (curMusicType)
        {
            case MusicType.Day:
                m_Audio.clip = DayMusic;
                break;

            case MusicType.Night:
                m_Audio.clip = NightMusic;
                break;

            case MusicType.Boss:
                m_Audio.clip = BossMusic;
                break;

            default:
                m_Audio.clip = null;
                break;
        }

        if (m_Audio.clip != null)
        {
            m_Audio.Play();
        }
    }
}
