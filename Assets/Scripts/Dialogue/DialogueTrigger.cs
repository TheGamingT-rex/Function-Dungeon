using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;

    public void Start()
    {
        if (dialogue == null)
        {
            Destroy(this);
        }
    }
    public virtual void TriggerDialogue()
    {
        Globals.DialogueManager.StartDialogue(dialogue);
    }
}
