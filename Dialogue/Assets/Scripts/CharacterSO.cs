using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(menuName = "DialogueSO/Character")]
public class CharacterSO : ScriptableObject
{
    [System.Serializable]
    public struct Expression
    {
        public string expName;
        public Sprite portrait;
    }

    public string charName;
    public float speed;
    public float pitch;
    public TMP_FontAsset font;

    public AudioClip[] sfx;
    public Expression[] expressions;

    public Sprite GetExpression(string name)
    {
        foreach (Expression e in expressions)
            if (e.expName == name)
                return e.portrait;
        return null;
    }
}
