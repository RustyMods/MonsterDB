using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MonsterDB.Misc;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class Base : Header
{
    [YamlMember(Order = 8)] 
    public VisualRef? Visuals;
    [YamlMember(Order = 9, Description = "If field removed, will remove component")] 
    public CharacterDropRef? Drops;
    [YamlMember(Order = 10, Description = "If field removed, will remove component")] 
    public TameableRef? Tameable;
    [YamlMember(Order = 11, Description = "If field removed, will remove component")] 
    public ProcreationRef? Procreation;
    [YamlMember(Order = 12, Description = "If field removed, will remove component")] 
    public GrowUpRef? GrowUp;
    [YamlMember(Order = 13, Description = "If field removed, will remove component")] 
    public NPCTalkRef? NPCTalk;
    [YamlMember(Order = 14, Description = "If field removed, will remove component")] 
    public MovementDamageRef? MovementDamage;
    [YamlMember(Order = 15)] 
    public SaddleRef? Saddle;
    [YamlMember(Order = 16, Description = "If field removed, will remove component")] 
    public DropProjectileOverDistanceRef? DropProjectileOverDistance;
    [YamlMember(Order = 17)] 
    public CinderSpawnerRef? CinderSpawner;
    [YamlMember(Order = 18)] 
    public CharacterTimedDestructionRef? TimedDestruction;
    [YamlMember(Order = 100)] 
    public MiscComponent? Extra;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        SetupSharedFields(prefab, isClone, source);
    }

    protected void SetupSharedFields(GameObject prefab, bool isClone = false, string source = "")
    {
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

        if (prefab.TryGetComponent(out DropProjectileOverDistance dp))
        {
            DropProjectileOverDistance = dp;
        }
        
        Visuals = new VisualRef
        {
            m_scale = prefab.transform.localScale
        };
        
        LevelEffects levelEffects = prefab.GetComponentInChildren<LevelEffects>();
        if (levelEffects != null)
        {
            Visuals.m_levelSetups = levelEffects.m_levelSetups.ToRef();
        }
        
        Renderer[]? renderers = prefab.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length > 0)
        {
            Visuals.m_renderers = renderers.ToRef();
        }
        
        Light[] lights = prefab.GetComponentsInChildren<Light>(true);
        if (lights.Length > 0)
        {
            Visuals.m_lights = lights.ToRef();
        }

        if (prefab.TryGetComponent(out CinderSpawner cinderSpawner))
        {
            CinderSpawner = cinderSpawner;
        }

        if (prefab.TryGetComponent(out CharacterTimedDestruction ctd))
        {
            TimedDestruction = ctd;
        }
        
        if (isClone)
        {
            IsCloned = true;
            ClonedFrom = source;
        }
    }

    protected void SetupUnknowns(GameObject prefab)
    {
        MiscComponent misc = new MiscComponent();
        misc.Setup(prefab);
        if (misc.components != null)
        {
            Extra = misc;
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        CreatureManager.Save(prefab, IsCloned, ClonedFrom);
        UpdatePrefab(prefab);
        List<Character>? characters = Character.GetAllCharacters();
        for (int i = 0; i < characters.Count; ++i)
        {
            Character? character = characters[i];
            UpdatePrefab(character.gameObject, true);
        }
        base.Update();
    }

    protected virtual void UpdatePrefab(GameObject prefab, bool isInstance = false)
    {
        UpdateScale(prefab);
        UpdateLevelEffects(prefab);
        UpdateVisual(prefab);
        UpdateCharacterDrop(prefab);
        UpdateGrowUp(prefab);
        UpdateTameable(prefab);
        UpdateProcreation(prefab);
        UpdateNpcTalk(prefab);
        UpdateMovementDamage(prefab);
        UpdateSaddle(prefab);
        UpdateDropProjectile(prefab);
        UpdateCinderSpawner(prefab);
        UpdateTimedDestruction(prefab);
    }
    
    protected void UpdateVisual(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.UpdateRenderers(prefab);
            Visuals.UpdateLights(prefab);
        }
    }
    protected void UpdateScale(GameObject prefab)
    {
        if (Visuals == null || !Visuals.m_scale.HasValue) return;
        prefab.transform.localScale = Visuals.m_scale.Value;
        var character = prefab.GetComponent<Character>();
        if (character != null && 
            character.m_deathEffects != null && 
            character.m_deathEffects.m_effectPrefabs != null)
        {
            foreach (EffectList.EffectData? effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab != null && effect.m_prefab.GetComponent<Ragdoll>())
                {
                    effect.m_prefab.transform.localScale = Visuals.m_scale.Value;
                    break;
                }
            }
        }
    }

    protected void UpdateTimedDestruction(GameObject prefab)
    {
        if (TimedDestruction == null || !prefab.TryGetComponent(out CharacterTimedDestruction ctd)) return;
        ctd.SetFieldsFrom(TimedDestruction);
    }

    protected void UpdateCinderSpawner(GameObject prefab)
    {
        if (CinderSpawner != null)
        {
            CinderSpawner.Update(prefab);
        }
    }
    
    protected void UpdateLevelEffects(GameObject prefab)
    {
        LevelEffects? levelEffects = prefab.GetComponentInChildren<LevelEffects>();
        if (levelEffects == null) return;
        if (levelEffects != null && Visuals != null && Visuals.m_levelSetups != null)
        {
            Dictionary<string, Renderer> renderers = prefab
                .GetComponentsInChildren<Renderer>(true)
                .ToDictionary(f => f.name);
            List<LevelEffects.LevelSetup> setups = new();
            foreach (LevelSetupRef? levelRef in Visuals.m_levelSetups)
            {
                LevelEffects.LevelSetup levelSetup = new LevelEffects.LevelSetup();
                levelRef.Set(levelSetup, renderers);
                setups.Add(levelSetup);
            }
            levelEffects.m_levelSetups = setups;
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

        if (md != null && 
            md.m_runDamageObject != null && 
            md.m_runDamageObject.TryGetComponent(out Aoe aoe))
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

    protected void UpdateDropProjectile(GameObject prefab)
    {
        if (DropProjectileOverDistance == null || 
            !prefab.TryGetComponent(out DropProjectileOverDistance dp)) return;
        dp.SetFieldsFrom(DropProjectileOverDistance);
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
    
}