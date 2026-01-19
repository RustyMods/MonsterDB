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

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not Base originalBase) return;
        if (Visuals != null && originalBase.Visuals != null) Visuals.ResetTo(originalBase.Visuals);

        if (originalBase.Drops == null) Drops = null;
        if (originalBase.Tameable == null) Tameable = null;
        if (originalBase.Procreation == null) Procreation = null;
        if (originalBase.GrowUp == null)  GrowUp = null;
        if (originalBase.NPCTalk == null) NPCTalk = null;
        if (originalBase.Saddle == null) Saddle = null;
        
        if (Drops != null && originalBase.Drops != null) Drops.ResetTo(originalBase.Drops);
        if (Tameable != null  && originalBase.Tameable != null) Tameable.ResetTo(originalBase.Tameable);
        if (Procreation != null && originalBase.Procreation != null) Procreation.ResetTo(originalBase.Procreation);
        if (GrowUp != null &&  originalBase.GrowUp != null) GrowUp.ResetTo(originalBase.GrowUp);
        if (NPCTalk != null && originalBase.NPCTalk != null)  NPCTalk.ResetTo(originalBase.NPCTalk);
        if (MovementDamage != null && originalBase.MovementDamage != null)  MovementDamage.ResetTo(originalBase.MovementDamage);
        if (Saddle != null && originalBase.Saddle != null)  Saddle.ResetTo(originalBase.Saddle);
        if (DropProjectileOverDistance != null && originalBase.DropProjectileOverDistance != null) DropProjectileOverDistance.ResetTo(originalBase.DropProjectileOverDistance);
        if (CinderSpawner != null && originalBase.CinderSpawner != null)  CinderSpawner.ResetTo(originalBase.CinderSpawner);
        if (TimedDestruction != null && originalBase.TimedDestruction != null)  TimedDestruction.ResetTo(originalBase.TimedDestruction);
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
            string? prefabName = Utils.GetPrefabName(character.name);
            if (Prefab != prefabName) continue;
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
        UpdateLevelEffects(prefab, isInstance);
        UpdateVisual(prefab, isInstance);
        UpdateCharacterDrop(prefab, isInstance);
        UpdateGrowUp(prefab, isInstance);
        UpdateTameable(prefab, isInstance);
        UpdateProcreation(prefab, isInstance);
        UpdateNpcTalk(prefab, isInstance);
        UpdateMovementDamage(prefab, isInstance);
        UpdateSaddle(prefab, isInstance);
        UpdateDropProjectile(prefab, isInstance);
        UpdateCinderSpawner(prefab, isInstance);
        UpdateTimedDestruction(prefab, isInstance);
    }
    
    protected virtual void UpdateVisual(GameObject prefab, bool isInstance = false)
    {
        if (Visuals != null)
        {
            Visuals.Update(prefab, isInstance, false);
        }
    }

    protected void UpdateTimedDestruction(GameObject prefab, bool isInstance = false)
    {
        if (TimedDestruction == null || !prefab.TryGetComponent(out CharacterTimedDestruction ctd)) return;
        TimedDestruction.UpdateFields(ctd, prefab.name, !isInstance);
    }

    protected void UpdateCinderSpawner(GameObject prefab, bool isInstance = false)
    {
        if (CinderSpawner != null)
        {
            CinderSpawner.Update(prefab, isInstance);
        }
    }
    
    protected void UpdateLevelEffects(GameObject prefab, bool isInstance = false)
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
    
    protected void UpdateCharacterDrop(GameObject prefab, bool isInstance = false)
    {
        CharacterDrop? drops = prefab.GetComponent<CharacterDrop>();

        if (!isInstance)
        {
            if (drops == null && Drops != null)
            {
                drops = prefab.AddComponent<CharacterDrop>();
            }
            else if (drops != null && Drops == null)
            {
                prefab.Remove<CharacterDrop>();
                drops = null;
            }
        }
        
        if (drops != null && Drops != null)
        {
            Drops.UpdateFields(drops, prefab.name, !isInstance);
        }
    }

    protected void UpdateGrowUp(GameObject prefab, bool isInstance = false)
    {
        Growup? growUp = prefab.GetComponent<Growup>();

        if (!isInstance)
        {
            if (growUp == null && GrowUp != null && !string.IsNullOrEmpty(GrowUp.m_grownPrefab))
            {
                growUp = prefab.AddComponent<Growup>();
            }
            else if (growUp != null && (GrowUp == null || string.IsNullOrEmpty(GrowUp.m_grownPrefab)))
            {
                prefab.Remove<Growup>();
                growUp = null;
            }
        }        
        
        if (growUp != null && GrowUp != null)
        {
            GrowUp.UpdateFields(growUp, prefab.name, !isInstance);
        }
    }
    
    protected void UpdateTameable(GameObject prefab, bool isInstance = false)
    {
        Tameable? tameable = prefab.GetComponent<Tameable>();
        if (!isInstance)
        {
            if (tameable == null && Tameable != null)
            {
                tameable = prefab.AddComponent<Tameable>();
            }
            else if (tameable != null && Tameable == null)
            {
                prefab.Remove<Tameable>();
                tameable = null;
            }
        }
        
        if (tameable != null && Tameable != null)
        {
            Tameable.UpdateFields(tameable, prefab.name, !isInstance);
        }
    }

    protected void UpdateProcreation(GameObject prefab, bool isInstance = false)
    {
        Procreation? procreation = prefab.GetComponent<Procreation>();

        if (!isInstance)
        {
            if (procreation == null && Procreation != null && !string.IsNullOrEmpty(Procreation.m_offspring))
            {
                procreation = prefab.AddComponent<Procreation>();
            }
            else if (procreation != null && (Procreation == null || string.IsNullOrEmpty(Procreation.m_offspring)))
            {
                prefab.Remove<Procreation>();
                procreation = null;
            }
        }
        
        if (procreation != null && Procreation != null)
        {
            Procreation.UpdateFields(procreation, prefab.name, !isInstance);
        }
    }

    protected void UpdateNpcTalk(GameObject prefab, bool isInstance = false)
    {
        NpcTalk? npcTalk = prefab.GetComponent<NpcTalk>();

        if (!isInstance)
        {
            if (npcTalk == null && NPCTalk != null)
            {
                npcTalk = prefab.AddComponent<NpcTalk>();
            }
            else if (npcTalk != null && NPCTalk == null)
            {
                prefab.Remove<NpcTalk>();
                npcTalk = null;
            }
        }
        
        if (npcTalk != null && NPCTalk != null)
        {
            NPCTalk.UpdateFields(npcTalk, prefab.name, !isInstance);
        }
    }

    protected void UpdateSaddle(GameObject prefab, bool isInstance = false)
    {
        Sadle? saddle = prefab.GetComponentInChildren<Sadle>(true);
        Saddle? custom = prefab.GetComponent<Saddle>();

        if (!isInstance)
        {
            if (Saddle != null && saddle == null && custom == null)
            {
                custom = prefab.AddComponent<Saddle>();
            }
            else if (Saddle == null)
            {
                prefab.Remove<Saddle>();
                custom = null;
            }
        }

        if (custom != null && Saddle != null)
        {
            Saddle.UpdateFields(custom, prefab.name, !isInstance);
            if (custom.m_attachPoint != null && Saddle.m_attachOffset.HasValue)
            {
                custom.m_attachPoint.localPosition = Saddle.m_attachOffset.Value;
            }
        }
        
        if (saddle == null || Saddle == null) return;
        Saddle.UpdateFields(saddle, prefab.name, !isInstance);
    }
    
    protected void UpdateMovementDamage(GameObject prefab, bool isInstance = false)
    {
        if (MovementDamage == null || MovementDamage.m_areaOfEffect == null || !prefab.TryGetComponent(out MovementDamage movementDamage)) return;
        MovementDamage.m_areaOfEffect.UpdateFields(movementDamage, prefab.name, !isInstance);
    }

    protected void UpdateDropProjectile(GameObject prefab, bool isInstance = false)
    {
        if (DropProjectileOverDistance == null || 
            !prefab.TryGetComponent(out DropProjectileOverDistance dp)) return;
        DropProjectileOverDistance.UpdateFields(dp, prefab.name, !isInstance);
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