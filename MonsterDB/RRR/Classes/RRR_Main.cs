using System;

namespace MonsterDB;

[Serializable]
public class RRR_Main
{
    public string? sOriginalPrefabName;
    public string? sNewPrefabName;
    public RRR_Appearance? Category_Appearance ;
    public RRR_Character? Category_Character ;
    public RRR_Humanoid? Category_Humanoid ;
    public RRR_BaseAI? Category_BaseAI ;
    public RRR_MonsterAI? Category_MonsterAI ;
    public RRR_CharacterDrop? Category_CharacterDrops ;
    public RRR_Special? Category_Special ;
    public RRR_NPC? Category_NpcOnly ;
    public string? sConfigFormatVersion ;
}