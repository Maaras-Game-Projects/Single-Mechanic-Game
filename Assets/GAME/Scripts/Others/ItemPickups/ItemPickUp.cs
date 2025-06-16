using UnityEngine;

public class ItemPickUp : MonoBehaviour
{
    [SerializeField] protected int iD = -1;

    public int GetID => iD;
    public string itemName;
    public string itemDescription;
    public Sprite itemIcon;
}
