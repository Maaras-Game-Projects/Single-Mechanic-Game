using UnityEngine;

public class NoteItemPickUp : ItemPickUp, IUsableItem
{
    [SerializeField] string noteText = "note";

    [SerializeField] HandleNoteUI noteUIHandler;

    public void UseItem()
    {
        noteUIHandler.SetNoteText(noteText);
        
        
    }

    
}
