using UnityEngine;
using UnityEngine.Events;

public class BossTriggerZone : MonoBehaviour
{
  [SerializeField]  private UnityEvent onZoneEnter;

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            // enable respective boss music,
            //handle fogwall logic
            //handle save state
            //handle boss activation and ui activation
            onZoneEnter?.Invoke();
        }
    }
}
