using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CLGameToolkit;
using UnityEngine;

public class MusicPlayer : MonoSingleton<MusicPlayer>
{
    [SerializeField] private float CrossFadeDuration = 2.0f;
    [SerializeField] private AudioSource MusicSource;

    private readonly Stack<MusicLayer> musicStack = new();

    public static int LayerCount => Instance.musicStack.Count;
    public static AudioClip CurrentClip => Instance.MusicSource.clip;

#if UNITY_EDITOR
    [Header("Player Preview")]
    [ReadOnly, SerializeField] private AudioClip Playing; // Display
    [ReadOnly, SerializeField] private List<MusicLayer> Layers; // Display
#endif

    private IEnumerator queueCoroutine;
    private Coroutine crossFadeCoroutine;
    private AudioSource fadeOutAudioSource;

    public static void PlayMusic(AudioClip music, float fadeDuration = 0f, float volume = 1.0f, bool addToStack = false)
    {
        if (music == null)
            return;

        PlayMusic(new[] { music }, fadeDuration, volume, addToStack);
    }

    public static void PlayMusic(AudioClip[] music, float fadeDuration = 0f, float volume = 1.0f, bool addToStack = false)
    {
        if (music == null || music.Length == 0) return;

        Stack<MusicLayer> stack = Instance.musicStack;

        if (stack.Count > 0 && Instance.IsSamePlaylist(stack.Peek(), music))
            return;

        MusicLayer layer = new(music, volume);

        if (addToStack) // Add previous playing music
        {
            if (stack.TryPeek(out var oldLayer))
            {
                oldLayer.SaveState(Instance.MusicSource.clip, Instance.MusicSource.time);
            }

            stack.Push(layer);
        }
        else
        {
            stack.TryPop(out var _);
            stack.Push(layer);
        }

        Instance.UpdateCurrentLayer(layer, fadeDuration);
    }

    public static void PlayMusicRoot(AudioClip[] music, float fadeDuration = 0f, float volume = 1.0f)
    {
        if (music == null || music.Length == 0) return;

        Stack<MusicLayer> stack = Instance.musicStack;
        if (stack.Count > 0 && Instance.IsSamePlaylist(stack.Peek(), music))
            return;

        MusicLayer layer = new(music, volume);
        Instance.musicStack.Clear();
        Instance.musicStack.Push(layer);

        Instance.UpdateCurrentLayer(layer, fadeDuration);
    }

    public static void PlayMusicRoot(AudioClip music, float fadeDuration = 0f, float volume = 1.0f)
    {
        if (music == null) return;
        PlayMusicRoot(new[] { music }, fadeDuration, volume);
    }

    public static void PlayMusicPopHistory(float fadeDuration = 0f, AudioClip[] fallbackClip = null, float fallbackVolume = 1f)
    {
        if (Instance.musicStack.Count > 1)
        {
            Instance.musicStack.Pop();
            MusicLayer next = Instance.musicStack.Peek();
            Instance.UpdateCurrentLayer(next, fadeDuration);
        }
        else if (fallbackClip != null)
        {
            PlayMusic(fallbackClip, fadeDuration, fallbackVolume);
        }
        else
        {
            Logger.Warn("MusicPlayer tried to pop root layer");
        }
    }

    private void UpdateCurrentLayer(MusicLayer layer, float fadeDuration = 0)
    {
        // If we want no repeats
        // var availableMusic = music.Where(clip => clip != musicSource.clip);
        // var availableMusicCount = availableMusic.Count();
        // AudioClip musicToPlay = availableMusic.ElementAt(Random.Range(0, availableMusicCount));

        if (queueCoroutine != null)
            StopCoroutine(queueCoroutine);

        if (crossFadeCoroutine != null)
            StopCoroutine(crossFadeCoroutine);

        if (fadeOutAudioSource != null)
            Destroy(fadeOutAudioSource);

        float newFadeDuration = fadeDuration > 0 ? fadeDuration : CrossFadeDuration;
        newFadeDuration = fadeDuration < 0 ? 0f : newFadeDuration; // Negative values means no fade

        // Improve shuffle and or other types of song selectors.
        AudioClip toPlay = layer.time > 0 ? layer.current : layer.songs.Random();

        Logger.Debug($"[Music] UpdateMusicLayer to {toPlay} from {MusicSource.clip}");
        crossFadeCoroutine = StartCoroutine(CrossFadeMusicAsync(toPlay, newFadeDuration, layer.volume, layer.time));

        bool shouldLoop = layer.songs.Length == 1;
        MusicSource.loop = shouldLoop;

        if (!shouldLoop)
        {
            queueCoroutine = WaitAndPlayNext();
            StartCoroutine(queueCoroutine);
        }


#if UNITY_EDITOR
        Layers = musicStack.ToList();
        Playing = toPlay;
#endif
    }

