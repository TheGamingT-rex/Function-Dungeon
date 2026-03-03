using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SpecialDialogueTrigger : DialogueTrigger {
    // This event can be used in the editor to trigger any custom behavior when the dialogue starts, such as playing a sound effect, changing the background music, or activating a cutscene.
    public UnityEvent onDialogueStart;
    
    public override void TriggerDialogue() {
        base.TriggerDialogue();
        onDialogueStart.Invoke();
    }
}
