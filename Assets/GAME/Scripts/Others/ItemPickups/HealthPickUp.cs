using UnityEngine;

namespace EternalKeep
{
    public class HealthPickUp : ItemPickUp, IUsableItem
    {
        [SerializeField] PlayerHealth playerHealth;
        [SerializeField] int potionCountValue = 1;
        public void UseItem()
        {
            playerHealth.IncrementHealthPotionCount(potionCountValue);

            //Add sound effect
        }

        public void DisablePickUpObject()
        {

            gameObject.SetActive(false);
        }

        // #region SAVE/LOAD

        // public override void SaveItemData(ref ItemPickUpData itemData)
        // {
        //     base.SaveItemData(ref itemData);
        // }

        // public override void LoadItemPickUpData(ItemPickUpData itemData)
        // {
        //     base.LoadItemPickUpData(itemData);
        // }

        // public override void ResetItemPickUpDataSaves(ref ItemPickUpData itemData)
        // {
        //     base.ResetItemPickUpDataSaves(ref itemData);
        // }

        // #endregion

    }

}

