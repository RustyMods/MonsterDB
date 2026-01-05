using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class BaseCreature : Base
{
    [YamlMember(Order = 9)] public CharacterDropRef? Drops;
    [YamlMember(Order = 10)] public TameableRef? Tameable;
    [YamlMember(Order = 11)] public ProcreationRef? Procreation;
    [YamlMember(Order = 12)] public GrowUpRef? GrowUp;
    [YamlMember(Order = 13)] public NPCTalkRef? NPCTalk;
    [YamlMember(Order = 14)] public MovementDamageRef? MovementDamage;
    [YamlMember(Order = 15)] public SaddleRef? Saddle;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Prefab = prefab.name;
        
        if (prefab.TryGetComponent(out CharacterDrop characterDrop))
        {
            Drops = characterDrop;
        }

        if (prefab.TryGetComponent(out Growup growUp))
        {
            GrowUp = growUp;
        }
        
        if (prefab.TryGetComponent(out Tameable tameable))
        {
            Tameable = tameable;
        }

        if (prefab.TryGetComponent(out Procreation procreation))
        {
            Procreation = procreation;
        }

        if (prefab.TryGetComponent(out NpcTalk npcTalk))
        {
            NPCTalk = npcTalk;
        }

        if (prefab.TryGetComponent(out MovementDamage md))
        {
            MovementDamage = md;
        }

        Sadle? saddle = prefab.GetComponentInChildren<Sadle>(true);
        if (saddle != null)
        {
            Saddle = saddle;
        }
        
        if (isClone)
        {
            IsCloned = true;
            ClonedFrom = source;
        }
    }
    
    protected void UpdateCharacterDrop(GameObject prefab)
    {
        CharacterDrop? drops = prefab.GetComponent<CharacterDrop>();
        if (drops == null && Drops != null)
        {
            drops = prefab.AddComponent<CharacterDrop>();
        }
        else if (drops != null && Drops == null)
        {
            prefab.Remove<CharacterDrop>();
            drops = null;
        }
        
        if (drops != null && Drops != null)
        {
            drops.SetFieldsFrom(Drops);
        }
    }

    protected void UpdateGrowUp(GameObject prefab)
    {
        Growup? growUp = prefab.GetComponent<Growup>();
        if (growUp == null && GrowUp != null && !string.IsNullOrEmpty(GrowUp.m_grownPrefab))
        {
            growUp = prefab.AddComponent<Growup>();
        }
        else if (growUp != null && (GrowUp == null || string.IsNullOrEmpty(GrowUp.m_grownPrefab)))
        {
            prefab.Remove<Growup>();
            growUp = null;
        }
        
        if (growUp != null && GrowUp != null)
        {
            growUp.SetFieldsFrom(GrowUp);
        }
    }

    protected Dictionary<string, GameObject> GetDefaultItems(Humanoid humanoid)
    {
        Dictionary<string, GameObject> attacks = new();
        if (humanoid.m_defaultItems != null)
        {
            foreach (GameObject? item in humanoid.m_defaultItems)
            {
                if (item == null) continue;
                attacks[item.name] = item;
            }
        }

        if (humanoid.m_randomWeapon != null)
        {
            foreach (GameObject? item in humanoid.m_randomWeapon)
            {
                if (item == null) continue;
                attacks[item.name] = item;
            }
        }
        
        if (humanoid.m_randomSets != null)
        {
            foreach (Humanoid.ItemSet? set in humanoid.m_randomSets)
            {
                foreach (GameObject? item in set.m_items)
                {
                    if (item == null) continue;
                    attacks[item.name] = item;
                }
            }
        }

        return attacks;
    }
    
    protected void UpdateTameable(GameObject prefab)
    {
        Tameable? tameable = prefab.GetComponent<Tameable>();
        if (tameable == null && Tameable != null)
        {
            tameable = prefab.AddComponent<Tameable>();
        }
        else if (tameable != null && Tameable == null)
        {
            prefab.Remove<Tameable>();
            tameable = null;
        }
        
        if (tameable != null && Tameable != null)
        {
            tameable.SetFieldsFrom(Tameable);
        }
    }

    protected void UpdateProcreation(GameObject prefab)
    {
        Procreation? procreation = prefab.GetComponent<Procreation>();
        if (procreation == null && Procreation != null && !string.IsNullOrEmpty(Procreation.m_offspring))
        {
            procreation = prefab.AddComponent<Procreation>();
        }
        else if (procreation != null && (Procreation == null || string.IsNullOrEmpty(Procreation.m_offspring)))
        {
            prefab.Remove<Procreation>();
            procreation = null;
        }
        
        if (procreation != null && Procreation != null)
        {
            procreation.SetFieldsFrom(Procreation);
        }
    }

    protected void UpdateNpcTalk(GameObject prefab)
    {
        NpcTalk? npcTalk = prefab.GetComponent<NpcTalk>();
        if (npcTalk == null && NPCTalk != null)
        {
            npcTalk = prefab.AddComponent<NpcTalk>();
        }
        else if (npcTalk != null && NPCTalk == null)
        {
            prefab.Remove<NpcTalk>();
            npcTalk = null;
        }

        if (npcTalk != null && NPCTalk != null)
        {
            npcTalk.SetFieldsFrom(NPCTalk);
        }
    }

    protected void UpdateSaddle(GameObject prefab)
    {
        Sadle? saddle = prefab.GetComponentInChildren<Sadle>(true);
        if (saddle == null || Saddle == null) return;
        saddle.SetFieldsFrom(Saddle);
    }
    
    protected void UpdateMovementDamage(GameObject prefab)
    {
        if (!prefab.TryGetComponent(out MovementDamage? md))
        {
            if (MovementDamage != null)
            {
                md = prefab.AddComponent<MovementDamage>();
                md.m_runDamageObject = new GameObject("RunDamageObject", typeof(Aoe));
            }
        }
        else
        {
            if (MovementDamage == null)
            {
                prefab.Remove<MovementDamage>();
                md = null;
            }
        }

        if (md != null && md.m_runDamageObject != null && md.m_runDamageObject.TryGetComponent(out Aoe aoe))
        {
            if (MovementDamage == null)
            {
                prefab.Remove<MovementDamage>();
            }
            else if (MovementDamage.m_areaOfEffect != null)
            {
                aoe.SetFieldsFrom(MovementDamage.m_areaOfEffect);
            }
        }
    }
}