    private IEnumerator WaitAndPlayNext()
    {
        yield return new WaitForSeconds(GetClipRemainingTime() - CrossFadeDuration);

        UpdateCurrentLayer(musicStack.Peek());
        yield break;
    }

    public void SkipSong()
    {
        if (queueCoroutine != null)
            StopCoroutine(queueCoroutine);

        UpdateCurrentLayer(musicStack.Peek());
    }

    public static void Pause()
    {
        Instance.MusicSource.Pause();
    }

    public static void Resume()
    {
        Instance.MusicSource.UnPause();
    }

    private IEnumerator CrossFadeMusicAsync(AudioClip musicClip, float duration, float volume, float time = 0)
    {
#if UNITY_EDITOR
        if (musicClip == null)
            Logger.Error("AudioManager tried to CrossFade to a null AudioClip!");
#endif

        if (MusicSource.clip == null || duration == 0f)
        {
            MusicSource.volume = volume;
            MusicSource.clip = musicClip;
            MusicSource.time = time;
            MusicSource.Play();
            crossFadeCoroutine = null;
            yield break;
        }

        // Add new audiosource and set it to all parameters of original audiosource
        fadeOutAudioSource = MusicSource.gameObject.AddComponent<AudioSource>();
        fadeOutAudioSource.clip = MusicSource.clip;
        fadeOutAudioSource.time = MusicSource.time;
        fadeOutAudioSource.volume = MusicSource.volume;
        fadeOutAudioSource.outputAudioMixerGroup = MusicSource.outputAudioMixerGroup;
        fadeOutAudioSource.Play();

        // set original audiosource volume and clip
        MusicSource.volume = 0f;
        MusicSource.clip = musicClip;
        MusicSource.time = time;
        MusicSource.Play();

        float t = 0;
        float v = fadeOutAudioSource.volume;

        // begin fading in original audiosource with new clip as we fade out new audiosource with old clip
        while (t < 0.98f)
        {
            t += Time.unscaledDeltaTime / duration;
            // t = Mathf.Lerp(t, 1f, Time.deltaTime * 0.2f);
            fadeOutAudioSource.volume = Mathf.Lerp(v, 0f, t);
            MusicSource.volume = Mathf.Lerp(0f, volume, t);
            yield return null;
        }

        MusicSource.volume = volume;
        Destroy(fadeOutAudioSource);
        crossFadeCoroutine = null;
        yield break;
    }

    private bool IsSamePlaylist(MusicLayer layer, AudioClip[] songs)
    {
        if (songs.Length == 1)
        {
            return MusicSource.clip == songs[0]; // layer.songs.Contains(songs[0]);
        }

        return songs == layer.songs;
    }

    private float GetClipRemainingTime()
    {
        return (float)((MusicSource.clip.length - MusicSource.time) / MusicSource.pitch);
    }

#if UNITY_EDITOR
    [System.Serializable]
#endif
    private class MusicLayer
    {
        public AudioClip[] songs;
        [System.NonSerialized] public AudioClip current;

        public float time;
        public float volume;

        public MusicLayer(AudioClip[] songs, float volume, float time = 0)
        {
            this.songs = songs;
            this.volume = volume;
            this.time = time;
        }

        public MusicLayer(AudioClip song, float volume, float time = 0) : this(new[] { song }, volume, time) { }

        public void SaveState(AudioClip song, float time)
        {
            this.current = song;
            this.time = time;
        }
    }
}
