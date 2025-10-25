using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private const string GROUP_MASTER = "MasterVolume";
    private const string GROUP_MUSIC = "MusicVolume";
    private const string GROUP_SFX = "SFXVolume";
    private const float MUTE_VOLUME = -80f;

    public float MinDistance { get; set; } = 1.5f;
    public float MaxDistance { get; set; } = 100f;

    [SerializeField]
    private AudioMixer Mixer;

    [SerializeField]
    private AudioSource SFXSource;
    [SerializeField]
    private AudioMixerGroup SFXMixerGroup;

    [SerializeField]
    private AudioMixerGroup UIMixerGroup;

    private GameObjectPool<AudioSource> sfxPool;

    void Awake()
    {
        sfxPool = new GameObjectPool<AudioSource>(10).DontDestroy();
        Instance = this;
    }

    public static void PlaySFX(AudioClip sfx, float volume = 1f, float pitch = 1f)
    {
        if (sfx != null)
        {
            Instance.PlayClipAtPoint(sfx, Vector3.zero, volume, pitch, Instance.SFXMixerGroup);
        }
    }

    public static void PlaySFX(AudioClip sfx, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        Instance.PlayClipAtPoint(sfx, position, volume, pitch, Instance.SFXMixerGroup);
    }

    public static void PlaySFX(AudioClip[] sfx, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        PlaySFX(sfx.RandomNullable(), position, volume, pitch);
    }

    public static void PlaySFX(AudioClip sfx, Transform transform, float volume = 1f, float pitch = 1f)
    {
        var audio = Instance.PlayClipAtPoint(sfx, transform.position, volume, pitch, Instance.SFXMixerGroup);

        if (audio != null)
            audio.transform.SetParent(transform);
    }

    public static void PlayUI(AudioClip sfx, float volume = 1f, float pitch = 1f)
    {
        Instance.PlayClipAtPoint(sfx, Vector3.zero, volume, pitch, Instance.UIMixerGroup);
    }

    public static void PlaySFX(AudioClip[] sfx, float volume = 1f)
    {
        PlaySFX(sfx.RandomNullable(), volume);
    }

    public static void SetMasterVolume(float volume)
    {
        Instance.Mixer.SetFloat(GROUP_MASTER, volume == 0 ? MUTE_VOLUME : Mathf.Log10(volume) * 20f);
    }

    public static void SetSFXVolume(float volume = 1f)
    {
        Instance.Mixer.SetFloat(GROUP_SFX, volume == 0 ? MUTE_VOLUME : Mathf.Log10(volume) * 20f);
    }

    public static void SetMusicVolume(float volume = 1f)
    {
        Instance.Mixer.SetFloat(GROUP_MUSIC, volume == 0 ? MUTE_VOLUME : Mathf.Log10(volume) * 20f);
    }

    private AudioSource PlayClipAtPoint(AudioClip clip, Vector3 position, float volume = 1.0f, float pitch = 1f, AudioMixerGroup group = null)
    {
        if (clip == null) return null;

        AudioSource audioSource = sfxPool.Get();
        audioSource.transform.position = position;

        // DEBUG REMOVE AFTER FIX!!!
//        if (!audioSource.enabled)
//        {
//            Logger.Warn($"SFX audio source is disabled! {audioSource.clip} -- {audioSource.gameObject.activeInHierarchy}");
//#if UNITY_EDITOR
//            Debug.Break();
//            audioSource.enabled = true;
//#endif
//        }

        if (group != null)
            audioSource.outputAudioMixerGroup = group;

        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.volume = volume;
        audioSource.Play();

        audioSource.spatialBlend = position.IsZero() ? 0f : 1f;
        audioSource.maxDistance = MinDistance;
        audioSource.minDistance = MaxDistance;

        float duration = clip.length; // * (Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale);
        sfxPool.ReleaseDelayed(audioSource, duration / pitch, true);

        return audioSource;
    }
}

[System.Serializable]
public class SoundContainer
{
    public AudioClip Clip;
    public float Pitch = 1f;
    public float Volume = 1f;
}
