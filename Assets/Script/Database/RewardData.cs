using UnityEngine;

[CreateAssetMenu(fileName = "New Reward", menuName = "Game Content/Reward")]
public class RewardData : ScriptableObject
{
  [Header("Identifiers")]
  public int reward_id;
  public QuizData quiz; // reference to the quiz this reward belongs to (optional)

  [Header("Requirements")]
  [Range(0, 3)] public int starRequired = 0; // 0..3 stars
  public AnswersRequired answersRequired = AnswersRequired.Three; // how many answers required

  [Header("Reward")]
  public RewardType rewardType = RewardType.Gold;

  // For gold/values
  public int rewardValue = 0;

  // If rewardType == Card, assign the CardData here
  public CardData cardReference;

  // Optional string id for external references
  public string reference_id;
}

public enum AnswersRequired
{
  Three = 3,
  Four = 4,
  Five = 5
}

public enum RewardType
{
  Card,
  Gold,
  Other
}