using UnityEngine;

namespace EternalKeep
{
    public class NoteItemPickUp : ItemPickUp, IUsableItem
    {
        [SerializeField] string noteText = "note";

        [SerializeField] HandleNoteUI noteUIHandler;

        public void UseItem()
        {
            noteUIHandler.SetNoteText(noteText);


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

