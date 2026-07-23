using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource seSource;

    [Header("BGM")]
    public AudioClip bgmClip;

    [Header("SE")]
    public AudioClip cannotMoveSE;
    public AudioClip clearSE;
    public AudioClip moveSE;
    public AudioClip uiMoveSE;
    public AudioClip decideSE;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }

        if (seSource == null)
        {
            seSource = gameObject.AddComponent<AudioSource>();
        }

        bgmSource.loop = true;
        bgmSource.playOnAwake = false;

        seSource.loop = false;
        seSource.playOnAwake = false;

        PlayBGM();
    }

    public void PlayBGM()
    {
        if (bgmClip == null) return;

        if (bgmSource.clip == bgmClip && bgmSource.isPlaying)
        {
            return;
        }

        bgmSource.clip = bgmClip;
        bgmSource.Play();
    }

    public void PlaySE(AudioClip clip)
    {
        if (clip == null) return;
        if (seSource == null) return;

        seSource.PlayOneShot(clip);
    }

    public void PlayCannotMoveSE()
    {
        PlaySE(cannotMoveSE);
    }

    public void PlayClearSE()
    {
        PlaySE(clearSE);
    }

    public void PlayMoveSE()
    {
        PlaySE(moveSE);
    }

    public void PlayUIMoveSE()
    {
        PlaySE(uiMoveSE);
    }

    public void PlayDecideSE()
    {
        PlaySE(decideSE);
    }
}