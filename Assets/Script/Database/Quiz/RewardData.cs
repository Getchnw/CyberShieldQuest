using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Reward", menuName = "Game Content/Reward")]

[System.Serializable]
public class CardRewardItem
{
    public CardData card;
    [Min(1)] public int amount = 1;
}
public class RewardData : ScriptableObject
{
  [Header("Identifiers")]
  public int reward_id;
  public QuizData quiz;

  [Header("Requirements")]
  [Range(0, 3)] public int starRequired = 0;

  [Header("Reward")]
  public RewardType rewardType = RewardType.Gold;
  // For gold/values
  public int rewardValue = 0;
  public List<CardRewardItem> cardReference;
  public int experiencePoints = 0;
}

//public enum AnswersRequired
//{
//  underthree = 0,
//  Three = 3,
//  Four = 4,
//  Five = 5
//}

public enum RewardType
{
  Card,
  Gold,
  Other
}