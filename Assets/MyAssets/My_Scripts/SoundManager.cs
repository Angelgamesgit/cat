using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq; // Dictionaryへの変換で利用


// ▼ Inspectorに表示するための定義 ▼


// --- UIサウンド関連 ---
[Serializable]
public class SoundClip {
    public UISoundType soundType;
    public AudioClip audioClip;
}
public enum UISoundType { Click, Submit, Cancel, Hover, Success, Error }

// --- 足音サウンド関連 (★追加) ---
[Serializable]
public class FootstepCollection
{
    public SurfaceType surfaceType;
    public AudioClip[] audioClips; // 同じ地面でも複数の音をランダムに鳴らすため配列にする
}

public enum SurfaceType { Default, Grass, Wood, Stone, Metal, Sand , Snow , Water}

// ▼ メインのSoundManagerクラス 

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{

    // --- シングルトンインスタンス ---
    public static SoundManager Instance { get; private set; }


    // --- Inspectorから設定する項目 ---
    [Header("■ Audio Mixer Settings")]
    [SerializeField] private AudioMixer mainMixer;
    [SerializeField, Tooltip("Master音量のパラメータ名")] private string masterVolumeParam = "MasterVolume";
    [SerializeField, Tooltip("BGM音量のパラメータ名")] private string bgmVolumeParam = "BGMVolume";
    [SerializeField, Tooltip("SE音量のパラメータ名")] private string seVolumeParam = "SEVolume";

    [Header("■ UI Sound Settings")]
    [SerializeField, Tooltip("UIの効果音を設定します")] private List<SoundClip> uiSoundClips;
    [SerializeField, Tooltip("UI効果音を再生するAudioSourceの出力先MixerGroup")] private AudioMixerGroup uiSoundMixerGroup;

    [Header("■ Footstep Settings")]
    [SerializeField, Tooltip("地面の種類ごとの足音を設定します")]
    private List<FootstepCollection> walk_footstepCollections, run_FootstepCollection , jumpUp_FootstepCollection , jumpDown_FootstepCollection;
    
    [SerializeField, Tooltip("足音を再生する際の出力先MixerGroup")]
    private AudioMixerGroup footstepMixerGroup; // 足音専用のMixerGroup


    // --- 内部変数 ---
    private AudioSource uiAudioSource;
    private Dictionary<UISoundType, AudioClip> uiSoundDictionary;
    //足音
    private Dictionary<SurfaceType, AudioClip[]> walk_footstepDictionary,run_footstepDictionary,jumpUp_footstepDictionary,jumpDown_footstepDictionary;
    private Dictionary<CatSystem.AnimState, Dictionary<SurfaceType, AudioClip[]>> stateDic;

    private const string MASTER_VOLUME_KEY = "MasterVolume_Value";
    private const string BGM_VOLUME_KEY = "BGMVolume_Value";
    private const string SE_VOLUME_KEY = "SEVolume_Value";

    void Awake()
    {
        // --- シングルトンパターンの実装 ---
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // --- UIサウンド関連の初期化 ---
        uiAudioSource = GetComponent<AudioSource>();
        uiAudioSource.outputAudioMixerGroup = uiSoundMixerGroup;
        uiAudioSource.playOnAwake = false;
        uiAudioSource.spatialBlend = 0;

        uiSoundDictionary = uiSoundClips.ToDictionary(clip => clip.soundType, clip => clip.audioClip);

        // --- 足音サウンド関連の初期化 (★追加) ---
        walk_footstepDictionary = walk_footstepCollections.ToDictionary(col => col.surfaceType, col => col.audioClips);
        run_footstepDictionary = run_FootstepCollection.ToDictionary(col => col.surfaceType, col => col.audioClips);
        jumpUp_footstepDictionary = jumpUp_FootstepCollection.ToDictionary(col => col.surfaceType, col => col.audioClips);
        jumpDown_footstepDictionary = jumpDown_FootstepCollection.ToDictionary(col => col.surfaceType, col => col.audioClips);
        stateDic = new Dictionary<CatSystem.AnimState, Dictionary<SurfaceType, AudioClip[]>>
        {
            { CatSystem.AnimState.walk, walk_footstepDictionary },
            { CatSystem.AnimState.run, run_footstepDictionary },
            { CatSystem.AnimState.jump, jumpUp_footstepDictionary },
            { CatSystem.AnimState.getoff, jumpDown_footstepDictionary },
            {CatSystem.AnimState.idle, jumpDown_footstepDictionary}
        };
    }

    void Start()
    {
        // --- 音量設定の読み込みと適用 ---
        LoadAndApplyVolume(masterVolumeParam, MASTER_VOLUME_KEY);
        LoadAndApplyVolume(bgmVolumeParam, BGM_VOLUME_KEY);
        LoadAndApplyVolume(seVolumeParam, SE_VOLUME_KEY);
    }

    #region Volume Control
    public void SetMasterVolume(float volume) { SetVolume(masterVolumeParam, MASTER_VOLUME_KEY, volume); }
    public void SetBGMVolume(float volume) { SetVolume(bgmVolumeParam, BGM_VOLUME_KEY, volume); }
    public void SetSEVolume(float volume) { SetVolume(seVolumeParam, SE_VOLUME_KEY, volume); }
    #endregion

    #region Sound Playback
    public void PlayUISound(UISoundType type)
    {
        if (uiSoundDictionary.TryGetValue(type, out AudioClip clip))
        {
            uiAudioSource.PlayOneShot(clip);
        }
    }

    public void PlaySoundAtPoint(AudioClip clip, Vector3 position, AudioMixerGroup mixerGroup )
    {
        GameObject tempAudioObj = new GameObject("TempAudio");
        tempAudioObj.transform.position = position;
        AudioSource audioSource = tempAudioObj.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.spatialBlend = 1.0f;
        audioSource.Play();
        Destroy(tempAudioObj, clip.length);
    }

    /// <summary>
    /// 指定された地面の種類の足音を再生します。(★追加)
    /// </summary>
    /// <param name="position">足音を再生する位置</param>
    /// <param name="surfaceType">地面の種類</param>
    public void PlayFootstep(Vector3 position, SurfaceType surfaceType, CatSystem.AnimState stateType)
    {
            // Dictionaryから足音クリップの配列を取得
            if (stateDic[stateType].TryGetValue(surfaceType, out AudioClip[] clips))
            {
                // 配列が空でなく、中身があれば処理
                if (clips.Length > 0)
                {
                    // 配列からランダムに一つのクリップを選択
                    AudioClip clipToPlay = clips[UnityEngine.Random.Range(0, clips.Length)];

                    // 選択したクリップを再生
                    PlaySoundAtPoint(clipToPlay, position, footstepMixerGroup);
                }
            }
    }
    #endregion

    #region Private Methods
    private void SetVolume(string parameterName, string prefsKey, float normalizedVolume)
    {
        float mixerVolume = normalizedVolume > 0.0001f ? Mathf.Log10(normalizedVolume) * 20 : -80;
        mainMixer.SetFloat(parameterName, mixerVolume);
        PlayerPrefs.SetFloat(prefsKey, normalizedVolume);
        PlayerPrefs.Save();
    }
    
    private void LoadAndApplyVolume(string parameterName, string prefsKey)
    {
        float volume = PlayerPrefs.GetFloat(prefsKey, 1.0f);
        float mixerVolume = volume > 0.0001f ? Mathf.Log10(volume) * 20 : -80;
        mainMixer.SetFloat(parameterName, mixerVolume);
    }
    #endregion
}