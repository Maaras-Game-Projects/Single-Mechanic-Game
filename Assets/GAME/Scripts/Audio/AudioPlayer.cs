using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    public class AudioPlayer : MonoBehaviour
    {
        [SerializeField] List<AudioClipData> audioClipDatas = new List<AudioClipData>();
        [SerializeField] CommonAudioDataSO commonAudioDataSO;


        public void PlayAudioClip(int clipIndex)
        {
            if (audioClipDatas.Count == 0) return;

            if (clipIndex < 0 || clipIndex >= audioClipDatas.Count)
            {
                //Debug.Log("Not a valid audio index");
                return;
            }
            AudioClip clip = audioClipDatas[clipIndex].clip;
            float fixedPitch = audioClipDatas[clipIndex].pitch;
            float pitchModifier = audioClipDatas[clipIndex].pitchRangeModifier;
            float volume = audioClipDatas[clipIndex].volume;
            float spatialBlend = audioClipDatas[clipIndex].spatialBlendVal;

            Vector3 position = Vector3.zero;
            if (audioClipDatas[clipIndex].useOwnPosition)
            {
                position = transform.position;
            }
            else
            {
                position = audioClipDatas[clipIndex].position;
            }

            float randomPitch = Random.Range(fixedPitch - pitchModifier, fixedPitch + pitchModifier);

            AudioManager_STN.instance.PlayAudio_SFX_General(clip, position, volume, randomPitch, spatialBlend);
        }
        
        public void PlayCommonAudioClip(int clipIndex)
        {
            if (commonAudioDataSO == null || commonAudioDataSO.allCommonAudios.Count == 0) return;

            if (clipIndex < 0 || clipIndex >= commonAudioDataSO.allCommonAudios.Count)
            {
                //Debug.Log("Not a valid audio index");
                return;
            }
            AudioClip clip = commonAudioDataSO.allCommonAudios[clipIndex].clip;
            float fixedPitch = commonAudioDataSO.allCommonAudios[clipIndex].pitch;
            float pitchModifier = commonAudioDataSO.allCommonAudios[clipIndex].pitchRangeModifier;
            float volume = commonAudioDataSO.allCommonAudios[clipIndex].volume;
            float spatialBlend = commonAudioDataSO.allCommonAudios[clipIndex].spatialBlendVal;

            Vector3 position = Vector3.zero;
            if (commonAudioDataSO.allCommonAudios[clipIndex].useOwnPosition)
            {
                position = transform.position;
            }
            else
            {
                position = commonAudioDataSO.allCommonAudios[clipIndex].position;
            }

            float randomPitch = Random.Range(fixedPitch - pitchModifier, fixedPitch + pitchModifier);

            AudioManager_STN.instance.PlayAudio_SFX_General(clip, position, volume, randomPitch, spatialBlend);
        }
    }

    [System.Serializable]

    public class AudioClipData
    {
        public string name;
        public AudioClip clip;
        public float pitch = 1f;

        public float pitchRangeModifier = 0.25f;

        public float volume = 1f;

        public float spatialBlendVal = 0.5f;

        public Vector3 position;

        public bool useOwnPosition = false;

    }

    [System.Serializable]

    public class AudioClipDataFootSteps
    {
        public float pitch = 1f;

        public float pitchRangeModifier = 0.25f;

        public float volume = 1f;
        public float spatialBlendVal = 0.5f;

        public AudioClip[] audioClips;
    }

}

