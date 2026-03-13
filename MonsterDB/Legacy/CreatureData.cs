using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDB.Solution.Methods;

namespace MonsterDB;

[Serializable]
public class CreatureData
{
    public VisualMethods.ScaleData m_scale = new();
    public VisualMethods.ScaleData m_ragdollScale = new();
    public Dictionary<string, VisualMethods.MaterialData> m_materials = new();
    public CharacterData m_characterData = new();
    public CharacterEffects m_effects = new();
    public List<ItemAttackData> m_defaultItems = [];
    public List<ItemAttackData> m_randomWeapons = [];
    public List<ItemAttackData> m_randomArmors = [];
    public List<ItemAttackData> m_randomShields = [];
    public List<RandomItemData> m_randomItems = [];
    public List<RandomItemSetsData> m_randomSets = [];
    public MonsterAIData m_monsterAIData = new();
    public AnimalAIData m_animalAIData = new();
    public List<CharacterDropData> m_characterDrops = [];
    public TameableData m_tameable = new();
    public ProcreationData m_procreation = new();
    public NPCTalkData m_npcTalk = new();
    public GrowUpData m_growUp = new();
    public List<LevelEffectData> m_levelEffects = [];
    public Dictionary<string, bool> m_particles = new();
    public VisualMethods.HumanData m_humanData = new();

    public List<ItemAttackData> GetAllItems()
    {
        List<ItemAttackData> list = [];
        list.AddRange(m_defaultItems);
        list.AddRange(m_randomWeapons);
        list.AddRange(m_randomArmors);
        list.AddRange(m_randomShields);
        list.Add(m_randomSets.SelectMany(x => x.m_items).ToArray());
        return list;
    }
}

[Serializable]
public class ItemAttackData
{
    public AttackData m_attackData = new();
    public ItemEffects m_effects = new();

    public void Set(ItemDataSharedRef r)
    {
        m_attackData.Set(ref r);
        m_effects.Set(ref r);
    }
}

[Serializable]
public class RandomItemSetsData
{
    public string m_name = "";
    public List<ItemAttackData> m_items = new();

    public HumanoidRef.ItemSet ToRef()
    {
        HumanoidRef.ItemSet r = new HumanoidRef.ItemSet
        {
            m_name = m_name,
            m_items = m_items.Select(x => x.m_attackData.Name).ToArray()
        };
        return r;
    }
}

[Serializable]
public class CharacterEffects
{
    public List<EffectInfo> m_hitEffects = [];
    public List<EffectInfo> m_critHitEffects = [];
    public List<EffectInfo> m_backstabHitEffects = [];
    public List<EffectInfo> m_deathEffects = [];
    public List<EffectInfo> m_waterEffects = [];
    public List<EffectInfo> m_tarEffects = [];
    public List<EffectInfo> m_slideEffects = [];
    public List<EffectInfo> m_jumpEffects = [];
    public List<EffectInfo> m_flyingContinuousEffects = [];
    public List<EffectInfo> m_pickupEffects = [];
    public List<EffectInfo> m_dropEffects = [];
    public List<EffectInfo> m_consumeItemEffects = [];
    public List<EffectInfo> m_equipEffects = [];
    public List<EffectInfo> m_perfectBlockEffects = [];
    public List<EffectInfo> m_alertedEffects = [];
    public List<EffectInfo> m_idleSounds = [];
    public List<EffectInfo> m_wakeupEffects = [];
    public List<EffectInfo> m_birthEffects = [];
    public List<EffectInfo> m_loveEffects = [];
    public List<EffectInfo> m_tamedEffects = [];
    public List<EffectInfo> m_soothEffects = [];
    public List<EffectInfo> m_petEffects = [];
    public List<EffectInfo> m_unSummonEffects = [];
    public List<EffectInfo> m_randomTalkFX = [];
    public List<EffectInfo> m_randomGreetFX = [];
    public List<EffectInfo> m_randomGoodbyeFX = [];

