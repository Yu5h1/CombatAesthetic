using System.Collections;
using UnityEngine;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class SoundManager : SingletonComponent<SoundManager>
{
    public static  float bgmVolume
    {
        get
        {
            if (PlayerPrefs.HasKey(nameof(bgmVolume)))
                return PlayerPrefs.GetFloat(nameof(bgmVolume));
            return 1;
        }
        set
        {
            if (bgmVolume == value)
                return;
            PlayerPrefs.SetFloat(nameof(bgmVolume), value);
            instance.OnBgmVolumeChanged();
        }
    }
    public static float sfxVolume
    {
        get
        {
            if (PlayerPrefs.HasKey(nameof(sfxVolume)))
                return PlayerPrefs.GetFloat(nameof(sfxVolume));
            return 0;
        }
        set
        {
            if (sfxVolume == value)
                return;
            PlayerPrefs.SetFloat(nameof(sfxVolume), value);
            instance.OnSfxVolumeChanged();
        }
    }

    private AudioListener _AudioListener;
    public AudioListener audioListener => this.GetOrAdd(out _AudioListener);

    private AudioSource _Audio_bgm;
    public AudioSource Audio_bgm => this.GetOrCreate(nameof(Audio_bgm), ref _Audio_bgm);

    private AudioSource _Audio_sfx;
    public AudioSource audio_sfx => this.GetOrCreate(nameof(audio_sfx),ref _Audio_sfx);

    public Timer fadeTimer = new Timer();

    static PoolManager PoolManager => PoolManager.instance;

    protected override void Init()
    {
        Audio_bgm.playOnAwake = false;
        audio_sfx.playOnAwake = false;
    }
    public void Start()
    {
        Audio_bgm.Stop();
        if (ResourcesEx.TryLoad($"Sound/{SceneController.ActiveScene.name}", out AudioClip clip, false))
            Audio_bgm.clip = clip;

        Audio_bgm.volume = bgmVolume;
        if (Audio_bgm.clip)
            Audio_bgm.Play();
    }
    #region Event

    private void OnBgmVolumeChanged()
    {
        Audio_bgm.volume = bgmVolume;
    }
    private void OnSfxVolumeChanged()
    {
        audio_sfx.volume = sfxVolume;
    }
    #endregion
    #region Public Methods

    //public void BgmFadeOut(float duration){
    //    fadeTimer.duration = duration;

    //}

    private void Init(AudioSource audio)
    {
        audio.spatialBlend = 1f;
    }

    public void play(string name, Vector3 position , Quaternion rotation )
    {
        if (!ResourcesEx.TryLoad($"Sound/{name}", out AudioClip clip))
            return;
        var audioSource = PoolManager.Spawn<AudioSource>(position, rotation, Init);
        audioSource.volume = sfxVolume;
        audioSource.clip = clip;
        audioSource.enabled = true;
        audioSource.Play();
        StartCoroutine(WaitForSoundToEnd(audioSource));
    }
    public static void Play(string name, Vector3 position, Quaternion rotation = default(Quaternion))
        => instance.play(name, position, rotation);
    #endregion
    #region private
    private IEnumerator WaitForSoundToEnd(AudioSource audioSource)
    {
        while (audioSource && audioSource.isPlaying)
            yield return null;
        if (audioSource)
        {
            audioSource.Stop();
            audioSource.enabled = false;
            PoolManager.instance.Despawn(audioSource);
        }
    }
    private void OnDestroy()
    {
        
    }
    #endregion
}
