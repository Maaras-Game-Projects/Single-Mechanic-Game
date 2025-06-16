using UnityEngine;

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

}
