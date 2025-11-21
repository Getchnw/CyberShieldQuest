using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character" ,menuName = "Game Content/Character")]
public class CharacterData : ScriptableObject
{
   public int character_id;
   public string characterName;
   public Sprite characterImage;
}
