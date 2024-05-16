using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu(menuName = "DialogueSO/Character")]
public class CharacterSO : ScriptableObject
{
    [System.Serializable]
    public struct Express
    {
        public Expression expName;
        public Sprite portrait;
    }

    public Character charName;
    public float speed;
    public float pitch;
    public TMP_FontAsset font;

    public AudioClip[] sfx;
    public Express[] expressions;

    public Sprite GetExpression(Expression name)
    {
        foreach (Express e in expressions)
            if (e.expName == name)
                return e.portrait;
        return null;
    }
}
