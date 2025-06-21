using UnityEngine;

public class FootstepsAudioPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] string footStepParamString;
    [SerializeField] float lastFootStep = 0;

    [SerializeField] AudioClip[] audioClips;

    void Update()
    {
        if (animator == null) return;

        float currentFootstep = animator.GetFloat(footStepParamString);

        if (lastFootStep < 0 && currentFootstep > 0 || lastFootStep > 0 && currentFootstep < 0)
        {
            int randomIndex = Random.Range(0, audioClips.Length);
            AudioManager_STN.instance.PlayAudio_SFX_General(audioClips[randomIndex], transform.position, 1
            , 0);
        }

        lastFootStep = currentFootstep;
    }
}
