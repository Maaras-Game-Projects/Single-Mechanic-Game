using UnityEngine;

public class FootstepsAudioPlayer : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] PlayerAnimationManager playerAnimationManager;
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] PlayerCombat playerCombat;

    [SerializeField] string footStepParamString;
    [SerializeField] float lastFootStep = 0;

    [SerializeField] AudioClipData[] audioClips;

    void Update()
    {
        if (animator == null) return;

        if (playerAnimationManager.inAnimActionStatus) return;
        if (playerLocomotion.isDodging) return;
        if (!playerLocomotion.isGrounded) return;
        if (playerAnimationManager.rootMotionUseStatus && !playerCombat.isBlocking) return;

        float currentFootstep = animator.GetFloat(footStepParamString);

        if (lastFootStep < 0 && currentFootstep > 0 || lastFootStep > 0 && currentFootstep < 0)
        {
            int randomIndex = Random.Range(0, audioClips.Length);
            AudioClip clip = audioClips[randomIndex].clip;
            Vector3 position = audioClips[randomIndex].position;
            float volume = audioClips[randomIndex].volume;
            float pitch = audioClips[randomIndex].pitch;
            float spatialBlend = audioClips[randomIndex].spatialBlendVal;
            AudioManager_STN.instance.PlayAudio_SFX_General(clip, position, volume,
            pitch, spatialBlend);
        }

        lastFootStep = currentFootstep;
    }
}
