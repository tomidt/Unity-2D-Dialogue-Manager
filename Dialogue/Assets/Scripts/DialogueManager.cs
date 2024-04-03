using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class DialogueManager : MonoBehaviour
{
    public CharaLibSO characterLibrary;

    public KeyCode continueDialogue;
    public KeyCode exitDialogue;

    [System.Serializable]
    public struct DialogueEvent
    {
        public string eventName;
        public UnityEvent eventAction;
    }
    public DialogueEvent[] events;

    public UnityEvent startDialogue;
    public UnityEvent endDialogue;

    [HideInInspector] public bool isTalking;

    public DialogueConversationSO convo;

    private DialogueUI uiManager;
    private DialogueAudio audioManager;

    private DialogueConversationSO currentConvo;

    private int currentIndex;
    private bool waitingBranch;

    private void Awake()
    {
        uiManager = GetComponentInChildren<DialogueUI>();
        audioManager = GetComponentInChildren<DialogueAudio>();
        uiManager.InitDialogueUI();
        isTalking = false;

        StartDialogueConvo(convo);
    }

    private void Update()
    {
        if (!isTalking)
            return;

        if (Input.GetKeyDown(exitDialogue))
        {
            ExitDialogue();
            return;
        }

        if (!Input.GetKeyDown(continueDialogue))
            return;

        if (waitingBranch)
            return;

        if (uiManager.IsDisplayingDialogue())
        {
            uiManager.SkipDialogue();
            return;
        }

        if (currentIndex < currentConvo.conversation.Length - 1)
        {
            currentIndex++;
            PlayDialogue();
        }  
        else
            ExitDialogue();
    }

    public void StartDialogueConvo(DialogueConversationSO dialogue)
    {
        currentConvo = dialogue;
        currentIndex = 0;

        audioManager.InitDialogueAudio(currentConvo.volume);
        uiManager.EnableUI(true);

        isTalking = true;
        waitingBranch = false;
        startDialogue.Invoke();

        PlayDialogue();
    }

    public void ExitDialogue()
    {
        EndDialogue();
    }

    public void SelectOption(int option)
    {
        endDialogue.Invoke();
        StartDialogueConvo(currentConvo.branchOptions[option].dialogConversation);
    }

    private void PlayDialogue()
    {
        if (currentConvo == null)
            throw new Exception($"No dialogue conversation exist");

        if (currentConvo.conversation[currentIndex].branch)
            StartBranch();
        else
            StartDialogue();

        CheckEvents();
    }

    private void StartBranch()
    {
        CharacterSO chara = characterLibrary.GetCharacter(currentConvo.conversation[currentIndex].charName);
        if (chara == null)
            chara = characterLibrary.GetCharacter(currentConvo.conversation[currentIndex - 1].charName);

        List<string> optionTextArr = new List<string>();
        for (int i = 0; i < currentConvo.branchOptions.Length; i++)
            optionTextArr.Add(currentConvo.branchOptions[i].optionText);

        uiManager.SetOptions(optionTextArr.ToArray(), chara.font);
        waitingBranch = true;
    }

    private void StartDialogue()
    {
        CharacterSO chara = characterLibrary.GetCharacter(currentConvo.conversation[currentIndex].charName);
        if (chara == null)
            throw new Exception($"Character ({currentConvo.conversation[currentIndex].charName}) does not exist");
        
        uiManager.DisplayDialogue(
            chara.charName,
            chara.speed,
            chara.pitch,
            chara.font,
            chara.sfx,
            chara.GetExpression(currentConvo.conversation[currentIndex].expression),
            currentConvo.conversation[currentIndex].volumeMod,
            currentConvo.conversation[currentIndex].dialogueText
        );
    }

    private void EndDialogue()
    {
        uiManager.SkipDialogue();
        uiManager.EnableUI(false);
        isTalking = false;
        endDialogue.Invoke();
    }

    private void CheckEvents()
    {
        if (events == null)
            return;

        if (currentConvo.conversation[currentIndex].events == null)
            return;

        foreach (string ev in currentConvo.conversation[currentIndex].events)
        {
            foreach (DialogueEvent de in events)
            {
                if (de.eventName == null || de.eventAction == null)
                    continue;

                if (ev == de.eventName)
                    de.eventAction.Invoke();
            }
        }
    }

    /*
    //current dialogue to play
    public AdvancedDialogueSO currConvo;
    private Dialogue currNpc;
    [SerializeField] private SceneChanger sceneChanger;
    public DialogueEventManager dem;
    [SerializeField] private IntSO befriend; 
    private int index;
    private bool recurChara;
    private bool hasChoice = false;
    public bool talkStarted;

    //Referencing UI
    private GameObject dialogueCanvas;
    private TMP_Text chara;
    private Image portrait;
    private TMP_Text dText;
    private string currChara;
    private Sprite currSprite;
    public CharaSO[] charaSO;

    //Referencing buttons for options
    public GameObject[] options;
    private TMP_Text[] optionText;
    private GameObject optionPanel; 

    //used to freeze player during dialogue
    private PlayerComponent pc;

    // display and speech text
    public AudioSource audioSource;
    public List<AudioClip> robotBeeps;
    public FloatSO talkingSpeed;
    private bool isDisplayingText = false;
    private bool audioEnabled;
    private float currPitch;

    // stop spam clicking space reactivate dialog after finished
    private bool justFinished;

    void Start()
    {
        optionPanel = GameObject.Find("OptionsPanel");
        optionPanel.SetActive(false);
        // Set up optionText array based on these buttons
        optionText = new TMP_Text[options.Length];
        for (int i = 0; i < options.Length; i++)
            optionText[i] = options[i].GetComponentInChildren<TMP_Text>();
        for (int i = 0; i < options.Length; i++)
            options[i].SetActive(false);
        //Other UI elements
        dialogueCanvas = GameObject.Find("DialogueCanvas");
        chara = GameObject.Find("CharText").GetComponent<TMP_Text>();
        portrait = GameObject.Find("Portrait").GetComponent<Image>();
        dText = GameObject.Find("DialogueText").GetComponent<TMP_Text>();
        pc = GameObject.Find("Player").GetComponent<PlayerComponent>();
        dialogueCanvas.SetActive(false);

        // audio
        audioEnabled = (audioSource != null) && (robotBeeps.Count > 0);

        justFinished = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && currNpc != null && currNpc.inRange && !justFinished)
        {
            if (!hasChoice)
            {
                pc.toggleFreeze(true);

                if (isDisplayingText)
                {
                    isDisplayingText = false;
                    return;
                }

                if (index < currConvo.charaInfo.Length)
                {
                    PlayDialogue();
                }
                else
                {
                    IncrementDialogue(currNpc);
                    TurnOffDialogue();
                }
            }
        }
    }

    void PlayDialogue()
    {   
        talkStarted = true;
        // Set basic information
        dialogueCanvas.SetActive(true);
        if(currConvo.charaInfo[index].chara == DialogueCharacters.Random)
            recurChara = false;
        else 
            recurChara = true;

        // Check if it's a branch or event
        if(currConvo.charaInfo[index].chara == DialogueCharacters.Branch)
        {
            hasChoice = true;
            ShowOptions();
        }
        else if(currConvo.charaInfo[index].chara == DialogueCharacters.Event)
        {
            CheckEvent();
        }
        else
        {
            // Set Actor Info and Dialogue Text for regular lines
            SetActorInfo(currConvo.charaInfo[index].chara, currConvo.charaInfo[index].expression);
            chara.text = currChara;
            portrait.sprite = currSprite;
            //dText.text = currConvo.charaInfo[index].dialogue;
            //Debug.Log(currConvo.charaInfo[index].dialogue);
            StartCoroutine(displayText(currConvo.charaInfo[index].dialogue));
            
            index++;  // Increment the index only if it's not a branch or event
        }
    }

    private IEnumerator displayText(string text)
    {
        isDisplayingText = true;
        bool formating = false;
        int currentChar = 0;
        System.Random rnd = new System.Random();
        audioSource.pitch = currPitch;

        while (isDisplayingText)
        {
            dText.text = text.Substring(0, currentChar + 1);

            switch(text[currentChar])
            {
                case ' ':
                    break;
                case '<':
                    formating = true;
                    break;
                case '>':
                    formating = false;
                    break;
                case '…':
                    // some of the worst code ive written in my life
                    for (int i = 0; i < 10; i++)
                    {
                        yield return new WaitForSeconds(talkingSpeed.value);
                        if (!isDisplayingText)
                            goto ExitEarly;
                    }
                    break;
                case '.':
                case '!':
                case '?':
                case ',':
                    for (int i = 0; i < 5; i++)
                    {
                        yield return new WaitForSeconds(talkingSpeed.value);
                        if (!isDisplayingText)
                            goto ExitEarly;
                    }
                    break;
                default:
                    if (audioEnabled)
                    {
                        if (formating)
                            break;

                        audioSource.clip = robotBeeps[rnd.Next(0, robotBeeps.Count)];
                        audioSource.Play();
                    }
                    yield return new WaitForSeconds(talkingSpeed.value);
                    break;
            }
            currentChar++;
            if (currentChar >= text.Length)
                isDisplayingText = false;
        }

        ExitEarly:
            isDisplayingText = false;

        dText.text = text;
    }

    void SetActorInfo(DialogueCharacters chara, ExpressionType expression)
    {
        if(recurChara && chara != DialogueCharacters.Branch)
        {
            for (int i = 0; i < charaSO.Length; i++)
            {
                if (charaSO[i].charName == currConvo.charaInfo[index].chara.ToString())
                {
                    currChara = charaSO[i].charName;
                    currSprite = charaSO[i].GetSprite(currConvo.charaInfo[index].expression);
                    currPitch = charaSO[i].pitch;
                }
            }
        }
        if (chara == DialogueCharacters.Branch)
        {
            hasChoice = true;
            ShowOptions();
        }
        if(!recurChara)
        {
            currChara = currConvo.randomName;
            currSprite = currConvo.randomPortrait;
        }
    }

    public void CheckEvent()
    {
        // HANDLES EVENT CHECK //
        if(currConvo.charaInfo[index].chara == DialogueCharacters.Event)
        {
            if(currConvo.enemyName != null)
            {
                sceneChanger.startEncounter(currConvo.enemyName.value, false);
            }
            if(currConvo.sceneName != null)
            {
                sceneChanger.changeScene(currConvo.sceneName.value.ToString());
            }
            if(currConvo.friends)
            {
                befriend.value += 1;
            }
            if(currConvo.eventInfo != null)
            {
               foreach (var spriteEvent in currConvo.eventInfo)
                {
                    if (spriteEvent.newSprite || spriteEvent.disableObject || spriteEvent.invis)
                    {
                        dem.TriggerEvent(spriteEvent.eventName);
                        if (index >= currConvo.charaInfo.Length - 1 && currConvo.cutscene == null)
                        {
                            TurnOffDialogue();
                        }
                    }
                } 
            }
            if(currConvo.cutscene != null)
            {
                currConvo = currConvo.cutscene;
                index = 0;
                PlayDialogue();
            }
            if(currConvo.endConvo.hasMeaning)
            {
                if(befriend.value > 2)
                {
                    currConvo = currConvo.endConvo.More;
                }
                if(befriend.value == 2)
                {
                    currConvo = currConvo.endConvo.Equal;
                }
                if(befriend.value < 2)
                {
                    currConvo = currConvo.endConvo.Less;
                }
                index = 0;
                PlayDialogue();
            }
        }
    }

    public void ShowOptions()
    {

        // PRESENTING OPTIONS //
        optionPanel.SetActive(true);

        // Reset all buttons to inactive before activating the correct ones
        foreach (var option in options)
            option.SetActive(false);

        for (int i = 0; i < options.Length && i < currConvo.optionText.Length; i++)
        {
            if (!string.IsNullOrEmpty(currConvo.optionText[i]))
            {
                optionText[i].text = currConvo.optionText[i];
                options[i].SetActive(true);
            }
            else
            {
                optionPanel.SetActive(false);
            }
        }
    }

    public void Option(int optionNum)
    {
        foreach(GameObject button in options)
            button.SetActive(false);
        switch(optionNum)
        {
            case 0: currConvo = currConvo.option0; break;
            case 1: currConvo = currConvo.option1; break;
            case 2: currConvo = currConvo.option2; break;
            case 3: currConvo = currConvo.option3; break;
        }
        index = 0;
        hasChoice = false;
        optionPanel.SetActive(false);
    }

    // BELOW SCRIPTS USED BY DIALOGUE.CS/
    public void InitiateDialogue(Dialogue npc)
    {
        if (npc != null && npc.convo != null)
        {
            currNpc = npc;
            currConvo = npc.convo[npc.convo_index];
            index = 0; // Reset index for new convo
        }
    }

    public void TurnOffDialogue()
    {
        index = 0;
        talkStarted = false;
        dialogueCanvas.SetActive(false);
        optionPanel.SetActive(false);
        pc.toggleFreeze(false);
        currConvo = currNpc.convo[currNpc.convo_index];

        StartCoroutine(justFinishedTalking());
    }

    private IEnumerator justFinishedTalking()
    {
        justFinished = true;

        yield return new WaitForSeconds(1f);

        justFinished = false;

    }

    public void IncrementDialogue(Dialogue npc)
    {
        if (npc != null && npc.convo_index < npc.convo.Length - 1)
        {
            npc.convo_index++;
        }
    }*/
}
