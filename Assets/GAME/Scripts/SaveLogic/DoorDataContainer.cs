using UnityEngine;

namespace EternalKeep
{
    public class DoorDataContainer : MonoBehaviour
    {
        [SerializeField] InteractableDoor[] doors;

        public InteractableDoor[] GetDoors
        {
            get { return doors; }
        }
    }


}
