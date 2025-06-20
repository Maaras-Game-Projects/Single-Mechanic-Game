using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioObject : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;

    AudioMixerGroup sfxAudioMixerGroup;
    AudioMixerGroup bgmAudioMixerGroup;

    IObjectPool<AudioObject> audioObjectPool;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetAudioObjectPool(IObjectPool<AudioObject> objectPool)
    {
        audioObjectPool = objectPool;
    }

    public void SetSFXAudioMixer(AudioMixerGroup audioMixerGroup)
    {
        sfxAudioMixerGroup = audioMixerGroup;
    }

    public void SetBGMAudioMixer(AudioMixerGroup audioMixerGroup)
    {
        bgmAudioMixerGroup = audioMixerGroup;
    }

    public void PlaySFXAudio(AudioClip clip, Vector3 position, float volume = 1)
    {
        audioSource.outputAudioMixerGroup = sfxAudioMixerGroup;
        audioSource.clip = clip;
        audioSource.spatialBlend = 1; //3d sound
        transform.position = position;
        audioSource.volume = volume;
        audioSource.Play();
        float waitTime = clip.length;
        
        StartCoroutine(audioCompleteCoroutine(waitTime));
    }

    public void PlayBGMAudio(AudioClip clip, Vector3 position, float volume = 1)
    {
        audioSource.outputAudioMixerGroup = bgmAudioMixerGroup;
        audioSource.clip = clip;
        audioSource.spatialBlend = 0; //2d sound
        transform.position = position;
        audioSource.volume = volume;
        audioSource.Play();
        float waitTime = clip.length;
        StartCoroutine(audioCompleteCoroutine(waitTime));
    }

    public void PlaySFXAudio(AudioClip clip, Vector3 position, float volume = 1, float pitch = 0.5f, float spatialBlend = 1f)
    {
        audioSource.outputAudioMixerGroup = sfxAudioMixerGroup;
        audioSource.clip = clip;
        audioSource.pitch = pitch;
        audioSource.spatialBlend = spatialBlend; //1 for 3d sound
        transform.position = position;
        audioSource.volume = volume;
        audioSource.Play();
        float waitTime;
        if (pitch > 0)
        {
            waitTime = clip.length / pitch; ;
        }
        else
        {
            waitTime = clip.length;
        }
         
        StartCoroutine(audioCompleteCoroutine(waitTime));
    }

    // need to add PlayUISFxAudio() method
    IEnumerator audioCompleteCoroutine(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        audioObjectPool.Release(this);
    }
}
