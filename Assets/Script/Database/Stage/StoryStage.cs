using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "New StoryStage", menuName = "Game Content/StoryStage")]

public class StoryStage : ScriptableObject
{
    public string stageID;
    public string stageName;
    public string stageName_th;
    public StoryData story;
    public Sprite stageImage;
    public int botLevel;
    public string deckDescription;  // คำบรรยายเด็คบอท
    public string deckDescription_th;
    public List<StarCondition> starConditions;
    public List<int> requiredChapters;
    [Header("Battle Settings (ส่งไปฉากต่อสู้)")]
    public List<MainCategory> botDecks; // บอทจะใช้การ์ดหมวดไหนบ้าง
    public BotDeckPreset botDeckPreset;
    [Header("Type of Stage Settings")]
    public bool isStoryBattle;
}