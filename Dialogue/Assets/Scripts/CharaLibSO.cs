using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "DialogueSO/CharacterLibrary")]
public class CharaLibSO : ScriptableObject
{
    public CharacterSO[] characters;

    public CharacterSO GetCharacter(Character name)
    {
        return characters.FirstOrDefault(character => character.charName == name);
    }
}