    public void Set(ref TameableRef? r)
    {
        if (r == null) return;
        r.m_tamedEffect = ToEffectListRef(m_tamedEffects);
        r.m_sootheEffect = ToEffectListRef(m_soothEffects);
        r.m_petEffect = ToEffectListRef(m_petEffects);
        r.m_unSummonEffect = ToEffectListRef(m_unSummonEffects);
    }

    public void Set(ref ProcreationRef? r)
    {
        if (r == null) return;
        r.m_birthEffects = ToEffectListRef(m_birthEffects);
        r.m_loveEffects = ToEffectListRef(m_loveEffects);
    }

    public void Set(ref NPCTalkRef? r)
    {
        if (r == null) return;
        r.m_randomGreetFX = ToEffectListRef(m_randomGreetFX);
        r.m_randomGoodbyeFX = ToEffectListRef(m_randomGoodbyeFX);
        r.m_randomTalkFX = ToEffectListRef(m_randomTalkFX);
    }

    public void Set(ref CharacterRef? reference)
    {
        if (reference == null)
        {
            reference = new CharacterRef();
        }

        reference.m_hitEffects = ToEffectListRef(m_hitEffects);
        reference.m_critHitEffects = ToEffectListRef(m_critHitEffects);
        reference.m_backstabHitEffects = ToEffectListRef(m_backstabHitEffects);
        reference.m_deathEffects = ToEffectListRef(m_deathEffects);
        reference.m_waterEffects = ToEffectListRef(m_waterEffects);
        reference.m_tarEffects = ToEffectListRef(m_tarEffects);
        reference.m_slideEffects = ToEffectListRef(m_slideEffects);
        reference.m_jumpEffects = ToEffectListRef(m_jumpEffects);
        reference.m_flyingContinuousEffect = ToEffectListRef(m_flyingContinuousEffects);
    }

    public void Set(ref MonsterAIRef? reference)
    {
        if (reference == null)
        {
            reference = new MonsterAIRef();
        }

        reference.m_wakeupEffects = ToEffectListRef(m_wakeupEffects);
    }

    public void Set(ref HumanoidRef? reference)
    {
        if (reference == null)
        {
            reference = new HumanoidRef();
        }

        reference.m_consumeItemEffects = ToEffectListRef(m_consumeItemEffects);
        reference.m_equipEffects = ToEffectListRef(m_equipEffects);
        reference.m_perfectBlockEffect = ToEffectListRef(m_perfectBlockEffects);
    }

    public EffectListRef? ToEffectListRef(List<EffectInfo> infos)
    {
        if (infos.Count == 0) return null;
        var data = new EffectListRef();
        data.m_effectPrefabs = infos.Select(x => x.ToRef()).ToList();
        return data;
    }
}

[Serializable]
public class ItemEffects
{
    public List<EffectInfo> m_hitEffects = [];
    public List<EffectInfo> m_hitTerrainEffects = [];
    public List<EffectInfo> m_blockEffects = [];
    public List<EffectInfo> m_startEffects = [];
    public List<EffectInfo> m_holdStartEffects = [];
    public List<EffectInfo> m_equipEffects = [];
    public List<EffectInfo> m_unEquipEffects = [];
    public List<EffectInfo> m_triggerEffects = [];
    public List<EffectInfo> m_trailStartEffects = [];

    public void Set(ref ItemDataSharedRef r)
    {
        r.m_hitEffect = ToEffectListRef(m_hitEffects);
        r.m_hitTerrainEffect = ToEffectListRef(m_hitTerrainEffects);
        r.m_blockEffect = ToEffectListRef(m_blockEffects);
        r.m_startEffect = ToEffectListRef(m_startEffects);
        r.m_holdStartEffect = ToEffectListRef(m_holdStartEffects);
        r.m_triggerEffect = ToEffectListRef(m_triggerEffects);
        r.m_trailStartEffect = ToEffectListRef(m_trailStartEffects);
    }
    
    public EffectListRef? ToEffectListRef(List<EffectInfo> infos)
    {
        if (infos.Count == 0) return null;
        EffectListRef data = new EffectListRef
        {
            m_effectPrefabs = infos.Select(x => x.ToRef()).ToList()
        };
        return data;
    }
}