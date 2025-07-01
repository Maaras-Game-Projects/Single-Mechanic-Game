
using UnityEngine;
using UnityEngine.Audio;

namespace EternalKeep
{
    [CreateAssetMenu(fileName = "AudioData", menuName = "AudioData")]
    public class AudioDataSO : ScriptableObject
    {
        public AudioObject audioObject;
        public AudioMixerGroup audioMixerGroup_SFX;
        public AudioMixerGroup audioMixerGroup_BGM;

        [Range(0,1)]
        public float default_SFX_Volume = 0.75f;

        [Range(0,1)]
        public float default_BGM_Volume = 0.75f;
        
    }

}

