using UnityEngine;

public static class CraftingSystem
{
    // ราคาในการสร้าง (Craft Cost)
    public static int GetCraftCost(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 100;
            case Rarity.Rare: return 150;
            case Rarity.Epic: return 400;
            case Rarity.Legendary: return 1000;
            default: return 100;
        }
    }

    // ราคาที่ได้จากการย่อย (Dismantle Value)
    public static int GetDismantleValue(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return 10;
            case Rarity.Rare: return 30;
            case Rarity.Epic: return 100;
            case Rarity.Legendary: return 250;
            default: return 5;
        }
    }
}