using UnityEngine;

namespace EternalKeep
{
    public class Teleporter : MonoBehaviour
    {
        [SerializeField] Transform teleportEndPoint;
        void OnTriggerEnter(Collider other)
        {
            if (other == null) return;
            if (other.gameObject.tag == "Player Weapon") return;
            other.gameObject.transform.position = teleportEndPoint.transform.position;
            ;
        }
    }

}

