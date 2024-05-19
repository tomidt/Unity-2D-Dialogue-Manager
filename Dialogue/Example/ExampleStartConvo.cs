using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleStartConvo : MonoBehaviour
{
    public DialogueManager dm;
    public DialogueConversationSO dialogue;

    private void Awake()
    {
        if (dm == null || dialogue == null)
            return;

        dm.StartDialogueConvo(dialogue);
    }
}
