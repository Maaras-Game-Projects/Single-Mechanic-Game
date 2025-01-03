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
}
