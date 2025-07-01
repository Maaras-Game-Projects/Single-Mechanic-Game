using UnityEngine;

namespace EternalKeep
{
    public class RotateCanvas_WS : MonoBehaviour
    {
        [SerializeField] Camera mainCamera;

        void LateUpdate()
        {
            transform.LookAt(mainCamera.transform);
            //transform.Rotate(0f, 180f, 0f); // Flip if needed (depends on your canvas facing direction)
        }
    }

}

