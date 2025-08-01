using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    Animator animator;

    public float blendSpeed = 6f;

    public float targetX;
    public float targetZ;

    public float currentX;
    public float currentZ;

    [SerializeField] float animTransitionDuration = 1f;

    [SerializeField] AnimationClip stunAnimClip;
    [SerializeField] AnimationClip damageAnimClip;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Trigger strafe direction
        if (Input.GetKeyDown(KeyCode.V))
        {
            SetStrafeAnimatorValues_Front();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            SetStrafeAnimatorValues_Back();
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            ChangeAnim(stunAnimClip.name);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            ChangeAnim(damageAnimClip.name);
        }

        // Smoothly blend to target
        UpdateAnimatorValues();
    }

    private void SetStrafeAnimatorValues_Front()
    {
        targetX = 0f;
        targetZ = 1f;
    }

    private void SetStrafeAnimatorValues_Back()
    {
        targetX = 0f;
        targetZ = -1f;
    }

    private void UpdateAnimatorValues()
    {
        currentX = Mathf.Lerp(currentX, targetX, Time.deltaTime * blendSpeed);
        currentZ = Mathf.Lerp(currentZ, targetZ, Time.deltaTime * blendSpeed);

        animator.SetFloat("X_Velocity", currentX);
        animator.SetFloat("Z_Velocity", currentZ);
    }

    private void ChangeAnim(string animationName)
    {
        animator.CrossFade(animationName, animTransitionDuration);
    }
}
