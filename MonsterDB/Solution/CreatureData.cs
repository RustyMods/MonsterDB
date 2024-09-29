using System;
using System.Collections.Generic;
using MonsterDB.Solution.Methods;

namespace MonsterDB.Solution;

[Serializable]
public class CreatureData
{
    public VisualMethods.ScaleData m_scale = new();
    public VisualMethods.ScaleData m_ragdollScale = new();
    public Dictionary<string, VisualMethods.MaterialData> m_materials = new();
    public CharacterData m_characterData = new();
    public CharacterEffects m_effects = new();
    public List<ItemAttackData> m_defaultItems = new();
    public List<ItemAttackData> m_randomWeapons = new();
    public List<ItemAttackData> m_randomArmors = new();
    public List<ItemAttackData> m_randomShields = new();
    public List<RandomItemData> m_randomItems = new();
    public List<RandomItemSetsData> m_randomSets = new();
    public MonsterAIData m_monsterAIData = new();
    public AnimalAIData m_animalAIData = new();
    public List<CharacterDropData> m_characterDrops = new();
    public TameableData m_tameable = new();
    public ProcreationData m_procreation = new();
    public NPCTalkData m_npcTalk = new();
}

[Serializable]
public class ItemAttackData
{
    public AttackData m_attackData = new();
    public ItemEffects m_effects = new();
}

[Serializable]
public class RandomItemSetsData
{
    public string m_name = "";
    public List<ItemAttackData> m_items = new();
}

[Serializable]
public class CharacterEffects
{
    public List<EffectInfo> m_hitEffects = new();
    public List<EffectInfo> m_critHitEffects = new();
    public List<EffectInfo> m_backstabHitEffects = new();
    public List<EffectInfo> m_deathEffects = new();
    public List<EffectInfo> m_waterEffects = new();
    public List<EffectInfo> m_tarEffects = new();
    public List<EffectInfo> m_slideEffects = new();
    public List<EffectInfo> m_jumpEffects = new();
    public List<EffectInfo> m_flyingContinuousEffects = new();
    public List<EffectInfo> m_pickupEffects = new();
    public List<EffectInfo> m_dropEffects = new();
    public List<EffectInfo> m_consumeItemEffects = new();
    public List<EffectInfo> m_equipEffects = new();
    public List<EffectInfo> m_perfectBlockEffects = new();
    public List<EffectInfo> m_alertedEffects = new();
    public List<EffectInfo> m_idleSounds = new();
    public List<EffectInfo> m_wakeupEffects = new();
    public List<EffectInfo> m_birthEffects = new();
    public List<EffectInfo> m_loveEffects = new();
    public List<EffectInfo> m_tamedEffects = new();
    public List<EffectInfo> m_soothEffects = new();
    public List<EffectInfo> m_petEffects = new();
    public List<EffectInfo> m_unSummonEffects = new();
    public List<EffectInfo> m_randomTalkFX = new();
    public List<EffectInfo> m_randomGreetFX = new();
    public List<EffectInfo> m_randomGoodbyeFX = new();
}

[Serializable]
public class ItemEffects
{
    public List<EffectInfo> m_hitEffects = new();
    public List<EffectInfo> m_hitTerrainEffects = new();
    public List<EffectInfo> m_blockEffects = new();
    public List<EffectInfo> m_startEffects = new();
    public List<EffectInfo> m_holdStartEffects = new();
    public List<EffectInfo> m_equipEffects = new();
    public List<EffectInfo> m_unEquipEffects = new();
    public List<EffectInfo> m_triggerEffects = new();
    public List<EffectInfo> m_trailStartEffects = new();
}