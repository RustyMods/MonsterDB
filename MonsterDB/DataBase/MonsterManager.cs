using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MonsterDB.Behaviors;
using UnityEngine;
using YamlDotNet.Serialization;
using Object = UnityEngine.Object;

namespace MonsterDB.DataBase;

public static class MonsterManager
{
    private static readonly Dictionary<string, MonsterData> m_defaultMonsterData = new();
    private static readonly List<string> m_newTames = new();

    private static readonly int Hue = Shader.PropertyToID("_Hue");
    private static readonly int Saturation = Shader.PropertyToID("_Saturation");
    private static readonly int Value = Shader.PropertyToID("_Value");
    private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
    private static readonly int Smoothness = Shader.PropertyToID("_Glossiness");
    private static readonly int UseGlossMap = Shader.PropertyToID("_UseGlossmap");
    private static readonly int Metallic = Shader.PropertyToID("_Metallic");
    private static readonly int MetalGloss = Shader.PropertyToID("_MetalGloss");
    private static readonly int MetalColor = Shader.PropertyToID("_MetalColor");
    private static readonly int EmissionMap = Shader.PropertyToID("_EmissionMap");
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int NormalStrength = Shader.PropertyToID("_BumpScale");
    private static readonly int BumpMap = Shader.PropertyToID("_BumpMap");
    private static readonly int TwoSidedNormals = Shader.PropertyToID("_TwoSidedNormals");
    private static readonly int UseStyles = Shader.PropertyToID("_UseStyles");
    private static readonly int Style = Shader.PropertyToID("_Style");
    private static readonly int StyleTex = Shader.PropertyToID("_StyleTex");
    private static readonly int AddRain = Shader.PropertyToID("_AddRain");
    private static readonly int MetallicGlossMap = Shader.PropertyToID("_MetallicGlossMap");

    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(ObjectDB), nameof(ObjectDB.Awake))]
    private static class Initiate_MonsterDB
    {
        private static void Postfix()
        {
            if (!ZNetScene.instance) return;
            MonsterDBPlugin.MonsterDBLogger.LogDebug("ObjectDB Awake: Initializing MONSTER DB Files");
            RegisterMonsterTweaks();
        }
    }

    private static void RegisterMonsterTweaks()
    {
        if (!ZNet.instance.IsServer()) return;
        Paths.CreateDirectories();
        
        ServerSync.UpdateServerTextures();
        
        List<MonsterData> deserialized = DeserializeMonsterFiles();

        RegisterClones(deserialized);

        RegisterMonsterTweaks(deserialized);

        ServerSync.UpdateServerMonsterDB();
    }

    private static void RegisterMonsterTweaks(List<MonsterData> list)
    {
        foreach (MonsterData data in list.Where(x => !x.isClone))
        {
            if (!GetMonsterData(data.PrefabName, out MonsterData monsterData))
            {
                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to get default data for " + data.PrefabName);
                continue;
            }

            if (UpdateMonsterData(data))
            {
                MonsterDBPlugin.MonsterDBLogger.LogDebug("Updated " + data.PrefabName);
            }

            ServerSync.m_serverData[data.PrefabName] = data;
        }
    }

    private static void RegisterClones(List<MonsterData> list)
    {
        foreach (var data in list.Where(x => x.isClone))
        {
            if (CloneMonster(data))
            {
                MonsterDBPlugin.MonsterDBLogger.LogDebug("Successfully created " + data.PrefabName);
                ServerSync.m_serverData[data.PrefabName] = data;
            }
        }
    }

    private static List<MonsterData> DeserializeMonsterFiles()
    {
        List<MonsterData> deserialized = new();
        Paths.CreateDirectories();
        string[] monsterFiles = Directory.GetFiles(Paths.MonsterPath);
        IDeserializer deserializer = new DeserializerBuilder().Build();
        
        foreach (var file in monsterFiles)
        {
            try
            {
                var serial = File.ReadAllText(file);
                var data = deserializer.Deserialize<MonsterData>(serial);
                deserialized.Add(data);
            }
            catch
            {
                MonsterDBPlugin.MonsterDBLogger.LogWarning("Failed to deserialize " + file);
            }
        }

        return deserialized;
    }

    public static void CreateBaseHuman()
    {
        GameObject player = ZNetScene.instance.GetPrefab("Player");
        if (!player) return;
        if (!player.TryGetComponent(out Player component)) return;
        GameObject human = UnityEngine.Object.Instantiate(player, MonsterDBPlugin._Root.transform, false);
        human.name = "Human01";
        UnityEngine.Object.Destroy(human.GetComponent<PlayerController>());
        UnityEngine.Object.Destroy(human.GetComponent<Player>());
        UnityEngine.Object.Destroy(human.GetComponent<Talker>());
        UnityEngine.Object.Destroy(human.GetComponent<Skills>());
        if (human.TryGetComponent(out ZNetView zNetView))
        {
            zNetView.m_persistent = true;
        }
        Humanoid humanoid = human.AddComponent<Humanoid>();
        humanoid.name = human.name;
        humanoid.m_name = "Human";
        humanoid.m_group = "Humans";
        humanoid.m_faction = Character.Faction.Players;
        humanoid.m_crouchSpeed = component.m_crouchSpeed;
        humanoid.m_walkSpeed = component.m_walkSpeed;
        humanoid.m_speed = component.m_speed;
        humanoid.m_runSpeed = component.m_runSpeed;
        humanoid.m_runTurnSpeed = component.m_runTurnSpeed;
        humanoid.m_acceleration = component.m_acceleration;
        humanoid.m_jumpForce = component.m_jumpForce;
        humanoid.m_jumpForceForward = component.m_jumpForceForward;
        humanoid.m_jumpForceTiredFactor = component.m_jumpForceForward;
        humanoid.m_airControl = component.m_airControl;
        humanoid.m_canSwim = true;
        humanoid.m_swimDepth = component.m_swimDepth;
        humanoid.m_swimSpeed = component.m_swimSpeed;
        humanoid.m_swimTurnSpeed = component.m_swimTurnSpeed;
        humanoid.m_swimAcceleration = component.m_swimAcceleration;
        humanoid.m_groundTilt = component.m_groundTilt;
        humanoid.m_groundTiltSpeed = component.m_groundTiltSpeed;
        humanoid.m_jumpStaminaUsage = component.m_jumpStaminaUsage;
        humanoid.m_eye = Utils.FindChild(human.transform, "EyePos");
        humanoid.m_hitEffects = component.m_hitEffects;
        humanoid.m_critHitEffects = component.m_critHitEffects;
        humanoid.m_backstabHitEffects = component.m_backstabHitEffects;

        GameObject newRagDoll = UnityEngine.Object.Instantiate(ZNetScene.instance.GetPrefab("Player_ragdoll"), MonsterDBPlugin._Root.transform, false);
        if (newRagDoll.TryGetComponent(out Ragdoll rag))
        {
            rag.m_ttl = 8f;
            rag.m_removeEffect = new()
            {
                m_effectPrefabs = new[]
                {
                    new EffectList.EffectData()
                    {
                        m_prefab = ZNetScene.instance.GetPrefab("vfx_corpse_destruction_small"),
                        m_enabled = true
                    }
                }
            };
            rag.m_float = true;
            rag.m_dropItems = true;
        }
        newRagDoll.name = "human_ragdoll";
        newRagDoll.AddComponent<RandomHuman>().m_isRagDoll = true;
        RegisterToZNetScene(newRagDoll);
        humanoid.m_deathEffects = new()
        {
            m_effectPrefabs = new[]
            {
                new EffectList.EffectData()
                {
                    m_prefab = ZNetScene.instance.GetPrefab("vfx_player_death"),
                    m_enabled = true,
                },
                new EffectList.EffectData()
                {
                    m_prefab = newRagDoll,
                    m_enabled = true
                }
            }
        };
        humanoid.m_waterEffects = component.m_waterEffects;
        humanoid.m_tarEffects = component.m_tarEffects;
        humanoid.m_slideEffects = component.m_slideEffects;
        humanoid.m_jumpEffects = component.m_jumpEffects;
        humanoid.m_flyingContinuousEffect = component.m_flyingContinuousEffect;
        humanoid.m_tolerateWater = true;
        humanoid.m_health = 100f;
        humanoid.m_damageModifiers = component.m_damageModifiers;
        humanoid.m_staggerWhenBlocked = true;
        humanoid.m_staggerDamageFactor = component.m_staggerDamageFactor;
        humanoid.m_defaultItems = component.m_defaultItems;
        humanoid.m_randomWeapon = new GameObject[] { };
        humanoid.m_randomArmor = new GameObject[] { };
        humanoid.m_randomShield = new GameObject[] { };
        humanoid.m_randomSets = new Humanoid.ItemSet[]
        {
            new Humanoid.ItemSet()
            {
                m_name = "Leather",
                m_items = new []
                {
                    ZNetScene.instance.GetPrefab("ArmorLeatherChest"),
                    ZNetScene.instance.GetPrefab("ArmorLeatherLegs"),
                    ZNetScene.instance.GetPrefab("Torch")
                }
            }, new Humanoid.ItemSet()
            {
                m_name = "Bronze",
                m_items = new []
                {
                    ZNetScene.instance.GetPrefab("ArmorBronzeChest"),
                    ZNetScene.instance.GetPrefab("ArmorBronzeLegs"),
                    ZNetScene.instance.GetPrefab("ShieldBronzeBuckler"),
                    ZNetScene.instance.GetPrefab("KnifeCopper")
                }
            }
        };
        humanoid.m_unarmedWeapon = component.m_unarmedWeapon;
        humanoid.m_pickupEffects = component.m_autopickupEffects;
        humanoid.m_dropEffects = component.m_dropEffects;
        humanoid.m_consumeItemEffects = component.m_consumeItemEffects;
        humanoid.m_equipEffects = component.m_equipStartEffects;
        humanoid.m_perfectBlockEffect = component.m_perfectBlockEffect;
        MonsterAI monsterAI = human.AddComponent<MonsterAI>();
        monsterAI.m_viewRange = 30f;
        monsterAI.m_viewAngle = 90f;
        monsterAI.m_hearRange = 9999f;
        monsterAI.m_idleSoundInterval = 10f;
        monsterAI.m_idleSoundChance = 0f;
        monsterAI.m_pathAgentType = Pathfinding.AgentType.Humanoid;
        monsterAI.m_moveMinAngle = 90f;
        monsterAI.m_smoothMovement = true;
        monsterAI.m_jumpInterval = 10f;
        monsterAI.m_randomCircleInterval = 2f;
        monsterAI.m_randomMoveInterval = 30f;
        monsterAI.m_randomMoveRange = 3f;
        monsterAI.m_alertRange = 20f;
        monsterAI.m_circulateWhileCharging = true;
        monsterAI.m_privateAreaTriggerTreshold = 4;
        monsterAI.m_interceptTimeMax = 2f;
        monsterAI.m_maxChaseDistance = 300f;
        monsterAI.m_circleTargetInterval = 8f;
        monsterAI.m_circleTargetDuration = 6f;
        monsterAI.m_circleTargetDistance = 8f;
        monsterAI.m_consumeRange = 2f;
        monsterAI.m_consumeSearchRange = 5f;
        monsterAI.m_consumeSearchInterval = 10f;
        monsterAI.m_consumeItems = new();
        CharacterDrop characterDrop = human.AddComponent<CharacterDrop>();
        characterDrop.m_drops = new List<CharacterDrop.Drop>()
        {
            new CharacterDrop.Drop()
            {
                m_prefab = ZNetScene.instance.GetPrefab("Coins"),
                m_amountMin = 1,
                m_amountMax = 10,
                m_chance = 0.25f,
                m_levelMultiplier = true,
            }
        };
        RandomAnimation randomAnimation = human.AddComponent<RandomAnimation>();
        randomAnimation.m_values = new()
        {
            new RandomAnimation.RandomValue()
            {
                m_name = "idle",
                m_value = 5,
                m_interval = 3,
                m_floatTransition = 1f,
            }
        };
        GameObject boar = ZNetScene.instance.GetPrefab("Boar");
        if (boar.TryGetComponent(out Tameable loxTame))
        {
            Tameable tameable = human.AddComponent<Tameable>();
            tameable.m_fedDuration = 600f;
            tameable.m_tamingTime = 1800f;
            tameable.m_commandable = true;
            tameable.m_tamedEffect = loxTame.m_tamedEffect;
            tameable.m_sootheEffect = loxTame.m_sootheEffect;
            tameable.m_petEffect = loxTame.m_petEffect;
            tameable.m_commandable = true;
            tameable.m_unsummonDistance = 0f;
            tameable.m_randomStartingName = new()
                { "Bjorn", "Harald", "Bo", "Frode", "Birger", "Arne", "Erik", "Kare", "Loki", "Thor", "Odin" };
        }

        human.AddComponent<RandomHuman>();
        if (!RegisterToZNetScene(human)) return;
    }

    private static GameObject? CreateClone(string originalMonster, string name)
    {
        GameObject critter = ZNetScene.instance.GetPrefab(originalMonster);
        if (!critter)
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find " + originalMonster);
            return null;
        }
        GameObject clone = UnityEngine.Object.Instantiate(critter, MonsterDBPlugin._Root.transform, false);
        clone.name = name;

        if (clone.TryGetComponent(out Humanoid humanoid))
        {
            CloneRagDoll(humanoid.m_deathEffects.m_effectPrefabs, clone);
        }
        else if (clone.TryGetComponent(out Character character))
        {
            CloneRagDoll(character.m_deathEffects.m_effectPrefabs, clone);
        }
        
        return clone;
    }

    private static void CloneRagDoll(EffectList.EffectData[] deathEffects, GameObject clone)
    {
        foreach (EffectList.EffectData effect in deathEffects)
        {
            if (!effect.m_prefab.GetComponent<Ragdoll>()) continue;
            GameObject clonedRagdoll = Object.Instantiate(effect.m_prefab, MonsterDBPlugin._Root.transform, false);
            clonedRagdoll.name = clone.name + "_ragdoll";
            RegisterToZNetScene(clonedRagdoll);
            effect.m_prefab = clonedRagdoll;
        }
    }

    public static bool Server_CloneMonster(MonsterData data) => CloneMonster(data);
    private static bool CloneMonster(MonsterData data)
    {
        if (!ZNetScene.instance) return false;
        GameObject? match = ZNetScene.instance.GetPrefab(data.PrefabName);
        if (!match)
        {
            GameObject? clone = CreateClone(data.OriginalMonster, data.PrefabName);
            if (clone == null) return false;
            if (!RegisterToZNetScene(clone))
            {
                MonsterDBPlugin.MonsterDBLogger.LogDebug($"Failed to register {clone.name} to ZNetScene");
                return false;
            }
        }
        
        UpdateMonsterData(data);
        return true;
    }
    public static bool Command_CloneMonster(string creatureName, string cloneName, bool writeToDisk = true)
    {
        if (!ZNetScene.instance) return false;

        bool exists = true;
        GameObject? clone = ZNetScene.instance.GetPrefab(cloneName);
        if (!clone)
        {
            clone = CreateClone(creatureName, cloneName);
            exists = false;
        }

        if (clone == null) return false;
        if (!exists)
        {
            if (!RegisterToZNetScene(clone))
            {
                MonsterDBPlugin.MonsterDBLogger.LogDebug($"Failed to register {clone.name} to ZNetScene");
                return false;
            }
        }
        if (!GetMonsterData(cloneName, out MonsterData data)) return false;
        data.OriginalMonster = creatureName;
        data.PrefabName = cloneName;
        data.DisplayName = cloneName;
        data.isClone = true;
        data.ENABLED = true;
        if (writeToDisk) WriteDataToDisk(cloneName, data);
        Command_UpdateMonster(cloneName);
        if (ZNet.instance.IsServer()) ServerSync.m_serverData[cloneName] = data;
        ServerSync.UpdateServerMonsterDB();
        return true;
    }

    private static bool RegisterToZNetScene(GameObject prefab)
    {
        if (!ZNetScene.instance) return false;
        if (ZNetScene.instance.m_prefabs.Contains(prefab)) return false;
        if (ZNetScene.instance.m_namedPrefabs.ContainsKey(prefab.name.GetStableHashCode())) return false;
        ZNetScene.instance.m_prefabs.Add(prefab);
        ZNetScene.instance.m_namedPrefabs[prefab.name.GetStableHashCode()] = prefab;
        return true;
    }

    public static bool Command_UpdateMonster(string creatureName)
    {
        string filePath = Paths.MonsterPath + Path.DirectorySeparatorChar + creatureName + ".yml";
        if (!File.Exists(filePath))
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find file: " + creatureName + ".yml");
            return false;
        }

        try
        {
            IDeserializer deserializer = new DeserializerBuilder().Build();
            string serial = File.ReadAllText(filePath);
            MonsterData data = deserializer.Deserialize<MonsterData>(serial);

            return UpdateMonsterData(data);
        }
        catch
        {
            MonsterDBPlugin.MonsterDBLogger.LogWarning("Failed to deserialize " + creatureName + ".yml");
            return false;
        }
    }

    public static bool Server_UpdateMonster(MonsterData data) => UpdateMonsterData(data);

    public static bool FileWatch_UpdateMonster(string filePath, string fileName)
    {
        if (!ZNet.instance.IsServer())
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Client: Must be server to update monster");
            return false;
        }
        if (!File.Exists(filePath))
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find file: " + fileName);
            return false;
        }

        try
        {
            var deserializer = new DeserializerBuilder().Build();
            var serial = File.ReadAllText(filePath);
            var data = deserializer.Deserialize<MonsterData>(serial);

            if (UpdateMonsterData(data))
            {
                ServerSync.m_serverData[data.PrefabName] = data;
                return true;
            }

            return false;
        }
        catch
        {
            MonsterDBPlugin.MonsterDBLogger.LogWarning("Failed to deserialize " + fileName);
            return false;
        }
    }

    public static bool ResetMonster(string creatureName)
    {
        return m_defaultMonsterData.TryGetValue(creatureName, out MonsterData data) && UpdateMonsterData(data, true);
    }

    private static void UpdateMonsterScale(MonsterData data, GameObject critter, bool reset)
    {
        if (!data.CHANGE_SCALE && !reset) return;
        critter.transform.localScale =
            new Vector3(data.Scale.x, data.Scale.y, data.Scale.z);
            
        if (critter.TryGetComponent(out CapsuleCollider collider))
        {
            collider.height = data.ColliderHeight;
            collider.center = new Vector3(data.ColliderCenter.x, data.ColliderCenter.y, data.ColliderCenter.z);
        }

        if (critter.TryGetComponent(out Humanoid humanoid))
        {
            foreach (var effect in humanoid.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab.GetComponent<Ragdoll>())
                {
                    effect.m_prefab.transform.localScale = new Vector3(data.Scale.x, data.Scale.y, data.Scale.z);
                }
            }
        }
        else if (critter.TryGetComponent(out Character character))
        {
            foreach (var effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab.GetComponent<Ragdoll>())
                {
                    effect.m_prefab.transform.localScale = new Vector3(data.Scale.x, data.Scale.y, data.Scale.z);
                }
            }
        }
    }

    private static void UpdateMonsterHumanoid(MonsterData data, Humanoid humanoid, bool reset)
    {
        humanoid.m_name = data.DisplayName;
        humanoid.m_group = data.Group;
        if (GetFaction(data.Faction, out Character.Faction faction)) humanoid.m_faction = faction;
        else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update faction for " + data.PrefabName);
        humanoid.m_boss = data.Boss;
        humanoid.m_dontHideBossHud = data.DoNotHideBossHUD;
        humanoid.m_bossEvent = data.BossEvent;
        humanoid.m_defeatSetGlobalKey = data.DefeatSetGlobalKey;

        if (data.CHANGE_MOVEMENT || reset)
        {
            humanoid.m_crouchSpeed = data.CrouchSpeed;
            humanoid.m_walkSpeed = data.WalkSpeed;
            humanoid.m_speed = data.Speed;
            humanoid.m_turnSpeed = data.TurnSpeed;
            humanoid.m_runSpeed = data.RunSpeed;
            humanoid.m_runTurnSpeed = data.RunTurnSpeed;
            humanoid.m_flySlowSpeed = data.FlySlowSpeed;
            humanoid.m_flyFastSpeed = data.FlyFastSpeed;
            humanoid.m_flyTurnSpeed = data.FlyTurnSpeed;
            humanoid.m_acceleration = data.Acceleration;
            humanoid.m_jumpForce = data.JumpForce;
            humanoid.m_jumpForceForward = data.JumpForceForward;
            humanoid.m_jumpForceTiredFactor = data.JumpForceTiredFactor;
            humanoid.m_airControl = data.AirControl;
            humanoid.m_canSwim = data.CanSwim;
            humanoid.m_swimDepth = data.SwimDepth;
            humanoid.m_swimSpeed = data.SwimSpeed;
            humanoid.m_swimTurnSpeed = data.SwimTurnSpeed;
            humanoid.m_swimAcceleration = data.SwimAcceleration;
            if (GetGroundTilt(data.GroundTilt, out Character.GroundTiltType type)) humanoid.m_groundTilt = type;
            else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update ground tilt type for " + data.PrefabName);
            humanoid.m_groundTiltSpeed = data.GroundTiltSpeed;
            humanoid.m_flying = data.Flying;
            humanoid.m_jumpStaminaUsage = data.JumpStaminaUsage;
            humanoid.m_disableWhileSleeping = data.DisableWhileSleeping;
        }
        // Effects
        if (data.CHANGE_CHARACTER_EFFECTS || reset)
        {
            if (GetEffects(data.HitEffects, out EffectList HitEffects)) humanoid.m_hitEffects = HitEffects;
            if (GetEffects(data.CriticalHitEffects, out EffectList CriticalHitEffects)) humanoid.m_critHitEffects = CriticalHitEffects;
            if (GetEffects(data.BackStabHitEffects, out EffectList BackStabHitEffects)) humanoid.m_backstabHitEffects = BackStabHitEffects;
            if (GetEffects(data.DeathEffects, out EffectList DeathEffects)) humanoid.m_deathEffects = DeathEffects;
            if (GetEffects(data.WaterEffects, out EffectList WaterEffects)) humanoid.m_waterEffects = WaterEffects;
            if (GetEffects(data.TarEffects, out EffectList TarEffects)) humanoid.m_tarEffects = TarEffects;
            if (GetEffects(data.SlideEffects, out EffectList SlideEffects)) humanoid.m_slideEffects = SlideEffects;
            if (GetEffects(data.JumpEffects, out EffectList JumpEffects)) humanoid.m_jumpEffects = JumpEffects;
            if (GetEffects(data.FlyingContinuousEffects, out EffectList FlyingContinuousEffects)) humanoid.m_flyingContinuousEffect = FlyingContinuousEffects;
            if (GetEffects(data.PickUpEffects, out EffectList PickUpEffects)) humanoid.m_pickupEffects = PickUpEffects;
            if (GetEffects(data.DropEffects, out EffectList DropEffects)) humanoid.m_pickupEffects = DropEffects;
            if (GetEffects(data.ConsumeItemEffects, out EffectList ConsumeItemEffects)) humanoid.m_pickupEffects = ConsumeItemEffects;
            if (GetEffects(data.EquipEffects, out EffectList EquipEffects)) humanoid.m_pickupEffects = EquipEffects;
            if (GetEffects(data.PerfectBlockEffect, out EffectList PerfectBlockEffect)) humanoid.m_pickupEffects = PerfectBlockEffect;
        }
        // Health & Damage
        if (data.CHANGE_HEALTH_DAMAGES || reset)
        {
            humanoid.m_tolerateWater = data.TolerateWater;
            humanoid.m_tolerateFire = data.TolerateFire;
            humanoid.m_tolerateSmoke = data.TolerateSmoke;
            humanoid.m_tolerateTar = data.TolerateTar;
            humanoid.m_health = data.Health;
            if (GetDamageMods(data.DamageModifiers, out HitData.DamageModifiers damageModifiers)) humanoid.m_damageModifiers = damageModifiers;
            humanoid.m_staggerWhenBlocked = data.StaggerWhenBlocked;
            humanoid.m_staggerDamageFactor = data.StaggerDamageFactor;
        }

        if (data.CHANGE_ATTACKS || reset)
        {
            if (GetItems(data.DefaultItems, reset, out List<GameObject> DefaultItems)) humanoid.m_defaultItems = DefaultItems.ToArray();
            if (GetItems(data.RandomWeapons, reset, out List<GameObject> RandomWeapons)) humanoid.m_randomWeapon = RandomWeapons.ToArray();
            if (GetItems(data.RandomArmors, reset, out List<GameObject> RandomArmors)) humanoid.m_randomArmor = RandomArmors.ToArray();
            if (GetItems(data.RandomShields, reset, out List<GameObject> RandomShields)) humanoid.m_randomShield = RandomShields.ToArray();
            if (GetRandomSets(data.RandomSets, reset, out List<Humanoid.ItemSet> RandomSets)) humanoid.m_randomSets = RandomSets.ToArray();
            if (MonsterDB.TryGetItemDrop(data.UnarmedWeapon, out ItemDrop? UnarmedWeapon)) humanoid.m_unarmedWeapon = UnarmedWeapon;
            humanoid.m_beardItem = data.BeardItem;
            humanoid.m_hairItem = data.HairItem;
        }
    }

    private static void UpdateMonsterCharacter(MonsterData data, Character character, bool reset)
    {
        character.m_name = data.DisplayName;
        character.m_group = data.Group;
        if (GetFaction(data.Faction, out Character.Faction faction)) character.m_faction = faction;
        else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update faction for " + data.PrefabName);
        character.m_boss = data.Boss;
        character.m_dontHideBossHud = data.DoNotHideBossHUD;
        character.m_bossEvent = data.BossEvent;
        character.m_defeatSetGlobalKey = data.DefeatSetGlobalKey;

        if (data.CHANGE_MOVEMENT || reset)
        {
            character.m_crouchSpeed = data.CrouchSpeed;
            character.m_walkSpeed = data.WalkSpeed;
            character.m_speed = data.Speed;
            character.m_turnSpeed = data.TurnSpeed;
            character.m_runSpeed = data.RunSpeed;
            character.m_runTurnSpeed = data.RunTurnSpeed;
            character.m_flySlowSpeed = data.FlySlowSpeed;
            character.m_flyFastSpeed = data.FlyFastSpeed;
            character.m_flyTurnSpeed = data.FlyTurnSpeed;
            character.m_acceleration = data.Acceleration;
            character.m_jumpForce = data.JumpForce;
            character.m_jumpForceForward = data.JumpForceForward;
            character.m_jumpForceTiredFactor = data.JumpForceTiredFactor;
            character.m_airControl = data.AirControl;
            character.m_canSwim = data.CanSwim;
            character.m_swimDepth = data.SwimDepth;
            character.m_swimSpeed = data.SwimSpeed;
            character.m_swimTurnSpeed = data.SwimTurnSpeed;
            character.m_swimAcceleration = data.SwimAcceleration;
            if (GetGroundTilt(data.GroundTilt, out Character.GroundTiltType type)) character.m_groundTilt = type;
            character.m_groundTiltSpeed = data.GroundTiltSpeed;
            character.m_flying = data.Flying;
            character.m_jumpStaminaUsage = data.JumpStaminaUsage;
            character.m_disableWhileSleeping = data.DisableWhileSleeping;
        }
        // Effects
        if (data.CHANGE_CHARACTER_EFFECTS || reset)
        {
            if (GetEffects(data.HitEffects, out EffectList HitEffects)) character.m_hitEffects = HitEffects;
            if (GetEffects(data.CriticalHitEffects, out EffectList CriticalHitEffects)) character.m_hitEffects = CriticalHitEffects;
            if (GetEffects(data.BackStabHitEffects, out EffectList BackStabHitEffects)) character.m_hitEffects = BackStabHitEffects;
            if (GetEffects(data.DeathEffects, out EffectList DeathEffects)) character.m_hitEffects = DeathEffects;
            if (GetEffects(data.WaterEffects, out EffectList WaterEffects)) character.m_hitEffects = WaterEffects;
            if (GetEffects(data.TarEffects, out EffectList TarEffects)) character.m_hitEffects = TarEffects;
            if (GetEffects(data.SlideEffects, out EffectList SlideEffects)) character.m_hitEffects = SlideEffects;
            if (GetEffects(data.JumpEffects, out EffectList JumpEffects)) character.m_hitEffects = JumpEffects;
            if (GetEffects(data.FlyingContinuousEffects, out EffectList FlyingContinuousEffects)) character.m_hitEffects = FlyingContinuousEffects;
        }
        // Health & Damage
        if (data.CHANGE_HEALTH_DAMAGES || reset)
        {
            character.m_tolerateWater = data.TolerateWater;
            character.m_tolerateFire = data.TolerateFire;
            character.m_tolerateSmoke = data.TolerateSmoke;
            character.m_tolerateTar = data.TolerateTar;
            character.m_health = data.Health;
            if (GetDamageMods(data.DamageModifiers, out HitData.DamageModifiers damageModifiers)) character.m_damageModifiers = damageModifiers;
            character.m_staggerWhenBlocked = data.StaggerWhenBlocked;
            character.m_staggerDamageFactor = data.StaggerDamageFactor;
        }
    }

    private static void UpdateMonsterAI(MonsterData data, MonsterAI monsterAI, bool reset)
    {
        monsterAI.m_viewRange = data.ViewRange;
        monsterAI.m_viewAngle = data.ViewAngle;
        monsterAI.m_hearRange = data.HearRange;
        monsterAI.m_mistVision = data.MistVision;
        if (data.CHANGE_MONSTER_EFFECTS || reset)
        {
            if (GetEffects(data.AlertedEffects, out EffectList AlertedEffects))
                monsterAI.m_alertedEffects = AlertedEffects;
            if (GetEffects(data.IdleSound, out EffectList IdleSounds))
                monsterAI.m_idleSound = IdleSounds;
            if (GetEffects(data.WakeUpEffects, out EffectList WakeUpEffects))
                monsterAI.m_wakeupEffects = WakeUpEffects;
        }
        monsterAI.m_idleSoundInterval = data.IdleSoundInterval;
        monsterAI.m_idleSoundChance = data.IdleSoundChance;
        if (GetPathType(data.PathAgentType, out Pathfinding.AgentType PathAgentType))
            monsterAI.m_pathAgentType = PathAgentType;
        monsterAI.m_moveMinAngle = data.MoveMinimumAngle;
        monsterAI.m_smoothMovement = data.smoothMovement;
        monsterAI.m_serpentMovement = data.SerpentMovement;
        monsterAI.m_serpentTurnRadius = data.SerpentTurnRadius;
        monsterAI.m_jumpInterval = data.JumpInterval;
        // Random Circle
        monsterAI.m_randomCircleInterval = data.RandomCircleInterval;
        // Random Movement
        monsterAI.m_randomMoveInterval = data.RandomMoveInterval;
        monsterAI.m_randomMoveRange = data.RandomMoveRange;
        // Fly Behavior
        monsterAI.m_randomFly = data.RandomFly;
        monsterAI.m_chanceToTakeoff = data.ChanceToTakeOff;
        monsterAI.m_chanceToLand = data.ChanceToLand;
        monsterAI.m_groundDuration = data.GroundDuration;
        monsterAI.m_airDuration = data.AirDuration;
        monsterAI.m_maxLandAltitude = data.MaxLandAltitude;
        monsterAI.m_takeoffTime = data.TakeOffTime;
        monsterAI.m_flyAltitudeMin = data.FlyAltitudeMinimum;
        monsterAI.m_flyAltitudeMax = data.FlyAltitudeMaximum;
        monsterAI.m_flyAbsMinAltitude = data.FlyAbsoluteMinimumAltitude;
        // Other
        monsterAI.m_avoidFire = data.AvoidFire;
        monsterAI.m_afraidOfFire = data.AfraidOfFire;
        monsterAI.m_avoidWater = data.AvoidWater;
        monsterAI.m_aggravatable = data.Aggravatable;
        monsterAI.m_passiveAggresive = data.PassiveAggressive;
        monsterAI.m_spawnMessage = data.SpawnMessage;
        monsterAI.m_deathMessage = data.DeathMessage;
        monsterAI.m_alertedMessage = data.AlertedMessage;
        // Monster AI
        monsterAI.m_alertRange = data.AlertRange;
        monsterAI.m_fleeIfHurtWhenTargetCantBeReached = data.FleeIfHurtWhenTargetCannotBeReached;
        monsterAI.m_fleeIfNotAlerted = data.FleeIfNotAlerted;
        monsterAI.m_fleeIfLowHealth = data.FleeIfLowHealth;
        monsterAI.m_circulateWhileCharging = data.CirculateWhileCharging;
        monsterAI.m_circulateWhileChargingFlying = data.CirculateWhileChargingFlying;
        monsterAI.m_enableHuntPlayer = data.EnableHuntPlayer;
        monsterAI.m_attackPlayerObjects = data.AttackPlayerObjects;
        monsterAI.m_privateAreaTriggerTreshold = data.PrivateAreaTriggerThreshold;
        monsterAI.m_interceptTimeMax = data.InterceptTimeMaximum;
        monsterAI.m_interceptTimeMin = data.InterceptTimeMinimum;
        monsterAI.m_maxChaseDistance = data.MaximumChaseDistance;
        monsterAI.m_minAttackInterval = data.MinimumAttackInterval;
        // Circle Target
        monsterAI.m_circleTargetInterval = data.CircleTargetInterval;
        monsterAI.m_circleTargetDuration = data.CircleTargetDuration;
        monsterAI.m_circleTargetDistance = data.CircleTargetDistance;
        // Sleep
        monsterAI.m_sleeping = data.Sleeping;
        monsterAI.m_wakeupRange = data.WakeUpRange;
        monsterAI.m_noiseWakeup = data.NoiseWakeUp;
        monsterAI.m_maxNoiseWakeupRange = data.MaximumNoiseWakeUpRange;

        monsterAI.m_wakeUpDelayMin = data.WakeUpDelayMinimum;
        monsterAI.m_wakeUpDelayMax = data.WakeUpDelayMaximum;
        // Other
        monsterAI.m_avoidLand = data.AvoidLand;
        // Consume Items 
        if (data.CHANGE_MONSTER_CONSUME || reset)
        {
            if (GetItemDrops(data.ConsumeItems, out List<ItemDrop> ConsumeItems)) monsterAI.m_consumeItems = ConsumeItems;
            monsterAI.m_consumeRange = data.ConsumeRange;
            monsterAI.m_consumeSearchRange = data.ConsumeSearchRange;
            monsterAI.m_consumeSearchInterval = data.ConsumeSearchInterval;
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Updated " + data.PrefabName + " consume items");
        }
    }

    private static void UpdateAnimalAI(MonsterData data, AnimalAI animalAI, bool reset)
    {
        animalAI.m_viewRange = data.ViewRange;
        animalAI.m_viewAngle = data.ViewAngle;
        animalAI.m_hearRange = data.HearRange;
        animalAI.m_mistVision = data.MistVision;
        if (data.CHANGE_MONSTER_EFFECTS || reset)
        {
            if (GetEffects(data.AlertedEffects, out EffectList AlertedEffects))
                animalAI.m_alertedEffects = AlertedEffects;
            if (GetEffects(data.IdleSound, out EffectList IdleSounds))
                animalAI.m_idleSound = IdleSounds;
        }
        animalAI.m_idleSoundInterval = data.IdleSoundInterval;
        animalAI.m_idleSoundChance = data.IdleSoundChance;
        if (GetPathType(data.PathAgentType, out Pathfinding.AgentType PathAgentType))
            animalAI.m_pathAgentType = PathAgentType;
        else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update path type of " + data.PrefabName);
        animalAI.m_moveMinAngle = data.MoveMinimumAngle;
        animalAI.m_smoothMovement = data.smoothMovement;
        animalAI.m_serpentMovement = data.SerpentMovement;
        animalAI.m_serpentTurnRadius = data.SerpentTurnRadius;
        animalAI.m_jumpInterval = data.JumpInterval;
        // Random Circle
        animalAI.m_randomCircleInterval = data.RandomCircleInterval;
        // Random Movement
        animalAI.m_randomMoveInterval = data.RandomMoveInterval;
        animalAI.m_randomMoveRange = data.RandomMoveRange;
        // Fly Behavior
        animalAI.m_randomFly = data.RandomFly;
        animalAI.m_chanceToTakeoff = data.ChanceToTakeOff;
        animalAI.m_chanceToLand = data.ChanceToLand;
        animalAI.m_groundDuration = data.GroundDuration;
        animalAI.m_airDuration = data.AirDuration;
        animalAI.m_maxLandAltitude = data.MaxLandAltitude;
        animalAI.m_takeoffTime = data.TakeOffTime;
        animalAI.m_flyAltitudeMin = data.FlyAltitudeMinimum;
        animalAI.m_flyAltitudeMax = data.FlyAltitudeMaximum;
        animalAI.m_flyAbsMinAltitude = data.FlyAbsoluteMinimumAltitude;
        // Other
        animalAI.m_avoidFire = data.AvoidFire;
        animalAI.m_afraidOfFire = data.AfraidOfFire;
        animalAI.m_avoidWater = data.AvoidWater;
        animalAI.m_aggravatable = data.Aggravatable;
        animalAI.m_passiveAggresive = data.PassiveAggressive;
        animalAI.m_spawnMessage = data.SpawnMessage;
        animalAI.m_deathMessage = data.DeathMessage;
        animalAI.m_alertedMessage = data.AlertedMessage;
        animalAI.m_timeToSafe = data.TimeToSafe;
    }

    private static void UpdateMonsterProcreation(MonsterData data, GameObject critter, bool reset)
    {
        if (critter.TryGetComponent(out Procreation procreation) && (data.CHANGE_PROCREATION || reset))
        {
            procreation.m_updateInterval = data.UpdateInterval;
            procreation.m_totalCheckRange = data.TotalCheckRange;
            procreation.m_maxCreatures = data.MaxCreatures;
            procreation.m_partnerCheckRange = data.PartnerCheckRange;
            procreation.m_pregnancyChance = data.PregnancyChance;
            procreation.m_pregnancyDuration = data.PregnancyDuration;
            procreation.m_requiredLovePoints = data.RequiredLovePoints;
            if (data.ChangeOffSpring)
            {
                if (GetOffspring(data.OffSpring, out GameObject Offspring)) procreation.m_offspring = Offspring;
                else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to change offspring of " + critter.name);
            }
            procreation.m_minOffspringLevel = data.MinimumOffspringLevel;
            procreation.m_spawnOffset = data.SpawnOffset;

            if (data.ChangeEffects)
            {
                if (GetEffects(data.BirthEffects, out EffectList BirthEffects))
                    procreation.m_birthEffects = BirthEffects;
                if (GetEffects(data.LoveEffects, out EffectList LoveEffects))
                    procreation.m_loveEffects = LoveEffects;
            }
        }
    }

    private static void UpdateMonsterTame(MonsterData data, GameObject critter, bool reset)
    {
        if (reset)
        {
            if (m_newTames.Contains(data.PrefabName))
            {
                if (critter.TryGetComponent(out Tameable tameable))
                {
                    UnityEngine.Object.Destroy(tameable);
                    m_newTames.Remove(data.PrefabName);
                }
            }
        }
        else if (data.CHANGE_TAMEABLE || reset)
        {
            if (!critter.TryGetComponent(out Tameable tameable))
            {
                if (data.MAKE_TAMEABLE)
                {
                    tameable = critter.AddComponent<Tameable>();
                    m_newTames.Add(data.PrefabName);
                }
            }

            if (tameable != null)
            {
                tameable.m_fedDuration = data.FedDuration;
                tameable.m_tamingTime = data.TamingTime;
                tameable.m_startsTamed = data.StartsTamed;

                if (data.CHANGE_TAME_EFFECTS)
                {
                    if (GetEffects(data.TamedEffects, out EffectList TameEffects))
                        tameable.m_tamedEffect = TameEffects;
                    if (GetEffects(data.SootheEffects, out EffectList SoothEffects))
                        tameable.m_sootheEffect = SoothEffects;
                    if (GetEffects(data.PetEffects, out EffectList PetEffects)) tameable.m_petEffect = PetEffects;
                    if (GetEffects(data.UnSummonEffects, out EffectList UnSummonEffects))
                        tameable.m_unSummonEffect = UnSummonEffects;
                }
                tameable.m_commandable = data.Commandable;
                tameable.m_unsummonDistance = data.UnSummonDistance;
                tameable.m_unsummonOnOwnerLogoutSeconds = data.UnSummonOnOwnerLogoutSeconds;
                if (GetSkill(data.LevelUpOwnerSkill, out Skills.SkillType skillType))
                    tameable.m_levelUpOwnerSkill = skillType;
                tameable.m_levelUpFactor = data.LevelUpFactor;
                if (GetSaddle(data.SaddleItem, out ItemDrop Saddle)) tameable.m_saddleItem = Saddle;
                tameable.m_dropSaddleOnDeath = data.DropSaddleOnDeath;
                tameable.m_dropItemVel = data.DropItemVelocity;
                tameable.m_randomStartingName = data.RandomStartingName;
            }
        }
    }

    private static Material CreateMaterial(MaterialData materialData, Material material)
    {
        Material newMat = new Material(material)
        {
            name = material.name
        };
        if (GetTexture2D(materialData.MainTexture, out Texture2D? MainTexture))
        {
            newMat.mainTexture = MainTexture;
        }
        newMat.color = new Color(materialData.MainColor.Red, materialData.MainColor.Green, materialData.MainColor.Blue, materialData.MainColor.Alpha);
        if (newMat.HasFloat(Hue)) newMat.SetFloat(Hue, materialData.Hue);
        if (newMat.HasFloat(Saturation)) newMat.SetFloat(Saturation, materialData.Saturation);
        if (newMat.HasFloat(Value)) newMat.SetFloat(Value, materialData.Value);
        if (newMat.HasFloat(Cutoff)) newMat.SetFloat(Cutoff, materialData.AlphaCutoff);
        if (newMat.HasFloat(Smoothness)) newMat.SetFloat(Smoothness, materialData.Smoothness);
        if (newMat.HasFloat(UseGlossMap)) newMat.SetFloat(UseGlossMap, materialData.UseGlossMap ? 1f : 0f);
        if (newMat.HasFloat(Metallic)) newMat.SetFloat(Metallic, materialData.Metallic);
        if (newMat.HasTexture(MetallicGlossMap))
        {
            if (GetTexture2D(materialData.GlossTexture, out Texture2D? GlossTexture))
            {
                newMat.SetTexture(MetallicGlossMap, GlossTexture);
            }
        }
        if (newMat.HasFloat(Metallic)) newMat.SetFloat(Metallic, materialData.Metallic);
        if (newMat.HasFloat(MetalGloss)) newMat.SetFloat(MetalGloss, materialData.MetalGloss);
        if (newMat.HasColor(MetalColor)) newMat.SetColor(MetalColor, new Color(materialData.MetalColor.Red, materialData.MetalColor.Green, materialData.MetalColor.Blue, materialData.MetalColor.Alpha));
        if (newMat.HasTexture(EmissionMap))
        {
            if (GetTexture2D(materialData.EmissionTexture, out Texture2D? EmissionTexture))
            {
                newMat.SetTexture(EmissionMap, EmissionTexture);
            }
        }
        if (newMat.HasColor(EmissionColor)) newMat.SetColor(EmissionColor, new Color(materialData.EmissionColor.Red, materialData.EmissionColor.Green, materialData.EmissionColor.Blue, materialData.EmissionColor.Alpha));
        if (newMat.HasFloat(NormalStrength)) newMat.SetFloat(NormalStrength, materialData.BumpStrength);

        if (newMat.HasTexture(BumpMap))
        {
            if (GetTexture2D(materialData.BumpTexture, out Texture2D? BumpTexture))
            {
                newMat.SetTexture(BumpMap, BumpTexture);
            }
        }
        if (newMat.HasFloat(TwoSidedNormals)) newMat.SetFloat(TwoSidedNormals, materialData.TwoSidedNormals ? 1f : 0f);
        if (newMat.HasFloat(UseStyles)) newMat.SetFloat(UseStyles, materialData.UseStyles ? 1f: 0f);
        if (newMat.HasFloat(Style)) newMat.SetFloat(Style, materialData.Style);
        if (newMat.HasTexture(StyleTex))
        {
            if (GetTexture2D(materialData.StyleTexture, out Texture2D? StyleTexture))
            {
                newMat.SetTexture(StyleTex, StyleTexture);

            }
        }
        if (newMat.HasFloat(AddRain)) newMat.SetFloat(AddRain, materialData.AddRain ? 1f: 0f);
        return newMat;
    }

    private static void UpdateMonsterMaterial(MonsterData data, GameObject critter, bool reset)
    {
        if (data.Materials.TrueForAll(x => !x.Enabled) && !reset) return;
        SkinnedMeshRenderer renderer = critter.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer == null) return;
        
        List<Material> newMaterials = new();
        foreach (var materialData in data.Materials)
        {
            Material material = renderer.materials.ToList().Find(x =>
                x.name.Replace("(Instance)", string.Empty).TrimEnd() == materialData.MaterialName);
            if (material == null) return;
            if (!materialData.Enabled && !reset)
            {
                newMaterials.Add(material);
            }
            else
            {
                newMaterials.Add(CreateMaterial(materialData, material));
            }
        }

        if (newMaterials.Count == 0) return;

        renderer.materials = newMaterials.ToArray();
        renderer.sharedMaterials = newMaterials.ToArray();

        if (critter.TryGetComponent(out Humanoid humanoid))
        {
            UpdateRagDollMaterials(humanoid.m_deathEffects.m_effectPrefabs, newMaterials);
        }
        else if (critter.TryGetComponent(out Character character))
        {
            UpdateRagDollMaterials(character.m_deathEffects.m_effectPrefabs, newMaterials);
        }
    }

    private static void UpdateRagDollMaterials(EffectList.EffectData[] deathEffects, List<Material> newMaterials)
    {
        foreach (var effect in deathEffects)
        {
            if (!effect.m_prefab.TryGetComponent(out Ragdoll ragdoll)) continue;
            ragdoll.m_mainModel.materials = newMaterials.ToArray();
            ragdoll.m_mainModel.sharedMaterials = newMaterials.ToArray();
        }
    }

    private static void UpdateMonsterModel(MonsterData data, GameObject critter, bool reset)
    {
        if (critter.TryGetComponent(out VisEquipment visEquipment))
        {
            visEquipment.SetModel(data.FemaleModel ? 1 : 0);
        }
    }

    private static bool UpdateMonsterData(MonsterData data, bool reset = false)
    {
        if (!ZNetScene.instance) return false;
        GameObject critter = ZNetScene.instance.GetPrefab(data.PrefabName);
        if (!critter)
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find " + data.PrefabName);
            return false;
        }
        
        UpdateMonsterScale(data, critter, reset);

        if (critter.TryGetComponent(out Humanoid humanoid) && (data.ENABLED || reset))
        {
            UpdateMonsterHumanoid(data, humanoid, reset);
        } 
        else if (critter.TryGetComponent(out Character character) && (data.ENABLED || reset))
        {
            UpdateMonsterCharacter(data, character, reset);
        }

        if (critter.TryGetComponent(out MonsterAI monsterAI) && (data.CHANGE_AI || reset))
        {
            UpdateMonsterAI(data, monsterAI, reset);
        } 
        else if (critter.TryGetComponent(out AnimalAI animalAI) && (data.CHANGE_AI || reset))
        {
            UpdateAnimalAI(data, animalAI, reset);
        }

        if (critter.TryGetComponent(out CharacterDrop characterDrop) && (data.CHANGE_DROP_TABLE || reset))
        {
            if (GetDrops(data.Drops, out List<CharacterDrop.Drop> Drops)) characterDrop.m_drops = Drops;
        }

        UpdateMonsterProcreation(data, critter, reset);

        UpdateMonsterTame(data, critter, reset);

        UpdateMonsterMaterial(data, critter, reset);

        UpdateMonsterModel(data, critter, reset);

        return true;
    }

    private static bool GetTexture2D(string input, out Texture2D? output)
    {
        output = null;
        if (TextureManager.RegisteredTextures.TryGetValue(input, out Texture2D tex))
        {
            output = tex;
            return true;
        }

        if (MonsterDB.m_textures.TryGetValue(input, out Texture2D texture))
        {
            output = texture;
            return true;
        }
        return false;
    }

    private static bool GetSaddle(string input, out ItemDrop output)
    {
        output = null!;
        GameObject? saddle = MonsterDB.TryGetGameObject(input);
        if (saddle == null) return false;
        if (saddle.TryGetComponent(out ItemDrop component))
        {
            output = component;
            return true;
        }
        return false;
    }

    private static bool GetSkill(string input, out Skills.SkillType output)
    {
        output = Skills.SkillType.None;
        if (Enum.TryParse(input, out Skills.SkillType type))
        {
            output = type;
            return true;
        }
        output = (Skills.SkillType)Math.Abs(input.GetStableHashCode());
        return Player.m_localPlayer && Player.m_localPlayer.m_skills.GetSkillDef(output) != null;
    }

    private static bool GetPathType(string input, out Pathfinding.AgentType output)
    {
        output = Pathfinding.AgentType.Humanoid;
        if (!Enum.TryParse(input, out Pathfinding.AgentType type)) return false; 
        output = type;
        return true;
    }

    private static bool GetDrops(List<DropData> input, out List<CharacterDrop.Drop> output)
    {
        output = new();
        foreach (var data in input)
        {
            GameObject? prefab = MonsterDB.TryGetGameObject(data.Prefab);
            if (prefab == null) continue;
            output.Add(new()
            {
                m_prefab = prefab,
                m_amountMin = data.AmountMinimum,
                m_amountMax = data.AmountMaximum,
                m_chance = data.Chance,
                m_onePerPlayer = data.OnePerPlayer,
                m_levelMultiplier = data.LevelMultiplier,
                m_dontScale = data.DoNotScale
            });
        }

        return output.Count > 0;
    }

    private static bool GetOffspring(string input, out GameObject output)
    {
        output = MonsterDB.TryGetGameObject(input)!;
        return output != null;
    }

    private static bool GetRandomSets(List<RandomSet> input, bool reset, out List<Humanoid.ItemSet> output)
    {
        output = new();
        foreach (RandomSet data in input)
        {
            Humanoid.ItemSet set = new()
            {
                m_name = data.Name
            };
            if (GetItems(data.Items, reset, out List<GameObject> items))
            {
                set.m_items = items.ToArray();
            }
            output.Add(set);
        }

        return true;
    }

    private static bool GetItemDrops(List<string> input, out List<ItemDrop> output)
    {
        output = new List<ItemDrop>();
        foreach (string itemName in input)
        {
            GameObject? item = MonsterDB.TryGetGameObject(itemName);
            if (item == null)
            {
                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find item " + itemName);
                continue;
            }
            if (!item.TryGetComponent(out ItemDrop component))
            {
                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find item drop for " + itemName);
                continue;
            }
            output.Add(component);
        }

        return output.Count > 0;
    }

    private static bool GetItems(List<CreatureItem> input, bool reset, out List<GameObject> output)
    {
        output = new List<GameObject>();
        foreach (var item in input)
        {
            if (!item.ENABLE_ITEM) continue;
            GameObject? prefab = MonsterDB.TryGetGameObject(item.Name);
            if (prefab == null) continue;
            if (!prefab.GetComponent<ItemDrop>()) continue;
            if (reset || !item.CHANGE_ITEM_DATA) return prefab;
            var clone = UnityEngine.Object.Instantiate(prefab, MonsterDBPlugin._Root.transform, false);
            clone.name = item.Name;
            if (!clone.TryGetComponent(out ItemDrop component)) continue;

            if (!item.AttackAnimation.IsNullOrWhiteSpace())
            {
                component.m_itemData.m_shared.m_attack.m_attackAnimation = item.AttackAnimation;
            }
            
            component.m_itemData.m_shared.m_attack.m_attackOriginJoint = item.AttackOrigin;
            component.m_itemData.m_shared.m_attack.m_hitTerrain = item.HitTerrain;
            component.m_itemData.m_shared.m_attack.m_hitFriendly = item.HitFriendly;
            component.m_itemData.m_shared.m_aiAttackRange = item.AttackRange;
            component.m_itemData.m_shared.m_aiAttackRangeMin = item.AttackRangeMinimum;
            component.m_itemData.m_shared.m_aiAttackInterval = item.AttackInterval;
            component.m_itemData.m_shared.m_aiAttackMaxAngle = item.AttackMaxAngle;
            component.m_itemData.m_shared.m_damages = GetDamages(item.AttackDamages);
            component.m_itemData.m_shared.m_toolTier = item.ToolTier;
            component.m_itemData.m_shared.m_attackForce = item.AttackForce;
            component.m_itemData.m_shared.m_dodgeable = item.Dodgeable;
            component.m_itemData.m_shared.m_blockable = item.Blockable;
            component.m_itemData.m_shared.m_spawnOnHit = MonsterDB.TryGetGameObject(item.SpawnOnHit);
            component.m_itemData.m_shared.m_spawnOnHitTerrain = MonsterDB.TryGetGameObject(item.SpawnOnHitTerrain);
            
            if (!item.AttackStatusEffect.IsNullOrWhiteSpace())
            {
                var effect = ObjectDB.instance.GetStatusEffect(item.AttackStatusEffect.GetStableHashCode());
                if (effect)
                {
                    component.m_itemData.m_shared.m_attackStatusEffect = effect;
                }
            }

            component.m_itemData.m_shared.m_attack.m_hitThroughWalls = item.HitThroughWalls;

            if (item.CHANGE_ITEM_EFFECTS)
            {
                if (GetEffects(item.HitEffects, out EffectList hitEffects))
                    component.m_itemData.m_shared.m_attack.m_hitEffect = hitEffects;
                if (GetEffects(item.HitTerrainEffects, out EffectList hitTerrainEffects)) component.m_itemData
                    .m_shared.m_attack.m_hitTerrainEffect = hitTerrainEffects;
                if (GetEffects(item.StartEffects, out EffectList startEffects))
                    component.m_itemData.m_shared.m_attack.m_startEffect = startEffects;
                if (GetEffects(item.TriggerEffects, out EffectList triggerEffects))
                    component.m_itemData.m_shared.m_attack.m_triggerEffect = triggerEffects;
                if (GetEffects(item.TrailStartEffects, out EffectList trailStartEffects))
                    component.m_itemData.m_shared.m_attack.m_trailStartEffect = trailStartEffects;
                if (GetEffects(item.BurstEffects, out EffectList burstEffects))
                    component.m_itemData.m_shared.m_attack.m_burstEffect = burstEffects;
            }

            if (item.CHANGE_PROJECTILE)
            {
                GameObject? projectilePrefab = MonsterDB.TryGetGameObject(item.Projectile_PrefabName);
                if (projectilePrefab != null && !item.Projectile_Clone_ID.IsNullOrWhiteSpace())
                {
                    var cloneProjectile = MonsterDB.TryGetGameObject(item.Projectile_Clone_ID);
                    if (!cloneProjectile)
                    {
                        cloneProjectile = Object.Instantiate(projectilePrefab, MonsterDBPlugin._Root.transform, false);
                        cloneProjectile.name = item.Projectile_Clone_ID;
                    }
                    if (cloneProjectile != null && item.CHANGE_PROJECTILE_DATA && cloneProjectile.TryGetComponent(out Projectile projectile))
                    {
                        projectile.m_damage = GetDamages(item.Projectile_Damages);
                        projectile.m_aoe = item.Projectile_AOE;
                        projectile.m_dodgeable = item.Projectile_Dodgeable;
                        projectile.m_blockable = item.Projectile_Blockable;
                        projectile.m_attackForce = item.Projectile_AttackForce;
                        if (!item.Projectile_StatusEffect.IsNullOrWhiteSpace())
                        {
                            if (ObjectDB.instance.GetStatusEffect(item.Projectile_StatusEffect.GetStableHashCode()))
                            {
                                projectile.m_statusEffect = item.Projectile_StatusEffect;
                            }
                        }

                        projectile.m_canHitWater = item.Projectile_CanHitWater;
                        if (!item.Projectile_SpawnOnHit.IsNullOrWhiteSpace())
                        {
                            GameObject? spawnOnHit = MonsterDB.TryGetGameObject(item.Projectile_SpawnOnHit);
                            if (spawnOnHit != null)
                            {
                                projectile.m_spawnOnHit = spawnOnHit;
                            }
                        }

                        projectile.m_spawnOnHitChance = item.Projectile_SpawnChance;
                        projectile.m_randomSpawnOnHit = item.Projectile_RandomSpawnOnHit.Select(MonsterDB.TryGetGameObject).Where(random => random != null).ToList();
                        projectile.m_staticHitOnly = item.Projectile_StaticHitOnly;
                        projectile.m_groundHitOnly = item.Projectile_GroundHitOnly;

                        if (item.PROJECTILE_CHANGE_EFFECTS)
                        {
                            if (GetEffects(item.Projectile_HitEffects, out EffectList projectileHitEffects))
                                projectile.m_hitEffects = projectileHitEffects;
                            if (GetEffects(item.Projectile_HitWaterEffects, out EffectList projectileHitWaterEffects))
                                projectile.m_hitWaterEffects = projectileHitWaterEffects;
                            if (GetEffects(item.Projectile_SpawnOnHitEffects, out EffectList projectileSpawnOnHitEffects))
                                projectile.m_spawnOnHitEffects = projectileSpawnOnHitEffects;
                        }
                    }

                    if (cloneProjectile != null)
                    {
                        RegisterToZNetScene(cloneProjectile);
                        component.m_itemData.m_shared.m_attack.m_attackProjectile = cloneProjectile;
                    }
                    
                }
            }
            
            output.Add(clone);
        }

        return true;
    }

    private static bool GetDamageMods(DamageModifiersData input, out HitData.DamageModifiers output)
    {
        output = new();
        if (!Enum.TryParse(input.Blunt, out HitData.DamageModifier Blunt)) return false;
        if (!Enum.TryParse(input.Slash, out HitData.DamageModifier Slash)) return false;
        if (!Enum.TryParse(input.Pierce, out HitData.DamageModifier Pierce)) return false;
        if (!Enum.TryParse(input.Chop, out HitData.DamageModifier Chop)) return false;
        if (!Enum.TryParse(input.Pickaxe, out HitData.DamageModifier Pickaxe)) return false;
        if (!Enum.TryParse(input.Fire, out HitData.DamageModifier Fire)) return false;
        if (!Enum.TryParse(input.Frost, out HitData.DamageModifier Frost)) return false;
        if (!Enum.TryParse(input.Lightning, out HitData.DamageModifier Lightning)) return false;
        if (!Enum.TryParse(input.Poison, out HitData.DamageModifier Poison)) return false;
        if (!Enum.TryParse(input.Spirit, out HitData.DamageModifier Spirit)) return false;
        
        output.m_blunt = Blunt;
        output.m_slash = Slash;
        output.m_pierce = Pierce;
        output.m_chop = Chop;
        output.m_pickaxe = Pickaxe;
        output.m_fire = Fire;
        output.m_frost = Frost;
        output.m_lightning = Lightning;
        output.m_poison = Poison;
        output.m_spirit = Spirit;
        return true;
    }

    private static bool GetEffects(List<EffectPrefab> input, out EffectList output)
    {
        output = new();
        if (!ZNetScene.instance) return false;
        List<EffectList.EffectData> effectData = new();
        foreach (EffectPrefab data in input)
        {
            GameObject? effect = MonsterDB.TryGetGameObject(data.Prefab);
            if (effect == null)
            {
                MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find effect " + data.Prefab);
                continue;
            }
            effectData.Add(new()
            {
                m_prefab = effect,
                m_enabled = data.Enabled,
                m_variant = data.Variant,
                m_attach = data.Attach,
                m_follow = data.Follow,
                m_inheritParentRotation = data.InheritParentRotation,
                m_inheritParentScale = data.InheritParentScale,
                m_randomRotation = data.RandomRotation,
                m_multiplyParentVisualScale = data.MultiplyParentVisualScale,
                m_scale = data.Scale
            });
        }

        if (effectData.Count <= 0) return false;
        output.m_effectPrefabs = effectData.ToArray();
        return true;
    }

    private static bool GetFaction(string input, out Character.Faction output)
    {
        output = Character.Faction.Boss;
        if (!Enum.TryParse(input, out Character.Faction faction)) return false;
        output = faction;
        return true;
    }

    private static bool GetGroundTilt(string input, out Character.GroundTiltType output)
    {
        output = Character.GroundTiltType.None;
        if (!Enum.TryParse(input, out Character.GroundTiltType type)) return false;
        output = type;
        return true;
    }

    private static void SaveHumanoidData(ref MonsterData data, Humanoid humanoid)
    {
        data.DisplayName = humanoid.m_name;
        data.Group = humanoid.m_group;
        data.Faction = humanoid.m_faction.ToString();
        data.Boss = humanoid.m_boss;
        data.DoNotHideBossHUD = humanoid.m_dontHideBossHud;
        data.BossEvent = humanoid.m_bossEvent;
        data.DefeatSetGlobalKey = humanoid.m_defeatSetGlobalKey;
        // Movement & Physics
        data.CrouchSpeed = humanoid.m_crouchSpeed;
        data.WalkSpeed = humanoid.m_walkSpeed;
        data.Speed = humanoid.m_speed;
        data.TurnSpeed = humanoid.m_turnSpeed;
        data.RunSpeed = humanoid.m_runSpeed;
        data.RunTurnSpeed = humanoid.m_runTurnSpeed;
        data.FlySlowSpeed = humanoid.m_flySlowSpeed;
        data.FlyFastSpeed = humanoid.m_flyFastSpeed;
        data.FlyTurnSpeed = humanoid.m_flyTurnSpeed;
        data.Acceleration = humanoid.m_acceleration;
        data.JumpForce = humanoid.m_jumpForce;
        data.JumpForceForward = humanoid.m_jumpForceForward;
        data.JumpForceTiredFactor = humanoid.m_jumpForceTiredFactor;
        data.AirControl = humanoid.m_airControl;
        data.CanSwim = humanoid.m_canSwim;
        data.SwimDepth = humanoid.m_swimDepth;
        data.SwimSpeed = humanoid.m_swimSpeed;
        data.SwimTurnSpeed = humanoid.m_swimTurnSpeed;
        data.SwimAcceleration = humanoid.m_swimAcceleration;
        data.GroundTilt = humanoid.m_groundTilt.ToString();
        data.GroundTiltSpeed = humanoid.m_groundTiltSpeed;
        data.Flying = humanoid.m_flying;
        data.JumpStaminaUsage = humanoid.m_jumpStaminaUsage;
        data.DisableWhileSleeping = humanoid.m_disableWhileSleeping;
        // Effects
        data.HitEffects = FormatEffectData(humanoid.m_hitEffects);
        data.CriticalHitEffects = FormatEffectData(humanoid.m_critHitEffects);
        data.BackStabHitEffects = FormatEffectData(humanoid.m_backstabHitEffects);
        data.DeathEffects = FormatEffectData(humanoid.m_deathEffects);
        data.WaterEffects = FormatEffectData(humanoid.m_waterEffects);
        data.TarEffects = FormatEffectData(humanoid.m_tarEffects);
        data.SlideEffects = FormatEffectData(humanoid.m_slideEffects);
        data.JumpEffects = FormatEffectData(humanoid.m_jumpEffects);
        data.FlyingContinuousEffects = FormatEffectData(humanoid.m_flyingContinuousEffect);
        // Health & Damages
        data.TolerateWater = humanoid.m_tolerateWater;
        data.TolerateFire = humanoid.m_tolerateFire;
        data.TolerateSmoke = humanoid.m_tolerateSmoke;
        data.TolerateTar = humanoid.m_tolerateTar;
        data.Health = humanoid.m_health;
        data.DamageModifiers = FormatDamageModifiers(humanoid.m_damageModifiers);
        data.StaggerWhenBlocked = humanoid.m_staggerWhenBlocked;
        data.StaggerDamageFactor = humanoid.m_staggerDamageFactor;
        data.DefaultItems = FormatItemList(humanoid.m_defaultItems);
        data.RandomWeapons = FormatItemList(humanoid.m_randomWeapon);
        data.RandomArmors = FormatItemList(humanoid.m_randomArmor);
        data.RandomShields = FormatItemList(humanoid.m_randomShield);
        data.RandomSets = FormatRandomSets(humanoid.m_randomSets);
        data.UnarmedWeapon = humanoid.m_unarmedWeapon ? humanoid.m_unarmedWeapon.name : "";
        data.BeardItem = humanoid.m_beardItem;
        data.HairItem = humanoid.m_hairItem;
        // Effects
        data.PickUpEffects = FormatEffectData(humanoid.m_pickupEffects);
        data.DropEffects = FormatEffectData(humanoid.m_dropEffects);
        data.ConsumeItemEffects = FormatEffectData(humanoid.m_consumeItemEffects);
        data.EquipEffects = FormatEffectData(humanoid.m_equipEffects);
        data.PerfectBlockEffect = FormatEffectData(humanoid.m_perfectBlockEffect);
        
    }

    private static void SaveCharacterData(ref MonsterData data, Character character)
    {
        data.DisplayName = character.m_name;
        data.Group = character.m_group;
        data.Faction = character.m_faction.ToString();
        data.Boss = character.m_boss;
        data.DoNotHideBossHUD = character.m_dontHideBossHud;
        data.BossEvent = character.m_bossEvent;
        data.DefeatSetGlobalKey = character.m_defeatSetGlobalKey;
        // Movement & Physics
        data.CrouchSpeed = character.m_crouchSpeed;
        data.WalkSpeed = character.m_walkSpeed;
        data.Speed = character.m_speed;
        data.TurnSpeed = character.m_turnSpeed;
        data.RunSpeed = character.m_runSpeed;
        data.RunTurnSpeed = character.m_runTurnSpeed;
        data.FlySlowSpeed = character.m_flySlowSpeed;
        data.FlyFastSpeed = character.m_flyFastSpeed;
        data.FlyTurnSpeed = character.m_flyTurnSpeed;
        data.Acceleration = character.m_acceleration;
        data.JumpForce = character.m_jumpForce;
        data.JumpForceForward = character.m_jumpForceForward;
        data.JumpForceTiredFactor = character.m_jumpForceTiredFactor;
        data.AirControl = character.m_airControl;
        data.CanSwim = character.m_canSwim;
        data.SwimDepth = character.m_swimDepth;
        data.SwimSpeed = character.m_swimSpeed;
        data.SwimTurnSpeed = character.m_swimTurnSpeed;
        data.SwimAcceleration = character.m_swimAcceleration;
        data.GroundTilt = character.m_groundTilt.ToString();
        data.GroundTiltSpeed = character.m_groundTiltSpeed;
        data.Flying = character.m_flying;
        data.JumpStaminaUsage = character.m_jumpStaminaUsage;
        data.DisableWhileSleeping = character.m_disableWhileSleeping;
        // Effects
        data.HitEffects = FormatEffectData(character.m_hitEffects);
        data.CriticalHitEffects = FormatEffectData(character.m_critHitEffects);
        data.BackStabHitEffects = FormatEffectData(character.m_backstabHitEffects);
        data.DeathEffects = FormatEffectData(character.m_deathEffects);
        data.WaterEffects = FormatEffectData(character.m_waterEffects);
        data.TarEffects = FormatEffectData(character.m_tarEffects);
        data.SlideEffects = FormatEffectData(character.m_slideEffects);
        data.JumpEffects = FormatEffectData(character.m_jumpEffects);
        data.FlyingContinuousEffects = FormatEffectData(character.m_flyingContinuousEffect);
        // Health & Damages
        data.TolerateWater = character.m_tolerateWater;
        data.TolerateFire = character.m_tolerateFire;
        data.TolerateSmoke = character.m_tolerateSmoke;
        data.TolerateTar = character.m_tolerateTar;
        data.Health = character.m_health;
        data.DamageModifiers = FormatDamageModifiers(character.m_damageModifiers);
        data.StaggerWhenBlocked = character.m_staggerWhenBlocked;
        data.StaggerDamageFactor = character.m_staggerDamageFactor;
    }

    private static void SaveMonsterAIData(ref MonsterData data, MonsterAI monsterAI)
    {
        data.ViewRange = monsterAI.m_viewRange;
        data.ViewAngle = monsterAI.m_viewAngle;
        data.HearRange = monsterAI.m_hearRange;
        data.MistVision = monsterAI.m_mistVision;
        data.AlertedEffects = FormatEffectData(monsterAI.m_alertedEffects);
        data.IdleSound = FormatEffectData(monsterAI.m_idleSound);
        data.IdleSoundInterval = monsterAI.m_idleSoundInterval;
        data.IdleSoundChance = monsterAI.m_idleSoundChance;
        data.PathAgentType = monsterAI.m_pathAgentType.ToString();
        data.MoveMinimumAngle = monsterAI.m_moveMinAngle;
        data.smoothMovement = monsterAI.m_smoothMovement;
        data.SerpentMovement = monsterAI.m_serpentMovement;
        data.SerpentTurnRadius = monsterAI.m_serpentTurnRadius;
        data.JumpInterval = monsterAI.m_jumpInterval;
        // Random Circle
        data.RandomCircleInterval = monsterAI.m_randomCircleInterval;
        // Random Movement
        data.RandomMoveInterval = monsterAI.m_randomMoveInterval;
        data.RandomMoveRange = monsterAI.m_randomMoveRange;
        // Fly Behavior
        data.RandomFly = monsterAI.m_randomFly;
        data.ChanceToTakeOff = monsterAI.m_chanceToTakeoff;
        data.ChanceToLand = monsterAI.m_chanceToLand;
        data.GroundDuration = monsterAI.m_groundDuration;
        data.AirDuration = monsterAI.m_airDuration;
        data.MaxLandAltitude = monsterAI.m_maxLandAltitude;
        data.TakeOffTime = monsterAI.m_takeoffTime;
        data.FlyAltitudeMinimum = monsterAI.m_flyAltitudeMin;
        data.FlyAltitudeMaximum = monsterAI.m_flyAltitudeMax;
        data.FlyAbsoluteMinimumAltitude = monsterAI.m_flyAbsMinAltitude;
        // Other
        data.AvoidFire = monsterAI.m_avoidFire;
        data.AfraidOfFire = monsterAI.m_afraidOfFire;
        data.AvoidWater = monsterAI.m_avoidWater;
        data.Aggravatable = monsterAI.m_aggravatable;
        data.PassiveAggressive = monsterAI.m_passiveAggresive;
        data.SpawnMessage = monsterAI.m_spawnMessage;
        data.DeathMessage = monsterAI.m_deathMessage;
        data.AlertedMessage = monsterAI.m_alertedMessage;
        // Monster AI
        data.AlertRange = monsterAI.m_alertRange;
        data.FleeIfHurtWhenTargetCannotBeReached = monsterAI.m_fleeIfHurtWhenTargetCantBeReached;
        data.FleeIfNotAlerted = monsterAI.m_fleeIfNotAlerted;
        data.FleeIfLowHealth = monsterAI.m_fleeIfLowHealth;
        data.CirculateWhileCharging = monsterAI.m_circulateWhileCharging;
        data.CirculateWhileChargingFlying = monsterAI.m_circulateWhileChargingFlying;
        data.EnableHuntPlayer = monsterAI.m_enableHuntPlayer;
        data.AttackPlayerObjects = monsterAI.m_attackPlayerObjects;
        data.PrivateAreaTriggerThreshold = monsterAI.m_privateAreaTriggerTreshold;
        data.InterceptTimeMaximum = monsterAI.m_interceptTimeMax;
        data.InterceptTimeMinimum = monsterAI.m_interceptTimeMin;
        data.MaximumChaseDistance = monsterAI.m_maxChaseDistance;
        data.MinimumAttackInterval = monsterAI.m_minAttackInterval;
        // Circle Target
        data.CircleTargetInterval = monsterAI.m_circleTargetInterval;
        data.CircleTargetDuration = monsterAI.m_circleTargetDuration;
        data.CircleTargetDistance = monsterAI.m_circleTargetDistance;
        // Sleep
        data.Sleeping = monsterAI.m_sleeping;
        data.WakeUpRange = monsterAI.m_wakeupRange;
        data.NoiseWakeUp = monsterAI.m_noiseWakeup;
        data.MaximumNoiseWakeUpRange = monsterAI.m_maxNoiseWakeupRange;
        data.WakeUpEffects = FormatEffectData(monsterAI.m_wakeupEffects);
        data.WakeUpDelayMinimum = monsterAI.m_wakeUpDelayMin;
        data.WakeUpDelayMaximum = monsterAI.m_wakeUpDelayMax;
        // Other
        data.AvoidLand = monsterAI.m_avoidLand;
        // Consume Items
        data.ConsumeItems = FormatItemList(monsterAI.m_consumeItems);
        data.ConsumeRange = monsterAI.m_consumeRange;
        data.ConsumeSearchRange = monsterAI.m_consumeSearchRange;
        data.ConsumeSearchInterval = monsterAI.m_consumeSearchInterval;
    }

    private static void SaveAnimalAIData(ref MonsterData data, AnimalAI animalAI)
    {
        data.ViewRange = animalAI.m_viewRange;
        data.ViewAngle = animalAI.m_viewAngle;
        data.HearRange = animalAI.m_hearRange;
        data.MistVision = animalAI.m_mistVision;
        data.AlertedEffects = FormatEffectData(animalAI.m_alertedEffects);
        data.IdleSound = FormatEffectData(animalAI.m_idleSound);
        data.IdleSoundInterval = animalAI.m_idleSoundInterval;
        data.IdleSoundChance = animalAI.m_idleSoundChance;
        data.PathAgentType = animalAI.m_pathAgentType.ToString();
        data.MoveMinimumAngle = animalAI.m_moveMinAngle;
        data.smoothMovement = animalAI.m_smoothMovement;
        data.SerpentMovement = animalAI.m_serpentMovement;
        data.SerpentTurnRadius = animalAI.m_serpentTurnRadius;
        data.JumpInterval = animalAI.m_jumpInterval;
        // Random Circle
        data.RandomCircleInterval = animalAI.m_randomCircleInterval;
        // Random Movement
        data.RandomMoveInterval = animalAI.m_randomMoveInterval;
        data.RandomMoveRange = animalAI.m_randomMoveRange;
        // Fly Behavior
        data.RandomFly = animalAI.m_randomFly;
        data.ChanceToTakeOff = animalAI.m_chanceToTakeoff;
        data.ChanceToLand = animalAI.m_chanceToLand;
        data.GroundDuration = animalAI.m_groundDuration;
        data.AirDuration = animalAI.m_airDuration;
        data.MaxLandAltitude = animalAI.m_maxLandAltitude;
        data.TakeOffTime = animalAI.m_takeoffTime;
        data.FlyAltitudeMinimum = animalAI.m_flyAltitudeMin;
        data.FlyAltitudeMaximum = animalAI.m_flyAltitudeMax;
        data.FlyAbsoluteMinimumAltitude = animalAI.m_flyAbsMinAltitude;
        // Other
        data.AvoidFire = animalAI.m_avoidFire;
        data.AfraidOfFire = animalAI.m_afraidOfFire;
        data.AvoidWater = animalAI.m_avoidWater;
        data.Aggravatable = animalAI.m_aggravatable;
        data.PassiveAggressive = animalAI.m_passiveAggresive;
        data.SpawnMessage = animalAI.m_spawnMessage;
        data.DeathMessage = animalAI.m_deathMessage;
        data.AlertedMessage = animalAI.m_alertedMessage;
    }

    private static void SaveProcreationData(ref MonsterData data, Procreation procreation)
    {
        data.CHANGE_PROCREATION = true;
        data.UpdateInterval = procreation.m_updateInterval;
        data.TotalCheckRange = procreation.m_totalCheckRange;
        data.MaxCreatures = procreation.m_maxCreatures;
        data.PartnerCheckRange = procreation.m_partnerCheckRange;
        data.PregnancyChance = procreation.m_pregnancyChance;
        data.PregnancyDuration = procreation.m_pregnancyDuration;
        data.RequiredLovePoints = procreation.m_requiredLovePoints;
        data.OffSpring = procreation.m_offspring ? procreation.m_offspring.name : "";
        data.MinimumOffspringLevel = procreation.m_minOffspringLevel;
        data.SpawnOffset = procreation.m_spawnOffset;
        data.BirthEffects = FormatEffectData(procreation.m_birthEffects);
        data.LoveEffects = FormatEffectData(procreation.m_loveEffects);
    }

    private static void SaveTameData(ref MonsterData data, Tameable tameable)
    {
        data.CHANGE_TAMEABLE = true;
        data.FedDuration = tameable.m_fedDuration;
        data.TamingTime = tameable.m_tamingTime;
        data.StartsTamed = tameable.m_startsTamed;
        data.TamedEffects = FormatEffectData(tameable.m_tamedEffect);
        data.SootheEffects = FormatEffectData(tameable.m_sootheEffect);
        data.PetEffects = FormatEffectData(tameable.m_petEffect);
        data.Commandable = tameable.m_commandable;
        data.UnSummonDistance = tameable.m_unsummonDistance;
        data.UnSummonOnOwnerLogoutSeconds = tameable.m_unsummonOnOwnerLogoutSeconds;
        data.UnSummonEffects = FormatEffectData(tameable.m_unSummonEffect);
        data.LevelUpOwnerSkill = tameable.m_levelUpOwnerSkill.ToString();
        data.LevelUpFactor = tameable.m_levelUpFactor;
        data.SaddleItem = tameable.m_saddleItem ? tameable.m_saddleItem.name : "";
        data.DropSaddleOnDeath = tameable.m_dropSaddleOnDeath;
        data.DropItemVelocity = tameable.m_dropItemVel;
        data.RandomStartingName = tameable.m_randomStartingName;
    }

    private static void SaveMaterialData(ref MonsterData data, GameObject critter)
    {
        SkinnedMeshRenderer renderer = critter.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!renderer) return;
        foreach (var material in renderer.materials)
        {
            MaterialData materialData = new MaterialData
            {
                Shader = material.shader.name,
                MaterialName = material.name.Replace("(Instance)",string.Empty).TrimEnd(),
            };
            if (material.mainTexture)
            {
                materialData.MainTexture = material.mainTexture.name;
            }
            if (material.HasColor("_Color")) materialData.MainColor = new()
                { Red = material.color.r, Green = material.color.g, Blue = material.color.b, Alpha = material.color.a };
            if (material.HasFloat(Hue)) materialData.Hue = material.GetFloat(Hue);
            if (material.HasFloat(Saturation)) materialData.Saturation = material.GetFloat(Saturation);
            if (material.HasFloat(Value)) materialData.Value = material.GetFloat(Value);
            if (material.HasFloat(Cutoff)) materialData.AlphaCutoff = material.GetFloat(Cutoff);
            if (material.HasFloat(Smoothness)) materialData.Smoothness = material.GetFloat(Smoothness);
            if (material.HasFloat(UseGlossMap)) materialData.UseGlossMap = material.GetFloat(UseGlossMap) > 0f;
            if (material.HasTexture(MetallicGlossMap)) materialData.GlossTexture = material.GetTexture(MetallicGlossMap) ? material.GetTexture(MetallicGlossMap).name : "";
            if (material.HasFloat(Metallic)) materialData.Metallic = material.GetFloat(Metallic);
            if (material.HasFloat(MetalGloss)) materialData.MetalGloss = material.GetFloat(MetalGloss);
            if (material.HasColor(MetalColor))
            {
                Color metalColor = material.GetColor(MetalColor);
                materialData.MetalColor = new()
                    { Red = metalColor.r, Green = metalColor.g, Blue = metalColor.b, Alpha = metalColor.a };
            }
            if (material.HasTexture(EmissionMap)) materialData.EmissionTexture = material.GetTexture(EmissionMap) ? material.GetTexture(EmissionMap).name : "";
            if (material.HasColor(EmissionColor))
            {
                Color emissionColor = material.GetColor(EmissionColor);
                materialData.EmissionColor = new ColorData() 
                    {Red = emissionColor.r, Green = emissionColor.g, Blue = emissionColor.b, Alpha = emissionColor.a};
            }
            if (material.HasFloat(NormalStrength)) materialData.BumpStrength = material.GetFloat(NormalStrength);
            if (material.HasTexture(BumpMap)) materialData.BumpTexture = material.GetTexture(BumpMap) ? material.GetTexture(BumpMap).name : "";
            if (material.HasFloat(TwoSidedNormals)) materialData.TwoSidedNormals = material.GetFloat(TwoSidedNormals) > 0f;
            if (material.HasFloat(UseStyles)) materialData.UseStyles = material.GetFloat(UseStyles) > 0f;
            if (material.HasFloat(Style)) materialData.Style = material.GetFloat(Style);
            if (material.HasTexture(StyleTex)) materialData.StyleTexture = material.GetTexture(StyleTex) ? material.GetTexture(StyleTex).name : "";
            if (material.HasFloat(AddRain)) materialData.AddRain = material.GetFloat(AddRain) > 0f;
            data.Materials.Add(materialData);
        }
    }
    
    private static bool GetMonsterData(string creatureName, out MonsterData data)
    {
        data = new MonsterData();
        if (!ZNetScene.instance) return false;
        GameObject critter = ZNetScene.instance.GetPrefab(creatureName);
        if (!critter)
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find " + creatureName);
            return false;
        }
        Vector3 localScale = critter.transform.localScale;
        data.Scale = new Scale(){x = localScale.x, y = localScale.y, z = localScale.z};

        if (critter.TryGetComponent(out CapsuleCollider collider))
        {
            data.ColliderHeight = collider.height;
            var center = collider.center;
            data.ColliderCenter = new Scale()
                { x = center.x, y = center.y, z = center.z };
        }
        
        data.PrefabName = creatureName;
        if (critter.TryGetComponent(out Humanoid humanoid)) SaveHumanoidData(ref data, humanoid);
        else if (critter.TryGetComponent(out Character character)) SaveCharacterData(ref data, character);
        else return false;
        if (critter.TryGetComponent(out MonsterAI monsterAI)) SaveMonsterAIData(ref data, monsterAI);
        else if (critter.TryGetComponent(out AnimalAI animalAI)) SaveAnimalAIData(ref data, animalAI);
        else return false;
        if (critter.TryGetComponent(out CharacterDrop characterDrop))
        {
            data.Drops = FormatDropData(characterDrop.m_drops);
        }
        if (critter.TryGetComponent(out Procreation procreation)) SaveProcreationData(ref data, procreation);
        if (critter.TryGetComponent(out Tameable tameable)) SaveTameData(ref data, tameable);
        SaveMaterialData(ref data, critter);
        // SaveMonsterTexture(critter);
        
        if (!m_defaultMonsterData.ContainsKey(creatureName))
        {
            m_defaultMonsterData[creatureName] = data;
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Saved default data for " + data.PrefabName);
        }

        return true;
    }

    private static void WriteDataToDisk(string creatureName, MonsterData data)
    {
        string filePath = Paths.MonsterPath + Path.DirectorySeparatorChar + creatureName + ".yml";
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
    }
    
    public static bool Command_SaveMonsterData(string creatureName)
    {
        if (!GetMonsterData(creatureName, out MonsterData data)) return false;
        WriteDataToDisk(creatureName, data);
        return true;
    }

    private static List<DropData> FormatDropData(List<CharacterDrop.Drop> data)
    {
        return data.Select(drop => new DropData()
            {
                Prefab = drop.m_prefab.name,
                AmountMinimum = drop.m_amountMin,
                AmountMaximum = drop.m_amountMax,
                Chance = drop.m_chance,
                OnePerPlayer = drop.m_onePerPlayer,
                LevelMultiplier = drop.m_levelMultiplier,
                DoNotScale = drop.m_dontScale
            })
            .ToList();
    }

    private static List<RandomSet> FormatRandomSets(Humanoid.ItemSet[] data)
    {
        return data.Select(set => new RandomSet()
        {
            Name = set.m_name, 
            Items = FormatItemList(set.m_items)
        }).ToList();
    }

    private static List<CreatureItem> FormatItemList(GameObject[] data)
    {
        List<CreatureItem> output = new();
        foreach (GameObject prefab in data)
        {
            if (prefab == null) continue;
            if (!prefab.TryGetComponent(out ItemDrop component)) continue;
            CreatureItem itemData = FormatAttack(component);
            output.Add(itemData);
        }

        return output;
    }

    public static CreatureItem FormatAttack(ItemDrop component)
    {
        Attack attack = component.m_itemData.m_shared.m_attack;
        CreatureItem itemData = new CreatureItem()
        {
            Name = component.name,
            AttackAnimation = attack.m_attackAnimation,
            AttackOrigin = attack.m_attackOriginJoint,
            HitTerrain = attack.m_hitTerrain,
            HitFriendly = attack.m_hitFriendly,
            AttackRange = component.m_itemData.m_shared.m_aiAttackRange,
            AttackRangeMinimum = component.m_itemData.m_shared.m_aiAttackRangeMin,
            AttackInterval = component.m_itemData.m_shared.m_aiAttackInterval,
            AttackMaxAngle = component.m_itemData.m_shared.m_aiAttackMaxAngle,
            AttackDamages = FormatDamages(component.m_itemData.m_shared.m_damages),
            ToolTier = component.m_itemData.m_shared.m_toolTier,
            AttackForce = component.m_itemData.m_shared.m_attackForce,
            Dodgeable = component.m_itemData.m_shared.m_dodgeable,
            Blockable = component.m_itemData.m_shared.m_blockable,
            HitThroughWalls = component.m_itemData.m_shared.m_attack.m_hitThroughWalls,
            HitEffects = FormatEffectData(attack.m_hitEffect),
            HitTerrainEffects = FormatEffectData(attack.m_hitTerrainEffect),
            StartEffects = FormatEffectData(attack.m_startEffect),
            TriggerEffects = FormatEffectData(attack.m_triggerEffect),
            TrailStartEffects = FormatEffectData(attack.m_trailStartEffect),
            BurstEffects = FormatEffectData(attack.m_burstEffect)
        };
        if (component.m_itemData.m_shared.m_spawnOnHit)
        {
            itemData.SpawnOnHit = component.m_itemData.m_shared.m_spawnOnHit.name;
        }

        if (component.m_itemData.m_shared.m_spawnOnHitTerrain)
        {
            itemData.SpawnOnHitTerrain = component.m_itemData.m_shared.m_spawnOnHitTerrain.name;
        }

        if (component.m_itemData.m_shared.m_attackStatusEffect)
        {
            itemData.AttackStatusEffect = component.m_itemData.m_shared.m_attackStatusEffect.name;
        }

        if (component.m_itemData.m_shared.m_attack.m_attackProjectile != null)
        {
            GameObject projectilePrefab = component.m_itemData.m_shared.m_attack.m_attackProjectile;
            if (projectilePrefab.TryGetComponent(out Projectile projectile))
            {
                itemData.Projectile_PrefabName = projectile.name;
                itemData.Projectile_Damages = FormatDamages(projectile.m_damage);
                itemData.Projectile_AOE = projectile.m_aoe;
                itemData.Projectile_Dodgeable = projectile.m_dodgeable;
                itemData.Projectile_Blockable = projectile.m_blockable;
                itemData.Projectile_AttackForce = projectile.m_attackForce;
                itemData.Projectile_StatusEffect = projectile.m_statusEffect;
                itemData.Projectile_CanHitWater = projectile.m_canHitWater;
                itemData.Projectile_SpawnOnHit = projectile.m_spawnOnHit == null ? "" : projectile.m_spawnOnHit.name;
                itemData.Projectile_SpawnChance = projectile.m_spawnOnHitChance;
                itemData.Projectile_RandomSpawnOnHit = projectile.m_randomSpawnOnHit.Where(x => x != null).Select(x => x.name).ToList();
                itemData.Projectile_StaticHitOnly = projectile.m_staticHitOnly;
                itemData.Projectile_GroundHitOnly = projectile.m_groundHitOnly;
                itemData.Projectile_HitEffects = FormatEffectData(projectile.m_hitEffects);
                itemData.Projectile_HitWaterEffects = FormatEffectData(projectile.m_hitWaterEffects);
                itemData.Projectile_SpawnOnHitEffects = FormatEffectData(projectile.m_spawnOnHitEffects);
            }
        }

        return itemData;
    }

    private static AttackDamages FormatDamages(HitData.DamageTypes input)
    {
        return new AttackDamages
        {
            Damage = input.m_damage,
            Blunt = input.m_blunt,
            Slash = input.m_slash,
            Pierce = input.m_pierce,
            Chop = input.m_chop,
            Pickaxe = input.m_pickaxe,
            Fire = input.m_fire,
            Frost = input.m_frost,
            Lightning = input.m_lightning,
            Poison = input.m_poison,
            Spirit = input.m_spirit
        };
    }

    private static HitData.DamageTypes GetDamages(AttackDamages input)
    {
        return new()
        {
            m_damage = input.Damage,
            m_blunt = input.Blunt,
            m_slash = input.Slash,
            m_pierce = input.Pierce,
            m_chop = input.Chop,
            m_pickaxe = input.Pickaxe,
            m_fire = input.Fire,
            m_frost = input.Frost,
            m_lightning = input.Lightning,
            m_poison = input.Poison,
            m_spirit = input.Spirit
        };
    }
    
    private static List<string> FormatItemList(List<ItemDrop> data)
    {
        return (from item in data where item != null select item.name).ToList();
    }

    private static List<EffectPrefab> FormatEffectData(EffectList data)
    {
        return data.m_effectPrefabs.Select(effect => new EffectPrefab()
            {
                Prefab = effect.m_prefab.name,
                Enabled = effect.m_enabled,
                Variant = effect.m_variant,
                Attach = effect.m_attach,
                Follow = effect.m_follow,
                InheritParentRotation = effect.m_inheritParentRotation,
                InheritParentScale = effect.m_inheritParentScale,
                RandomRotation = effect.m_randomRotation,
                Scale = effect.m_scale
            })
            .ToList();
    }

    private static DamageModifiersData FormatDamageModifiers(HitData.DamageModifiers data)
    {
        return new DamageModifiersData
        {
            Blunt = data.m_blunt.ToString(),
            Slash = data.m_slash.ToString(),
            Pierce = data.m_pierce.ToString(),
            Chop = data.m_chop.ToString(),
            Pickaxe = data.m_pickaxe.ToString(),
            Fire = data.m_fire.ToString(),
            Frost = data.m_frost.ToString(),
            Lightning = data.m_lightning.ToString(),
            Poison = data.m_poison.ToString(),
            Spirit = data.m_spirit.ToString()
        };
    }

}