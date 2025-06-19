using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] List<AudioClipData> audioClipDatas = new List<AudioClipData>();

    public void PlayAudioClip(int clipIndex)
    {
        if (audioClipDatas.Count == 0) return;

        if (clipIndex < 0 || clipIndex >= audioClipDatas.Count)
        {
            Debug.Log("Not a valid audio index");
            return;
        }
        AudioClip clip = audioClipDatas[clipIndex].clip;
        float pitch = audioClipDatas[clipIndex].pitch;
        float volume = audioClipDatas[clipIndex].volume;
        float spatialBlend = audioClipDatas[clipIndex].spatialBlendVal;
        Vector3 position = audioClipDatas[clipIndex].position;

        AudioManager_STN.instance.PlayAudio_SFX_General(clip, position, volume, pitch, spatialBlend);
    }
}

[System.Serializable]

public class AudioClipData
{
    public AudioClip clip;
    public float pitch = 0.5f;

    public float volume = 0.75f;

    public float spatialBlendVal = 0.5f;

    public Vector3 position;

}
