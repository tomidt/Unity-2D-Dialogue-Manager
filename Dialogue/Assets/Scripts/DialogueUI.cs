using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [SerializeField] private DialogueAudio dialogueAudio;

    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private TMP_Text characterName;

    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Image dialoguePortrait;

    [SerializeField] private GameObject[] options;
    [SerializeField] private TMP_Text[] optionsText;

    private Character charName;
    private string dialogue;
    private TMP_FontAsset font;
    private Sprite portrait;
    private AudioClip[] sfx;
    private float volumeMod;
    private float speed;
    private float pitch;

    private bool skipDialogue;
    private bool displayingDialogue;

    public void InitDialogueUI()
    {
        DisableOptions();
        EnableUI(false);
        skipDialogue = false;
        displayingDialogue = false;
    }

    public void EnableUI(bool value)
    {
        dialogueCanvas.SetActive(value);
    }

    public void DisplayDialogue(Character cn, float sp, float pt, TMP_FontAsset ft, AudioClip[] sf, Sprite pr, float vm, string di)
    {
        charName = cn;
        dialogue = di;
        font = ft;
        portrait = pr;
        sfx = sf;
        volumeMod = vm;
        speed = sp;
        pitch = pt;

        DisableOptions();

        SetUIAssets();

        StartCoroutine(SetDialogueText());
    }

    public void SetOptions(string[] opTexts, TMP_FontAsset font)
    {
        DisableOptions();

        for (int i = 0; i < opTexts.Length; i++)
        {
            options[i].SetActive(true);

            if (font != null)
                optionsText[i].font = font;

            optionsText[i].text = opTexts[i];
        }
    }

    public bool IsDisplayingDialogue()
    {
        return displayingDialogue;
    }

    public void SkipDialogue()
    {
        skipDialogue = true;
    }

    private void DisableOptions()
    {
        options.ToList().ForEach(option => option.SetActive(false));
    }

    private void SetUIAssets()
    {
        if (charName != Character.None)
            characterName.text = charName.ToString();

        if (font != null)
            characterName.font = font;

        if (portrait != null)
            dialoguePortrait.sprite = portrait;
    }

    private IEnumerator SetDialogueText()
    {
        if (font != null)
            dialogueText.font = font;

        if (dialogue[0] == '*' || speed <= 0)
        {
            dialogueText.text = dialogue;
            yield break;
        }

        dialogueAudio.SetAudioProfile(sfx, pitch, volumeMod);

        List<FormatFlags> flagsList = ParseSetFlags();
        List<string> displayStrings = FormatStrings(flagsList);
        List<char> displayChars = GetDisplayChars(flagsList);

        displayingDialogue = true;

        for (int i = 0; i < displayChars.Count; i++)
        {
            if (skipDialogue)
                break;

            dialogueText.text = displayStrings[i];

            switch (displayChars[i])
            {
                case '?':
                case '!': 
                    dialogueAudio.PlaySingleNote();
                    goto case ',';
                case '.':
                case ',':
                    yield return StartCoroutine(ResponsiveSleep(speed * 5f));
                    break;
                default:
                    yield return new WaitForSeconds(speed);
                    dialogueAudio.PlaySingleNote();
                    break;
            }
        }

        dialogueText.text = dialogue;
        displayingDialogue = false;
        skipDialogue = false;

        yield return null;
    }

    private struct FormatFlags
    {
        public char c;
        public bool bold;
        public bool italic;
        public bool underline;
        public string color;
    }

    private List<FormatFlags> ParseSetFlags()
    {
        List<FormatFlags> flagsList = new List<FormatFlags>();
        FormatFlags parser = new FormatFlags()
        {
            bold = false,
            italic = false,
            underline = false,
            color = null
        };

        bool inTag = false;
        bool endTag = false;
        bool ignoreNext = false;

        foreach (char ch in dialogue)
        {
            if (ch == '\\')
            {
                ignoreNext = true;
                continue;
            }

            if ((ch == '<' || ch == '>') && !ignoreNext)
            {
                inTag = !inTag;
                endTag = false;
                continue;
            }

            if (!inTag)
                flagsList.Add(new FormatFlags
                {
                    c = ch,
                    bold = parser.bold,
                    italic = parser.italic,
                    underline = parser.underline,
                    color = parser.color
                });
            else
                switch (ch)
                {
                    case 'b':
                        parser.bold = !endTag;
                        break;
                    case 'i':
                        parser.italic = !endTag;
                        break;
                    case 'u':
                        parser.underline = !endTag;
                        break;
                    case '/':
                        endTag = true;
                        break;
                    case 'c':
                        Match colorMatch = Regex.Match(dialogue, @"<color\s*=\s*""(.*?)""");
                        parser.color = colorMatch.Success ? colorMatch.Groups[1].Value : null;
                        break;
                }

            ignoreNext = false;
        }

        return flagsList;
    }

    private List<string> FormatStrings(List<FormatFlags> flagsList)
    {
        List<string> output = new List<string>();
        string current = "";

        FormatFlags setter = new FormatFlags()
        {
            bold = false,
            italic = false,
            underline = false,
            color = null
        };

        foreach (FormatFlags flag in flagsList)
        {
            if (flag.c == ' ')
            {
                current += flag.c;
                continue;
            }

            if (flag.bold != setter.bold)
            {
                current += PrintTag("b", flag.bold);
                setter.bold = flag.bold;
            }

            if (flag.italic != setter.italic)
            {
                current += PrintTag("i", flag.italic);
                setter.italic = flag.italic;
            }

            if (flag.underline != setter.underline)
            {
                current += PrintTag("u", flag.underline);
                setter.underline = flag.underline;
            }

            if (flag.color != setter.color)
            {
                current += PrintTag("color", true, flag.color);
                setter.color = flag.color;
            }

            current += flag.c;
            output.Add(current
                + (setter.bold ? "</b>" : "")
                + (setter.italic ? "</i>" : "")
                + (setter.underline ? "</u>" : "")
            );
        }

        return output;
    }

    private string PrintTag(string tag, bool operation, string color = null)
    {
        if (tag != "color")
            return "<" + (operation ? "" : "/") + tag + ">";
        return "<color=\"" + color + "\">";
    }

    private List<char> GetDisplayChars(List<FormatFlags> flagsList)
    {
        List<char> output = new List<char>();
        foreach (FormatFlags flags in flagsList)
            if (flags.c != ' ')
                output.Add(flags.c);

        return output;
    }

    private IEnumerator ResponsiveSleep(float time)
    {
        for (float i = 0; i < time * speed; i += speed)
        {
            yield return new WaitForSeconds(time);
            if (skipDialogue)
                yield break;
        }
    }
}