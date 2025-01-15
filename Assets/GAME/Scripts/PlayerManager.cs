using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] PlayerLocomotion playerLocomotion;
    [SerializeField] MyInputManager myInputManager;

    private void Update()
    {
        myInputManager.HandleAllInput();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        if(myInputManager.walkInput)
        {
            playerLocomotion.isWalking = true;
        }
        else
        {
            playerLocomotion.isWalking = false;
        }
    }
}
