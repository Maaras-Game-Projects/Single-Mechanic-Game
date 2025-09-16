using System.Collections.Generic;
using UnityEngine;

namespace EternalKeep
{
    [CreateAssetMenu(fileName ="CommonAudioContainer",menuName ="ScriptableObjects/CommonAudio")]
    public class CommonAudioDataSO : ScriptableObject
    {
        public List<AudioClipData> allCommonAudios = new List<AudioClipData>();
    }


}

