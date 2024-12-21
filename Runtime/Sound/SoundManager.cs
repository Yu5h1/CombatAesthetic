using System.Collections;
using UnityEngine;
using Yu5h1Lib;

[DisallowMultipleComponent]
public class SoundManager : SingletonBehaviour<SoundManager>
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
    public AudioListener audioListener => this.GetOrCreate(nameof(AudioListener),ref _AudioListener);

    private AudioSource _Audio_bgm;
    public AudioSource Audio_bgm => this.GetOrCreate(nameof(Audio_bgm), ref _Audio_bgm);

    private AudioSource _Audio_sfx;
    public AudioSource audio_sfx => this.GetOrCreate(nameof(audio_sfx),ref _Audio_sfx);

    public Timer fadeTimer = new Timer();

    static PoolManager PoolManager => PoolManager.instance;

    protected override void Init()
    {
        $"{audioListener} is ready.".print();
        Audio_bgm.playOnAwake = false;
        Audio_bgm.loop = true;
        audio_sfx.playOnAwake = false;
    }
    public void Start()
    {
        Audio_bgm.Stop();
        if (!$"bgm {SceneController.ActiveScene.name} does not exist".printWarningIf(
            !ResourcesUtility.TryLoad($"Sound/{SceneController.ActiveScene.name}", out AudioClip clip)))
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

    private void InitAudioSource(AudioSource audio)
    {
        audio.spatialBlend = 1f;
        audio.spread = 180;
        audio.minDistance = 0.1f;
        audio.maxDistance = 10f;
    }

    public void play(string name, Vector3 position , Quaternion rotation )
    {
        if ("".printWarningIf(!ResourcesUtility.TryLoad($"Sound/{name}", out AudioClip clip)))
            return;
        var audioSource = PoolManager.Spawn<AudioSource>(position, rotation, InitAudioSource);
        audioSource.volume = sfxVolume;
        audioSource.clip = clip;
        //audioSource.enabled = true;
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
            //audioSource.enabled = false;
            PoolManager.instance.Despawn(audioSource);
        }
    }
    private void OnDestroy()
    {
        
    }
    #endregion
}
