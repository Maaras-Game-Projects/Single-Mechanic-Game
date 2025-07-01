using UnityEngine;

namespace EternalKeep
{
    public class FootstepsAudioPlayer : MonoBehaviour
    {
        [SerializeField] Animator animator;

        [SerializeField] PlayerAnimationManager playerAnimationManager;
        [SerializeField] PlayerLocomotion playerLocomotion;
        [SerializeField] PlayerCombat playerCombat;

        [SerializeField] string footStepParamString;
        [SerializeField] float lastFootStep = 0;

        [SerializeField] FootStepData footStepData;
        [SerializeField] LayerMask groundLayerMask;

        [SerializeField] bool useDefaultFootStepsOnly = false;

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
                AudioClipDataFootSteps footStepAudioData = GetFootStepDataBySurface();

                int randomIndex = Random.Range(0, footStepAudioData.audioClips.Length);
                AudioClip clip = footStepAudioData.audioClips[randomIndex];
                Vector3 position = transform.position;
                float volume = footStepAudioData.volume;
                float pitch = footStepAudioData.pitch;
                float randomPitch = Random.Range(pitch - footStepAudioData.pitchRangeModifier,
                    pitch + footStepAudioData.pitchRangeModifier);
                float spatialBlend = footStepAudioData.spatialBlendVal;

                AudioManager_STN.instance.PlayAudio_SFX_General(clip, position, volume,
                randomPitch, spatialBlend);

            }

            lastFootStep = currentFootstep;
        }

        // Need to update this method to handle different surfaces when adding more surface types
        AudioClipDataFootSteps GetFootStepDataBySurface()
        {
            //Debug.DrawRay(transform.position, Vector3.down * 5f, Color.red, 5f);
            if (useDefaultFootStepsOnly)
            {
                return footStepData.defaultSurfaceAudioSetting;
            }

            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 5f, groundLayerMask))
            {

                if (hit.collider != null)
                {
                    SurfaceDefinition surfaceDefinition = hit.collider.GetComponent<SurfaceDefinition>();
                    if (surfaceDefinition != null)
                    {

                        switch (surfaceDefinition.surfaceType)
                        {
                            case SurfaceType.Grass:
                                return footStepData.grassFootStepAudioSetting;
                            default:
                                return footStepData.defaultSurfaceAudioSetting; // Fallback to default
                        }
                    }
                    else
                    {
                        return footStepData.defaultSurfaceAudioSetting; // Fallback to default
                    }

                }
            }

            return footStepData.defaultSurfaceAudioSetting;
        }

    #if UNITY_EDITOR

        void OnDrawGizmos()
        {
            // Gizmos.color = Color.red;
            // Gizmos.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * 5f);
        }
    #endif
    }

}
