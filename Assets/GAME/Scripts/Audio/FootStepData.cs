using UnityEngine;

namespace EternalKeep
{
    [CreateAssetMenu(fileName = "FootStepData", menuName = "ScriptableObjects/FootStepData")]
    public class FootStepData : ScriptableObject
    {
        public AudioClipDataFootSteps defaultSurfaceAudioSetting;
        public AudioClipDataFootSteps grassFootStepAudioSetting;

    }

}

