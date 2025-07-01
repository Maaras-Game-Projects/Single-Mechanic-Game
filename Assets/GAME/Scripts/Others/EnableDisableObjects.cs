using UnityEngine;

namespace EternalKeep
{
    public class EnableDisableObjects : MonoBehaviour
    {
        [SerializeField] private GameObject[] objectsToEnable;
        [SerializeField] private GameObject[] objectsToDisable;

        public void EnableObjects()
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }

        public void DisableObjects()
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }

        public void EnableObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        public void DisableObject(GameObject obj)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }

}

