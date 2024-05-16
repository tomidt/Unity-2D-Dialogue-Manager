using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

[CreateAssetMenu(menuName = "DialogueSO/DialogueConversation")]
public class DialogueConversationSO : ScriptableObject
{  
    [System.Serializable]
    public struct DialogueLine
    {
        // if true, will pull up branch options instead
        // only a single branch per DialogueConversation
        // branch can only be the last DialogLine
        public bool branch;

        public Character charName;
        public Expression expression;

        // additive volume modifier ; 0.2 = 120% ; -0.4 = 60%
        public float volumeMod;
        
        [TextAreaAttribute]
        public string dialogueText;

        // events will trigger when this dialogue is reached
        public string[] events;
    }

    [System.Serializable]
    public struct BranchOption
    {
        public string optionText;
        public DialogueConversationSO dialogConversation;
    }

    public float volume;
    public DialogueLine[] conversation;
    public BranchOption[] branchOptions;
    public DialogueConversationSO nextConvo;
}