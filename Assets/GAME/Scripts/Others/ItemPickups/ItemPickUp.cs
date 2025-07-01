using UnityEngine;
using UnityEngine.Events;

namespace EternalKeep
{
    public class ItemPickUp : MonoBehaviour
    {
        [SerializeField] protected int iD = -1;

        //items like health can be respawned after reset, but door keys wont since once a door is opened, it stays open
        [Tooltip("Set to true if the item can respawn after reset even if it picked up before, false if it cannot (e.g., door keys)")]
        public bool canRespawnAfterPickUp = true;

        public int GetID => iD;
        public string itemName;
        public string itemDescription;
        public Sprite itemIcon;

        public bool isItemPickedUp;

        [Tooltip("set true only for noteItem Pickup")]
        public bool isNoteItemPickup = false;

        [SerializeField] GameObject pickUpGameObject;

        [Space]
        [SerializeField] UnityEvent onItemPickedUp;

        public virtual void PickUpItem()
        {
            isItemPickedUp = true;
            SaveSystem.SaveGame();
            onItemPickedUp?.Invoke();
            pickUpGameObject.SetActive(false);

        }

        public virtual void ResetItemPickUp()
        {
            if (isItemPickedUp && !canRespawnAfterPickUp) return;
            isItemPickedUp = false;
            pickUpGameObject.SetActive(true);
        }

        #region SAVE/LOAD

        public virtual void SaveItemData(ref ItemPickUpData itemData)
        {
            itemData.isPickedUp = isItemPickedUp;

        }

        public virtual void LoadItemPickUpData(ItemPickUpData itemData)
        {
            isItemPickedUp = itemData.isPickedUp;

            if (isItemPickedUp)
            {
                pickUpGameObject.SetActive(false); // Disable the item if it has been picked up
            }
            else
            {
                pickUpGameObject.SetActive(true); // Enable the item if it has not been picked up
            }
        }

        public virtual void ResetItemPickUpDataSaves(ref ItemPickUpData itemData)
        {
            if (isItemPickedUp && !canRespawnAfterPickUp)
            {
                itemData.isPickedUp = true;
                return; // Do not reset if the item cannot respawn
            }

            itemData.isPickedUp = false; // Reset the pickup status
        }

        #endregion
    }

    [System.Serializable]
    public struct ItemPickUpData
    {
        public bool isPickedUp;
    }


}


