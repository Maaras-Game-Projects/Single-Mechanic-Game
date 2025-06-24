using UnityEngine;

public class PickUpItemDataContainer : MonoBehaviour
{
    [SerializeField] public ItemPickUp[] itemPickUps;

    public ItemPickUp[] GetItemPickUps
    {
        get { return itemPickUps; }
    }
}
