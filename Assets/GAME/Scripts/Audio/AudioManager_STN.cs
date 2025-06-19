using System;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioManager_STN : MonoBehaviour
{
    public static AudioManager_STN instance;

    private IObjectPool<AudioObject> audioObjectPool;

    [SerializeField] AudioDataSO audioDataSO;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        InitialiseAudioObjectPool();
    }

    private void InitialiseAudioObjectPool()
    {
        audioObjectPool = new ObjectPool<AudioObject>(CreateAudioObject,
                actionOnGet: obj => obj.gameObject.SetActive(true),
                actionOnRelease: obj => obj.gameObject.SetActive(false),
                actionOnDestroy: obj => Destroy(obj),
                maxSize: 30
                );
    }

    private AudioObject CreateAudioObject()
    {
        AudioObject obj = Instantiate(audioDataSO.audioObject);
        obj.SetAudioObjectPool(audioObjectPool);
        obj.SetSFXAudioMixer(audioDataSO.audioMixerGroup_SFX);
        obj.SetBGMAudioMixer(audioDataSO.audioMixerGroup_BGM);
        return obj;
    }

    public static void PlayAudio_SFX(AudioClip audioClip, Vector3 position, float volume = 1)
    {
        AudioObject audioObject = instance.audioObjectPool.Get();
        audioObject.PlaySFXAudio(audioClip, position, volume);
    }

    public static void PlayAudio_BGM(AudioClip audioClip, Vector3 position, float volume = 1)
    {
        AudioObject audioObject = instance.audioObjectPool.Get();
        audioObject.PlayBGMAudio(audioClip, position, volume);
    }
    
     // need to add PlayUISFxAudio() method
}
