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
    [YamlMember(Order = 14)] 
    public MovementDamageRef? MovementDamage;
    [YamlMember(Order = 15)] public SaddleRef? Saddle;
    [YamlMember(Order = 16, Description = "If field removed, will remove component")] 
    public DropProjectileOverDistanceRef? DropProjectileOverDistance;
    [YamlMember(Order = 17)] public CinderSpawnerRef? CinderSpawner;
    [YamlMember(Order = 18)] public CharacterTimedDestructionRef? TimedDestruction;
    [YamlMember(Order = 19, Description = "If entries removed, will still be registered, set enabled to false to disable")] public SpawnDataRef[]? SpawnData;
    [YamlMember(Order = 100)] public MiscComponent? Extra;
    [YamlMember(Order = 101, Description = "Reference only, these are attack animation triggers")] public List<string>? AnimationTriggers;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        SetupSharedFields(prefab, isClone, source);
        if (isClone) SetupSpawnData();
    }

    protected void SetupAnimationTriggers(GameObject prefab)
    {
        if (!prefab.GetComponentInChildren<Animator>()) return;
        
        ZNetView.m_forceDisableInit = true;
        GameObject? instance = UnityEngine.Object.Instantiate(prefab);
        ZNetView.m_forceDisableInit = false;
        
        Animator? animator = instance.GetComponentInChildren<Animator>();
        AnimationTriggers = animator.parameters.Select(x => x.name).ToList();

        UnityEngine.Object.DestroyImmediate(instance);
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
        
        ParticleSystem[] particleSystems = prefab.GetComponentsInChildren<ParticleSystem>(true);
        if (particleSystems.Length > 0)
        {
            Visuals.m_particleSystems = particleSystems.ToRef();
        }

        if (prefab.TryGetComponent(out CinderSpawner cinderSpawner))
        {
            CinderSpawner = cinderSpawner;
        }

        if (prefab.TryGetComponent(out CharacterTimedDestruction ctd))
        {
            TimedDestruction = ctd;
        }
        
        SetupAnimationTriggers(prefab);
        
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

    protected void SetupSpawnData()
    {
        SpawnDataRef data = new SpawnDataRef
        {
            m_name = $"MDB {Prefab} Spawn Data",
            m_prefab = Prefab
        };
        SpawnData = new[] { data };
    }
    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;

        CreatureManager.TrySave(prefab, out _, IsCloned, ClonedFrom);
        
        UpdatePrefab(prefab);
        List<Character>? characters = Character.GetAllCharacters();
        for (int i = 0; i < characters.Count; ++i)
        {
            Character? character = characters[i];
            if (character == null || character.gameObject == null) continue;
            UpdatePrefab(character.gameObject, true);
        }

        if (SpawnData != null)
        {
            for (int i = 0; i < SpawnData.Length; ++i)
            {
                SpawnDataRef data = SpawnData[i];
                SpawnManager.Update(data);
            }
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
            Visuals.UpdateParticleSystems(prefab);
        }
    }
    protected void UpdateScale(GameObject prefab)
    {
        if (Visuals == null || !Visuals.m_scale.HasValue) return;
        Character? character = prefab.GetComponent<Character>();
        if (character != null && 
            character.m_deathEffects != null && 
            character.m_deathEffects.m_effectPrefabs != null)
        {
            foreach (EffectList.EffectData? effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab != null && effect.m_prefab.GetComponent<Ragdoll>())
                {
                    // Gotcha! ragdoll scale is not always the same as prefab, need to multiply modification instead of direct value set
                    if (effect.m_prefab.transform.localScale == prefab.transform.localScale)
                    {
                        effect.m_prefab.transform.localScale = Visuals.m_scale.Value;
                    }
                    else
                    {
                        effect.m_prefab.transform.localScale = Vector3.Scale(effect.m_prefab.transform.localScale, Visuals.m_scale.Value);
                    }
                    break;
                }
            }
        }
        
        prefab.transform.localScale = Visuals.m_scale.Value;
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
                .ToDict(f => f.name);
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
        Saddle? custom = null;
        
        if (Saddle != null && saddle == null)
        {
            custom = prefab.GetComponent<Saddle>();
            if (custom == null)
            {
                custom = prefab.AddComponent<Saddle>();
            }
        }
        else if (Saddle == null)
        {
            prefab.Remove<Saddle>();
            custom = null;
        }

        if (custom != null && Saddle != null)
        {
            custom.SetFieldsFrom(Saddle);
            if (custom.m_attachPoint != null && Saddle.m_attachOffset.HasValue)
            {
                custom.m_attachPoint.localPosition = Saddle.m_attachOffset.Value;
            }
        }
        
        if (saddle == null || Saddle == null) return;
        saddle.SetFieldsFrom(Saddle);
    }
    
    protected void UpdateMovementDamage(GameObject prefab)
    {
        if (MovementDamage == null || MovementDamage.m_areaOfEffect == null || !prefab.TryGetComponent(out MovementDamage movementDamage)) return;
        movementDamage.SetFieldsFrom(MovementDamage.m_areaOfEffect);
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