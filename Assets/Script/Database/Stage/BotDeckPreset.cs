using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bot Deck Preset", menuName = "Game Content/Bot Deck Preset")]
public class BotDeckPreset : ScriptableObject
{
    public string presetId;
    public string displayName;

    [TextArea(2, 6)]
    public string description;

    [Header("Random Deck Mode")]
    public bool useRandomAllCardsPool = false;
    [Min(1)] public int randomDeckSize = 30;

    [Header("Manual Card IDs")]
    public List<string> cardIds = new List<string>();

    public bool HasCards()
    {
        if (useRandomAllCardsPool)
        {
            CardData[] allCards = LoadAllCards();
            return allCards != null && allCards.Length > 0;
        }

        return cardIds != null && cardIds.Any(cardId => !string.IsNullOrWhiteSpace(cardId));
    }

    public List<CardData> BuildDeckCards()
    {
        List<CardData> result = new List<CardData>();

        if (useRandomAllCardsPool)
        {
            CardData[] allCards = LoadAllCards();
            if (allCards == null || allCards.Length == 0)
                return result;

            int count = Mathf.Max(1, randomDeckSize);
            for (int i = 0; i < count; i++)
            {
                int index = Random.Range(0, allCards.Length);
                CardData randomCard = allCards[index];
                if (randomCard != null)
                {
                    result.Add(randomCard);
                }
            }

            return result;
        }

        if (cardIds == null)
            return result;

        foreach (string cardId in cardIds)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                continue;

            CardData card = ResolveCardById(cardId);
            if (card != null)
            {
                result.Add(card);
            }
        }

        return result;
    }

    public string GetSummary()
    {
        if (useRandomAllCardsPool)
        {
            return $"Random all cards pool x{Mathf.Max(1, randomDeckSize)}";
        }

        if (cardIds == null || cardIds.Count == 0)
            return string.Empty;

        List<string> parts = new List<string>();
        foreach (string cardId in cardIds)
        {
            if (string.IsNullOrWhiteSpace(cardId))
                continue;

            parts.Add(cardId);
        }

        return string.Join("\n", parts);
    }

    private CardData[] LoadAllCards()
    {
        CardData[] loadedCards = Resources.LoadAll<CardData>("GameContent/Cards");
        if (loadedCards == null || loadedCards.Length == 0)
            return new CardData[0];

        return loadedCards.Where(card => card != null && !string.IsNullOrWhiteSpace(card.card_id)).ToArray();
    }

    private CardData ResolveCardById(string cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
            return null;

        if (GameContentDatabase.Instance != null)
        {
            CardData card = GameContentDatabase.Instance.GetCardByID(cardId);
            if (card != null)
                return card;
        }

        CardData[] loadedCards = LoadAllCards();
        if (loadedCards == null || loadedCards.Length == 0)
            return null;

        foreach (CardData card in loadedCards)
        {
            if (card != null && card.card_id == cardId)
                return card;
        }

        return null;
    }
}