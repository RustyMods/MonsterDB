using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using MonsterDB.Behaviors;
using UnityEngine;
using YamlDotNet.Serialization;

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

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class Initiate_MonsterDB
    {
        private static void Postfix()
        {
            RegisterMonsterTweaks();
        }
    }

    private static void RegisterMonsterTweaks()
    {
        if (!ZNet.instance.IsServer()) return;
        Paths.CreateDirectories();
        string[] monsterFiles = Directory.GetFiles(Paths.MonsterPath);
        var deserializer = new DeserializerBuilder().Build();
        foreach (var file in monsterFiles)
        {
            try
            {
                var serial = File.ReadAllText(file);
                var data = deserializer.Deserialize<MonsterData>(serial);

                if (data.isClone)
                {
                    if (CloneMonster(data.OriginalMonster, data.Character.PrefabName, false))
                    {
                        MonsterDBPlugin.MonsterDBLogger.LogInfo("Successfully created " + data.Character.PrefabName);
                    }
                }
                else
                {
                    if (!GetMonsterData(data.Character.PrefabName, out MonsterData monsterData))
                    {
                        MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to get default data for " +
                                                                data.Character.PrefabName);
                        continue;
                    }

                    if (UpdateMonsterData(data))
                    {
                        MonsterDBPlugin.MonsterDBLogger.LogInfo("Updated " + data.Character.PrefabName);
                    }
                }

                ServerSync.m_serverData[data.Character.PrefabName] = data;
            }
            catch
            {
                MonsterDBPlugin.MonsterDBLogger.LogWarning("Failed to deserialize " + file);
            }
        }
        ServerSync.UpdateServerMonsterDB();
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

    public static bool CloneMonster(string creatureName, string cloneName, bool writeToDisk = true)
    {
        if (!ZNetScene.instance) return false;
        GameObject critter = ZNetScene.instance.GetPrefab(creatureName);
        if (!critter)
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find " + creatureName);
            return false;
        }
        GameObject clone = UnityEngine.Object.Instantiate(critter, MonsterDBPlugin._Root.transform, false);
        clone.name = cloneName;
        SkinnedMeshRenderer renderer = clone.GetComponentInChildren<SkinnedMeshRenderer>();
        List<Material> newMaterials = new();
        for (var index = 0; index < renderer.sharedMaterials.Length; index++)
        {
            var material = renderer.sharedMaterials[index];
            Material cloneMat = new Material(material);
            cloneMat.name = cloneName + "_mat_" + index;
            newMaterials.Add(cloneMat);
        }
        renderer.sharedMaterials = newMaterials.ToArray();
        renderer.materials = newMaterials.ToArray();

        if (!RegisterToZNetScene(clone))
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug($"Failed to register {clone.name} to ZNetScene");
            return false;
        }
        if (!GetMonsterData(cloneName, out MonsterData data)) return false;
        data.OriginalMonster = creatureName;
        data.Character.PrefabName = cloneName;
        data.Character.Name = cloneName;
        data.isClone = true;
        data.Character.Enabled = true;
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
            var deserializer = new DeserializerBuilder().Build();
            var serial = File.ReadAllText(filePath);
            var data = deserializer.Deserialize<MonsterData>(serial);

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
                ServerSync.m_serverData[data.Character.PrefabName] = data;
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
        if (m_defaultMonsterData.TryGetValue(creatureName, out MonsterData data)) return UpdateMonsterData(data, true);
        MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find default data for " + creatureName);
        return false;
    }

    private static void UpdateMonsterScale(MonsterData data, GameObject critter, bool reset)
    {
        if (!data.Character.ChangeScale && !reset) return;
        critter.transform.localScale =
            new Vector3(data.Character.Scale.x, data.Character.Scale.y, data.Character.Scale.z);
            
        if (critter.TryGetComponent(out CapsuleCollider collider))
        {
            collider.height = data.Character.ColliderHeight;
            collider.center = new Vector3(data.Character.ColliderCenter.x, data.Character.ColliderCenter.y, data.Character.ColliderCenter.z);
        }

        if (critter.TryGetComponent(out Humanoid humanoid))
        {
            foreach (var effect in humanoid.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab.GetComponent<Ragdoll>())
                {
                    effect.m_prefab.transform.localScale = new Vector3(data.Character.Scale.x, data.Character.Scale.y, data.Character.Scale.z);
                }
            }
        }
        else if (critter.TryGetComponent(out Character character))
        {
            foreach (var effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab.GetComponent<Ragdoll>())
                {
                    effect.m_prefab.transform.localScale = new Vector3(data.Character.Scale.x, data.Character.Scale.y, data.Character.Scale.z);
                }
            }
        }
    }

    private static void UpdateMonsterHumanoid(MonsterData data, Humanoid humanoid, bool reset)
    {
        humanoid.m_name = data.Character.Name;
        humanoid.m_group = data.Character.Group;
        if (GetFaction(data.Character.Faction, out Character.Faction faction)) humanoid.m_faction = faction;
        else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update faction for " + data.Character.PrefabName);
        humanoid.m_boss = data.Character.Boss;
        humanoid.m_dontHideBossHud = data.Character.DoNotHideBossHUD;
        humanoid.m_bossEvent = data.Character.BossEvent;
        humanoid.m_defeatSetGlobalKey = data.Character.DefeatSetGlobalKey;

        if (data.Character.ChangeMovement || reset)
        {
            humanoid.m_crouchSpeed = data.Character.CrouchSpeed;
            humanoid.m_walkSpeed = data.Character.WalkSpeed;
            humanoid.m_speed = data.Character.Speed;
            humanoid.m_turnSpeed = data.Character.TurnSpeed;
            humanoid.m_runSpeed = data.Character.RunSpeed;
            humanoid.m_runTurnSpeed = data.Character.RunTurnSpeed;
            humanoid.m_flySlowSpeed = data.Character.FlySlowSpeed;
            humanoid.m_flyFastSpeed = data.Character.FlyFastSpeed;
            humanoid.m_flyTurnSpeed = data.Character.FlyTurnSpeed;
            humanoid.m_acceleration = data.Character.Acceleration;
            humanoid.m_jumpForce = data.Character.JumpForce;
            humanoid.m_jumpForceForward = data.Character.JumpForceForward;
            humanoid.m_jumpForceTiredFactor = data.Character.JumpForceTiredFactor;
            humanoid.m_airControl = data.Character.AirControl;
            humanoid.m_canSwim = data.Character.CanSwim;
            humanoid.m_swimDepth = data.Character.SwimDepth;
            humanoid.m_swimSpeed = data.Character.SwimSpeed;
            humanoid.m_swimTurnSpeed = data.Character.SwimTurnSpeed;
            humanoid.m_swimAcceleration = data.Character.SwimAcceleration;
            if (GetGroundTilt(data.Character.GroundTilt, out Character.GroundTiltType type)) humanoid.m_groundTilt = type;
            else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update ground tilt type for " + data.Character.PrefabName);
            humanoid.m_groundTiltSpeed = data.Character.GroundTiltSpeed;
            humanoid.m_flying = data.Character.Flying;
            humanoid.m_jumpStaminaUsage = data.Character.JumpStaminaUsage;
            humanoid.m_disableWhileSleeping = data.Character.DisableWhileSleeping;
        }
        // Effects
        if (data.Character.ChangeEffects || reset)
        {
            if (GetEffects(data.Character.HitEffects, out EffectList HitEffects)) humanoid.m_hitEffects = HitEffects;
            if (GetEffects(data.Character.CriticalHitEffects, out EffectList CriticalHitEffects)) humanoid.m_critHitEffects = CriticalHitEffects;
            if (GetEffects(data.Character.BackStabHitEffects, out EffectList BackStabHitEffects)) humanoid.m_backstabHitEffects = BackStabHitEffects;
            if (GetEffects(data.Character.DeathEffects, out EffectList DeathEffects)) humanoid.m_deathEffects = DeathEffects;
            if (GetEffects(data.Character.WaterEffects, out EffectList WaterEffects)) humanoid.m_waterEffects = WaterEffects;
            if (GetEffects(data.Character.TarEffects, out EffectList TarEffects)) humanoid.m_tarEffects = TarEffects;
            if (GetEffects(data.Character.SlideEffects, out EffectList SlideEffects)) humanoid.m_slideEffects = SlideEffects;
            if (GetEffects(data.Character.JumpEffects, out EffectList JumpEffects)) humanoid.m_jumpEffects = JumpEffects;
            if (GetEffects(data.Character.FlyingContinuousEffects, out EffectList FlyingContinuousEffects)) humanoid.m_flyingContinuousEffect = FlyingContinuousEffects;
            if (GetEffects(data.Character.PickUpEffects, out EffectList PickUpEffects)) humanoid.m_pickupEffects = PickUpEffects;
            if (GetEffects(data.Character.DropEffects, out EffectList DropEffects)) humanoid.m_pickupEffects = DropEffects;
            if (GetEffects(data.Character.ConsumeItemEffects, out EffectList ConsumeItemEffects)) humanoid.m_pickupEffects = ConsumeItemEffects;
            if (GetEffects(data.Character.EquipEffects, out EffectList EquipEffects)) humanoid.m_pickupEffects = EquipEffects;
            if (GetEffects(data.Character.PerfectBlockEffect, out EffectList PerfectBlockEffect)) humanoid.m_pickupEffects = PerfectBlockEffect;
        }
        // Health & Damage
        if (data.Character.ChangeHealthDamage || reset)
        {
            humanoid.m_tolerateWater = data.Character.TolerateWater;
            humanoid.m_tolerateFire = data.Character.TolerateFire;
            humanoid.m_tolerateSmoke = data.Character.TolerateSmoke;
            humanoid.m_tolerateTar = data.Character.TolerateTar;
            humanoid.m_health = data.Character.Health;
            if (GetDamageMods(data.Character.DamageModifiers, out HitData.DamageModifiers damageModifiers)) humanoid.m_damageModifiers = damageModifiers;
            humanoid.m_staggerWhenBlocked = data.Character.StaggerWhenBlocked;
            humanoid.m_staggerDamageFactor = data.Character.StaggerDamageFactor;
        }

        if (data.Character.ChangeItems || reset)
        {
            if (GetItems(data.Character.DefaultItems, out List<GameObject> DefaultItems)) humanoid.m_defaultItems = DefaultItems.ToArray();
            if (GetItems(data.Character.RandomWeapons, out List<GameObject> RandomWeapons)) humanoid.m_randomWeapon = RandomWeapons.ToArray();
            if (GetItems(data.Character.RandomArmors, out List<GameObject> RandomArmors)) humanoid.m_randomArmor = RandomArmors.ToArray();
            if (GetItems(data.Character.RandomShields, out List<GameObject> RandomShields)) humanoid.m_randomShield = RandomShields.ToArray();
            if (GetRandomSets(data.Character.RandomSets, out List<Humanoid.ItemSet> RandomSets)) humanoid.m_randomSets = RandomSets.ToArray();
            if (DataBase.TryGetItemDrop(data.Character.UnarmedWeapon, out ItemDrop? UnarmedWeapon)) humanoid.m_unarmedWeapon = UnarmedWeapon;
            humanoid.m_beardItem = data.Character.BeardItem;
            humanoid.m_hairItem = data.Character.HairItem;
        }
    }

    private static void UpdateMonsterCharacter(MonsterData data, Character character, bool reset)
    {
        character.m_name = data.Character.Name;
        character.m_group = data.Character.Group;
        if (GetFaction(data.Character.Faction, out Character.Faction faction)) character.m_faction = faction;
        else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update faction for " + data.Character.PrefabName);
        character.m_boss = data.Character.Boss;
        character.m_dontHideBossHud = data.Character.DoNotHideBossHUD;
        character.m_bossEvent = data.Character.BossEvent;
        character.m_defeatSetGlobalKey = data.Character.DefeatSetGlobalKey;

        if (data.Character.ChangeMovement || reset)
        {
            character.m_crouchSpeed = data.Character.CrouchSpeed;
            character.m_walkSpeed = data.Character.WalkSpeed;
            character.m_speed = data.Character.Speed;
            character.m_turnSpeed = data.Character.TurnSpeed;
            character.m_runSpeed = data.Character.RunSpeed;
            character.m_runTurnSpeed = data.Character.RunTurnSpeed;
            character.m_flySlowSpeed = data.Character.FlySlowSpeed;
            character.m_flyFastSpeed = data.Character.FlyFastSpeed;
            character.m_flyTurnSpeed = data.Character.FlyTurnSpeed;
            character.m_acceleration = data.Character.Acceleration;
            character.m_jumpForce = data.Character.JumpForce;
            character.m_jumpForceForward = data.Character.JumpForceForward;
            character.m_jumpForceTiredFactor = data.Character.JumpForceTiredFactor;
            character.m_airControl = data.Character.AirControl;
            character.m_canSwim = data.Character.CanSwim;
            character.m_swimDepth = data.Character.SwimDepth;
            character.m_swimSpeed = data.Character.SwimSpeed;
            character.m_swimTurnSpeed = data.Character.SwimTurnSpeed;
            character.m_swimAcceleration = data.Character.SwimAcceleration;
            if (GetGroundTilt(data.Character.GroundTilt, out Character.GroundTiltType type)) character.m_groundTilt = type;
            character.m_groundTiltSpeed = data.Character.GroundTiltSpeed;
            character.m_flying = data.Character.Flying;
            character.m_jumpStaminaUsage = data.Character.JumpStaminaUsage;
            character.m_disableWhileSleeping = data.Character.DisableWhileSleeping;
        }
        // Effects
        if (data.Character.ChangeEffects || reset)
        {
            if (GetEffects(data.Character.HitEffects, out EffectList HitEffects)) character.m_hitEffects = HitEffects;
            if (GetEffects(data.Character.CriticalHitEffects, out EffectList CriticalHitEffects)) character.m_hitEffects = CriticalHitEffects;
            if (GetEffects(data.Character.BackStabHitEffects, out EffectList BackStabHitEffects)) character.m_hitEffects = BackStabHitEffects;
            if (GetEffects(data.Character.DeathEffects, out EffectList DeathEffects)) character.m_hitEffects = DeathEffects;
            if (GetEffects(data.Character.WaterEffects, out EffectList WaterEffects)) character.m_hitEffects = WaterEffects;
            if (GetEffects(data.Character.TarEffects, out EffectList TarEffects)) character.m_hitEffects = TarEffects;
            if (GetEffects(data.Character.SlideEffects, out EffectList SlideEffects)) character.m_hitEffects = SlideEffects;
            if (GetEffects(data.Character.JumpEffects, out EffectList JumpEffects)) character.m_hitEffects = JumpEffects;
            if (GetEffects(data.Character.FlyingContinuousEffects, out EffectList FlyingContinuousEffects)) character.m_hitEffects = FlyingContinuousEffects;
        }
        // Health & Damage
        if (data.Character.ChangeHealthDamage || reset)
        {
            character.m_tolerateWater = data.Character.TolerateWater;
            character.m_tolerateFire = data.Character.TolerateFire;
            character.m_tolerateSmoke = data.Character.TolerateSmoke;
            character.m_tolerateTar = data.Character.TolerateTar;
            character.m_health = data.Character.Health;
            if (GetDamageMods(data.Character.DamageModifiers, out HitData.DamageModifiers damageModifiers)) character.m_damageModifiers = damageModifiers;
            character.m_staggerWhenBlocked = data.Character.StaggerWhenBlocked;
            character.m_staggerDamageFactor = data.Character.StaggerDamageFactor;
        }
    }

    private static void UpdateMonsterAI(MonsterData data, MonsterAI monsterAI, bool reset)
    {
        monsterAI.m_viewRange = data.MonsterAI.ViewRange;
        monsterAI.m_viewAngle = data.MonsterAI.ViewAngle;
        monsterAI.m_hearRange = data.MonsterAI.HearRange;
        monsterAI.m_mistVision = data.MonsterAI.MistVision;
        if (data.MonsterAI.ChangeEffects || reset)
        {
            if (GetEffects(data.MonsterAI.AlertedEffects, out EffectList AlertedEffects))
                monsterAI.m_alertedEffects = AlertedEffects;
            if (GetEffects(data.MonsterAI.IdleSound, out EffectList IdleSounds))
                monsterAI.m_idleSound = IdleSounds;
            if (GetEffects(data.MonsterAI.WakeUpEffects, out EffectList WakeUpEffects))
                monsterAI.m_wakeupEffects = WakeUpEffects;
        }
        monsterAI.m_idleSoundInterval = data.MonsterAI.IdleSoundInterval;
        monsterAI.m_idleSoundChance = data.MonsterAI.IdleSoundChance;
        if (GetPathType(data.MonsterAI.PathAgentType, out Pathfinding.AgentType PathAgentType))
            monsterAI.m_pathAgentType = PathAgentType;
        monsterAI.m_moveMinAngle = data.MonsterAI.MoveMinimumAngle;
        monsterAI.m_smoothMovement = data.MonsterAI.smoothMovement;
        monsterAI.m_serpentMovement = data.MonsterAI.SerpentMovement;
        monsterAI.m_serpentTurnRadius = data.MonsterAI.SerpentTurnRadius;
        monsterAI.m_jumpInterval = data.MonsterAI.JumpInterval;
        // Random Circle
        monsterAI.m_randomCircleInterval = data.MonsterAI.RandomCircleInterval;
        // Random Movement
        monsterAI.m_randomMoveInterval = data.MonsterAI.RandomMoveInterval;
        monsterAI.m_randomMoveRange = data.MonsterAI.RandomMoveRange;
        // Fly Behavior
        monsterAI.m_randomFly = data.MonsterAI.RandomFly;
        monsterAI.m_chanceToTakeoff = data.MonsterAI.ChanceToTakeOff;
        monsterAI.m_chanceToLand = data.MonsterAI.ChanceToLand;
        monsterAI.m_groundDuration = data.MonsterAI.GroundDuration;
        monsterAI.m_airDuration = data.MonsterAI.AirDuration;
        monsterAI.m_maxLandAltitude = data.MonsterAI.MaxLandAltitude;
        monsterAI.m_takeoffTime = data.MonsterAI.TakeOffTime;
        monsterAI.m_flyAltitudeMin = data.MonsterAI.FlyAltitudeMinimum;
        monsterAI.m_flyAltitudeMax = data.MonsterAI.FlyAltitudeMaximum;
        monsterAI.m_flyAbsMinAltitude = data.MonsterAI.FlyAbsoluteMinimumAltitude;
        // Other
        monsterAI.m_avoidFire = data.MonsterAI.AvoidFire;
        monsterAI.m_afraidOfFire = data.MonsterAI.AfraidOfFire;
        monsterAI.m_avoidWater = data.MonsterAI.AvoidWater;
        monsterAI.m_aggravatable = data.MonsterAI.Aggravatable;
        monsterAI.m_passiveAggresive = data.MonsterAI.PassiveAggressive;
        monsterAI.m_spawnMessage = data.MonsterAI.SpawnMessage;
        monsterAI.m_deathMessage = data.MonsterAI.DeathMessage;
        monsterAI.m_alertedMessage = data.MonsterAI.AlertedMessage;
        // Monster AI
        monsterAI.m_alertRange = data.MonsterAI.AlertRange;
        monsterAI.m_fleeIfHurtWhenTargetCantBeReached = data.MonsterAI.FleeIfHurtWhenTargetCannotBeReached;
        monsterAI.m_fleeIfNotAlerted = data.MonsterAI.FleeIfNotAlerted;
        monsterAI.m_fleeIfLowHealth = data.MonsterAI.FleeIfLowHealth;
        monsterAI.m_circulateWhileCharging = data.MonsterAI.CirculateWhileCharging;
        monsterAI.m_circulateWhileChargingFlying = data.MonsterAI.CirculateWhileChargingFlying;
        monsterAI.m_enableHuntPlayer = data.MonsterAI.EnableHuntPlayer;
        monsterAI.m_attackPlayerObjects = data.MonsterAI.AttackPlayerObjects;
        monsterAI.m_privateAreaTriggerTreshold = data.MonsterAI.PrivateAreaTriggerThreshold;
        monsterAI.m_interceptTimeMax = data.MonsterAI.InterceptTimeMaximum;
        monsterAI.m_interceptTimeMin = data.MonsterAI.InterceptTimeMinimum;
        monsterAI.m_maxChaseDistance = data.MonsterAI.MaximumChaseDistance;
        monsterAI.m_minAttackInterval = data.MonsterAI.MinimumAttackInterval;
        // Circle Target
        monsterAI.m_circleTargetInterval = data.MonsterAI.CircleTargetInterval;
        monsterAI.m_circleTargetDuration = data.MonsterAI.CircleTargetDuration;
        monsterAI.m_circleTargetDistance = data.MonsterAI.CircleTargetDistance;
        // Sleep
        monsterAI.m_sleeping = data.MonsterAI.Sleeping;
        monsterAI.m_wakeupRange = data.MonsterAI.WakeUpRange;
        monsterAI.m_noiseWakeup = data.MonsterAI.NoiseWakeUp;
        monsterAI.m_maxNoiseWakeupRange = data.MonsterAI.MaximumNoiseWakeUpRange;

        monsterAI.m_wakeUpDelayMin = data.MonsterAI.WakeUpDelayMinimum;
        monsterAI.m_wakeUpDelayMax = data.MonsterAI.WakeUpDelayMaximum;
        // Other
        monsterAI.m_avoidLand = data.MonsterAI.AvoidLand;
        // Consume Items 
        if (data.MonsterAI.ChangeConsume || reset)
        {
            if (GetItemDrops(data.MonsterAI.ConsumeItems, out List<ItemDrop> ConsumeItems)) monsterAI.m_consumeItems = ConsumeItems;
            monsterAI.m_consumeRange = data.MonsterAI.ConsumeRange;
            monsterAI.m_consumeSearchRange = data.MonsterAI.ConsumeSearchRange;
            monsterAI.m_consumeSearchInterval = data.MonsterAI.ConsumeSearchInterval;
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Updated " + data.Character.PrefabName + " consume items");
        }
    }

    private static void UpdateAnimalAI(MonsterData data, AnimalAI animalAI, bool reset)
    {
        animalAI.m_viewRange = data.MonsterAI.ViewRange;
        animalAI.m_viewAngle = data.MonsterAI.ViewAngle;
        animalAI.m_hearRange = data.MonsterAI.HearRange;
        animalAI.m_mistVision = data.MonsterAI.MistVision;
        if (data.MonsterAI.ChangeEffects || reset)
        {
            if (GetEffects(data.MonsterAI.AlertedEffects, out EffectList AlertedEffects))
                animalAI.m_alertedEffects = AlertedEffects;
            if (GetEffects(data.MonsterAI.IdleSound, out EffectList IdleSounds))
                animalAI.m_idleSound = IdleSounds;
        }
        animalAI.m_idleSoundInterval = data.MonsterAI.IdleSoundInterval;
        animalAI.m_idleSoundChance = data.MonsterAI.IdleSoundChance;
        if (GetPathType(data.MonsterAI.PathAgentType, out Pathfinding.AgentType PathAgentType))
            animalAI.m_pathAgentType = PathAgentType;
        else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to update path type of " + data.Character.PrefabName);
        animalAI.m_moveMinAngle = data.MonsterAI.MoveMinimumAngle;
        animalAI.m_smoothMovement = data.MonsterAI.smoothMovement;
        animalAI.m_serpentMovement = data.MonsterAI.SerpentMovement;
        animalAI.m_serpentTurnRadius = data.MonsterAI.SerpentTurnRadius;
        animalAI.m_jumpInterval = data.MonsterAI.JumpInterval;
        // Random Circle
        animalAI.m_randomCircleInterval = data.MonsterAI.RandomCircleInterval;
        // Random Movement
        animalAI.m_randomMoveInterval = data.MonsterAI.RandomMoveInterval;
        animalAI.m_randomMoveRange = data.MonsterAI.RandomMoveRange;
        // Fly Behavior
        animalAI.m_randomFly = data.MonsterAI.RandomFly;
        animalAI.m_chanceToTakeoff = data.MonsterAI.ChanceToTakeOff;
        animalAI.m_chanceToLand = data.MonsterAI.ChanceToLand;
        animalAI.m_groundDuration = data.MonsterAI.GroundDuration;
        animalAI.m_airDuration = data.MonsterAI.AirDuration;
        animalAI.m_maxLandAltitude = data.MonsterAI.MaxLandAltitude;
        animalAI.m_takeoffTime = data.MonsterAI.TakeOffTime;
        animalAI.m_flyAltitudeMin = data.MonsterAI.FlyAltitudeMinimum;
        animalAI.m_flyAltitudeMax = data.MonsterAI.FlyAltitudeMaximum;
        animalAI.m_flyAbsMinAltitude = data.MonsterAI.FlyAbsoluteMinimumAltitude;
        // Other
        animalAI.m_avoidFire = data.MonsterAI.AvoidFire;
        animalAI.m_afraidOfFire = data.MonsterAI.AfraidOfFire;
        animalAI.m_avoidWater = data.MonsterAI.AvoidWater;
        animalAI.m_aggravatable = data.MonsterAI.Aggravatable;
        animalAI.m_passiveAggresive = data.MonsterAI.PassiveAggressive;
        animalAI.m_spawnMessage = data.MonsterAI.SpawnMessage;
        animalAI.m_deathMessage = data.MonsterAI.DeathMessage;
        animalAI.m_alertedMessage = data.MonsterAI.AlertedMessage;
        animalAI.m_timeToSafe = data.MonsterAI.TimeToSafe;
    }

    private static void UpdateMonsterProcreation(MonsterData data, GameObject critter, bool reset)
    {
        if (critter.TryGetComponent(out Procreation procreation) && (data.Procreation.Enabled || reset))
        {
            procreation.m_updateInterval = data.Procreation.UpdateInterval;
            procreation.m_totalCheckRange = data.Procreation.TotalCheckRange;
            procreation.m_maxCreatures = data.Procreation.MaxCreatures;
            procreation.m_partnerCheckRange = data.Procreation.PartnerCheckRange;
            procreation.m_pregnancyChance = data.Procreation.PregnancyChance;
            procreation.m_pregnancyDuration = data.Procreation.PregnancyDuration;
            procreation.m_requiredLovePoints = data.Procreation.RequiredLovePoints;
            if (data.Procreation.ChangeOffSpring)
            {
                if (GetOffspring(data.Procreation.OffSpring, out GameObject Offspring)) procreation.m_offspring = Offspring;
                else MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to change offspring of " + critter.name);
            }
            procreation.m_minOffspringLevel = data.Procreation.MinimumOffspringLevel;
            procreation.m_spawnOffset = data.Procreation.SpawnOffset;

            if (data.Procreation.ChangeEffects)
            {
                if (GetEffects(data.Procreation.BirthEffects, out EffectList BirthEffects))
                    procreation.m_birthEffects = BirthEffects;
                if (GetEffects(data.Procreation.LoveEffects, out EffectList LoveEffects))
                    procreation.m_loveEffects = LoveEffects;
            }
        }
    }

    private static void UpdateMonsterTame(MonsterData data, GameObject critter, bool reset)
    {
        if (reset)
        {
            if (m_newTames.Contains(data.Character.PrefabName))
            {
                if (critter.TryGetComponent(out Tameable tameable))
                {
                    UnityEngine.Object.Destroy(tameable);
                    m_newTames.Remove(data.Character.PrefabName);
                }
            }
        }
        else if (data.Tameable.Enabled || reset)
        {
            if (!critter.TryGetComponent(out Tameable tameable))
            {
                if (data.Tameable.MakeTameable)
                {
                    tameable = critter.AddComponent<Tameable>();
                    m_newTames.Add(data.Character.PrefabName);
                }
            }

            if (tameable != null)
            {
                tameable.m_fedDuration = data.Tameable.FedDuration;
                tameable.m_tamingTime = data.Tameable.TamingTime;
                tameable.m_startsTamed = data.Tameable.StartsTamed;

                if (data.Tameable.ChangeEffects)
                {
                    if (GetEffects(data.Tameable.TamedEffects, out EffectList TameEffects))
                        tameable.m_tamedEffect = TameEffects;
                    if (GetEffects(data.Tameable.SootheEffects, out EffectList SoothEffects))
                        tameable.m_sootheEffect = SoothEffects;
                    if (GetEffects(data.Tameable.PetEffects, out EffectList PetEffects)) tameable.m_petEffect = PetEffects;
                    if (GetEffects(data.Tameable.UnSummonEffects, out EffectList UnSummonEffects))
                        tameable.m_unSummonEffect = UnSummonEffects;
                }
                tameable.m_commandable = data.Tameable.Commandable;
                tameable.m_unsummonDistance = data.Tameable.UnSummonDistance;
                tameable.m_unsummonOnOwnerLogoutSeconds = data.Tameable.UnSummonOnOwnerLogoutSeconds;
                if (GetSkill(data.Tameable.LevelUpOwnerSkill, out Skills.SkillType skillType))
                    tameable.m_levelUpOwnerSkill = skillType;
                tameable.m_levelUpFactor = data.Tameable.LevelUpFactor;
                if (GetSaddle(data.Tameable.SaddleItem, out ItemDrop Saddle)) tameable.m_saddleItem = Saddle;
                tameable.m_dropSaddleOnDeath = data.Tameable.DropSaddleOnDeath;
                tameable.m_dropItemVel = data.Tameable.DropItemVelocity;
                tameable.m_randomStartingName = data.Tameable.RandomStartingName;
            }
        }
    }

    private static void ManipulateMaterial(MaterialData materialData, Material material)
    {
        if (GetTexture2D(materialData.MainTexture, out Texture2D? MainTexture))
        {
            material.mainTexture = MainTexture;
        }
        material.color = new Color(materialData.MainColor.Red, materialData.MainColor.Green, materialData.MainColor.Blue, materialData.MainColor.Alpha);
        if (material.HasFloat(Hue)) material.SetFloat(Hue, materialData.Hue);
        if (material.HasFloat(Saturation)) material.SetFloat(Saturation, materialData.Saturation);
        if (material.HasFloat(Value)) material.SetFloat(Value, materialData.Value);
        if (material.HasFloat(Cutoff)) material.SetFloat(Cutoff, materialData.AlphaCutoff);
        if (material.HasFloat(Smoothness)) material.SetFloat(Smoothness, materialData.Smoothness);
        if (material.HasFloat(UseGlossMap)) material.SetFloat(UseGlossMap, materialData.UseGlossMap ? 1f : 0f);
        if (material.HasFloat(Metallic)) material.SetFloat(Metallic, materialData.Metallic);
        if (material.HasTexture(MetallicGlossMap))
        {
            if (GetTexture2D(materialData.GlossTexture, out Texture2D? GlossTexture))
            {
                material.SetTexture(MetallicGlossMap, GlossTexture);
            }
        }
        if (material.HasFloat(Metallic)) material.SetFloat(Metallic, materialData.Metallic);
        if (material.HasFloat(MetalGloss)) material.SetFloat(MetalGloss, materialData.MetalGloss);
        if (material.HasColor(MetalColor)) material.SetColor(MetalColor, new Color(materialData.MetalColor.Red, materialData.MetalColor.Green, materialData.MetalColor.Blue, materialData.MetalColor.Alpha));
        if (material.HasTexture(EmissionMap))
        {
            if (GetTexture2D(materialData.EmissionTexture, out Texture2D? EmissionTexture))
            {
                material.SetTexture(EmissionMap, EmissionTexture);
            }
        }
        if (material.HasColor(EmissionColor)) material.SetColor(EmissionColor, new Color(materialData.EmissionColor.Red, materialData.EmissionColor.Green, materialData.EmissionColor.Blue, materialData.EmissionColor.Alpha));
        if (material.HasFloat(NormalStrength)) material.SetFloat(NormalStrength, materialData.BumpStrength);

        if (material.HasTexture(BumpMap))
        {
            if (GetTexture2D(materialData.BumpTexture, out Texture2D? BumpTexture))
            {
                material.SetTexture(BumpMap, BumpTexture);
            }
        }
        if (material.HasFloat(TwoSidedNormals)) material.SetFloat(TwoSidedNormals, materialData.TwoSidedNormals ? 1f : 0f);
        if (material.HasFloat(UseStyles)) material.SetFloat(UseStyles, materialData.UseStyles ? 1f: 0f);
        if (material.HasFloat(Style)) material.SetFloat(Style, materialData.Style);
        if (material.HasTexture(StyleTex))
        {
            if (GetTexture2D(materialData.StyleTexture, out Texture2D? StyleTexture))
            {
                material.SetTexture(StyleTex, StyleTexture);

            }
        }
        if (material.HasFloat(AddRain)) material.SetFloat(AddRain, materialData.AddRain ? 1f: 0f); 
    }

    private static void UpdateMonsterMaterial(MonsterData data, GameObject critter, bool reset)
    {
        SkinnedMeshRenderer renderer = critter.GetComponentInChildren<SkinnedMeshRenderer>();

        foreach (var materialData in data.Materials)
        {
            if (!materialData.Enabled && !reset) continue;
            if (!renderer) continue;
            Material material = renderer.materials.ToList().Find(x => x.name.Replace("(Instance)",string.Empty).TrimEnd() == materialData.MaterialName);
            if (material != null)
            {
                ManipulateMaterial(materialData, material);
            }
        }

        if (critter.TryGetComponent(out Humanoid humanoid))
        {
            foreach (var effect in humanoid.m_deathEffects.m_effectPrefabs)
            {
                if (!effect.m_prefab.TryGetComponent(out Ragdoll ragdoll)) continue;
                foreach (var materialData in data.Materials)
                {
                    if (!materialData.Enabled && !reset) continue;
                    Material material = ragdoll.m_mainModel.materials.ToList().Find(x =>
                        x.name.Replace("(Instance)", string.Empty).TrimEnd() == materialData.MaterialName);
                    if (material != null)
                    {
                        ManipulateMaterial(materialData, material);
                    }
                }
            }
        }
        else if (critter.TryGetComponent(out Character character))
        {
            foreach (var effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (!effect.m_prefab.TryGetComponent(out Ragdoll ragdoll)) continue;
                foreach (var materialData in data.Materials)
                {
                    if (!materialData.Enabled && !reset) continue;
                    Material material = ragdoll.m_mainModel.materials.ToList().Find(x =>
                        x.name.Replace("(Instance)", string.Empty).TrimEnd() == materialData.MaterialName);
                    if (material != null)
                    {
                        ManipulateMaterial(materialData, material);
                    }
                }
            }
        }
    }

    private static void UpdateMonsterModel(MonsterData data, GameObject critter, bool reset)
    {
        if (data.Character.FemaleModel)
        {
            if (critter.TryGetComponent(out VisEquipment visEquipment))
            {
                visEquipment.SetModel(1);
            }
        }
    }

    private static bool UpdateMonsterData(MonsterData data, bool reset = false)
    {
        if (!ZNetScene.instance) return false;
        GameObject critter = ZNetScene.instance.GetPrefab(data.Character.PrefabName);
        if (!critter)
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find " + data.Character.PrefabName);
            return false;
        }
        
        UpdateMonsterScale(data, critter, reset);

        if (critter.TryGetComponent(out Humanoid humanoid) && (data.Character.Enabled || reset))
        {
            UpdateMonsterHumanoid(data, humanoid, reset);
        } 
        else if (critter.TryGetComponent(out Character character) && (data.Character.Enabled || reset))
        {
            UpdateMonsterCharacter(data, character, reset);
        }

        if (critter.TryGetComponent(out MonsterAI monsterAI) && (data.MonsterAI.Enabled || reset))
        {
            UpdateMonsterAI(data, monsterAI, reset);
        } 
        else if (critter.TryGetComponent(out AnimalAI animalAI) && (data.MonsterAI.Enabled || reset))
        {
            UpdateAnimalAI(data, animalAI, reset);
        }

        if (critter.TryGetComponent(out CharacterDrop characterDrop) && (data.CharacterDrop.Enabled || reset))
        {
            if (GetDrops(data.CharacterDrop.Drops, out List<CharacterDrop.Drop> Drops)) characterDrop.m_drops = Drops;
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

        if (DataBase.m_textures.TryGetValue(input, out Texture2D texture))
        {
            output = texture;
            return true;
        }
        return false;
    }

    private static bool GetSaddle(string input, out ItemDrop output)
    {
        output = null!;
        GameObject? saddle = DataBase.TryGetGameObject(input);
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
            GameObject? prefab = DataBase.TryGetGameObject(data.Prefab);
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
        output = DataBase.TryGetGameObject(input)!;
        return output != null;
    }

    private static bool GetRandomSets(List<RandomSet> input, out List<Humanoid.ItemSet> output)
    {
        output = new();
        foreach (RandomSet data in input)
        {
            Humanoid.ItemSet set = new()
            {
                m_name = data.Name
            };
            if (GetItems(data.Items, out List<GameObject> items))
            {
                set.m_items = items.ToArray();
            }
            output.Add(set);
        }

        return output.Count > 0;
    }

    private static bool GetItemDrops(List<string> input, out List<ItemDrop> output)
    {
        output = new();
        foreach (var itemName in input)
        {
            GameObject? item = DataBase.TryGetGameObject(itemName);
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

    private static bool GetItems(List<CreatureItem> input, out List<GameObject> output)
    {
        output = new();
        foreach (var item in input)
        {
            GameObject? prefab = DataBase.TryGetGameObject(item.Name);
            if (prefab == null) continue;
            var clone = UnityEngine.Object.Instantiate(prefab, MonsterDBPlugin._Root.transform, false);
            if (!clone.TryGetComponent(out ItemDrop component)) continue;

            if (!item.AttackAnimation.IsNullOrWhiteSpace())
            {
                MonsterDBPlugin.MonsterDBLogger.LogDebug("Attack animation is null or empty, using default animation for " + prefab.name + " animation: " + component.m_itemData.m_shared.m_attack.m_attackAnimation);
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
            component.m_itemData.m_shared.m_spawnOnHit = DataBase.TryGetGameObject(item.SpawnOnHit);
            component.m_itemData.m_shared.m_spawnOnHitTerrain = DataBase.TryGetGameObject(item.SpawnOnHitTerrain);

            if (!item.AttackStatusEffect.IsNullOrWhiteSpace())
            {
                var effect = ObjectDB.instance.GetStatusEffect(item.AttackStatusEffect.GetStableHashCode());
                if (effect)
                {
                    component.m_itemData.m_shared.m_attackStatusEffect = effect;
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
        foreach (var data in input)
        {
            GameObject? effect = DataBase.TryGetGameObject(data.Prefab);
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
    
    private static void SaveMonsterTexture(GameObject critter)
    {
        SkinnedMeshRenderer renderer = critter.GetComponentInChildren<SkinnedMeshRenderer>();
        if (!renderer) return;
        foreach (var material in renderer.materials)
        {
            Texture2D? tex = material.mainTexture as Texture2D;
            if (tex == null) continue;
            TextureManager.SaveTextureToPNG(tex, critter.name);
        }
    }

    private static void SaveHumanoidData(ref MonsterData data, Humanoid humanoid)
    {
        data.Character.Name = humanoid.m_name;
        data.Character.Group = humanoid.m_group;
        data.Character.Faction = humanoid.m_faction.ToString();
        data.Character.Boss = humanoid.m_boss;
        data.Character.DoNotHideBossHUD = humanoid.m_dontHideBossHud;
        data.Character.BossEvent = humanoid.m_bossEvent;
        data.Character.DefeatSetGlobalKey = humanoid.m_defeatSetGlobalKey;
        // Movement & Physics
        data.Character.CrouchSpeed = humanoid.m_crouchSpeed;
        data.Character.WalkSpeed = humanoid.m_walkSpeed;
        data.Character.Speed = humanoid.m_speed;
        data.Character.TurnSpeed = humanoid.m_turnSpeed;
        data.Character.RunSpeed = humanoid.m_runSpeed;
        data.Character.RunTurnSpeed = humanoid.m_runTurnSpeed;
        data.Character.FlySlowSpeed = humanoid.m_flySlowSpeed;
        data.Character.FlyFastSpeed = humanoid.m_flyFastSpeed;
        data.Character.FlyTurnSpeed = humanoid.m_flyTurnSpeed;
        data.Character.Acceleration = humanoid.m_acceleration;
        data.Character.JumpForce = humanoid.m_jumpForce;
        data.Character.JumpForceForward = humanoid.m_jumpForceForward;
        data.Character.JumpForceTiredFactor = humanoid.m_jumpForceTiredFactor;
        data.Character.AirControl = humanoid.m_airControl;
        data.Character.CanSwim = humanoid.m_canSwim;
        data.Character.SwimDepth = humanoid.m_swimDepth;
        data.Character.SwimSpeed = humanoid.m_swimSpeed;
        data.Character.SwimTurnSpeed = humanoid.m_swimTurnSpeed;
        data.Character.SwimAcceleration = humanoid.m_swimAcceleration;
        data.Character.GroundTilt = humanoid.m_groundTilt.ToString();
        data.Character.GroundTiltSpeed = humanoid.m_groundTiltSpeed;
        data.Character.Flying = humanoid.m_flying;
        data.Character.JumpStaminaUsage = humanoid.m_jumpStaminaUsage;
        data.Character.DisableWhileSleeping = humanoid.m_disableWhileSleeping;
        // Effects
        data.Character.HitEffects = FormatEffectData(humanoid.m_hitEffects);
        data.Character.CriticalHitEffects = FormatEffectData(humanoid.m_critHitEffects);
        data.Character.BackStabHitEffects = FormatEffectData(humanoid.m_backstabHitEffects);
        data.Character.DeathEffects = FormatEffectData(humanoid.m_deathEffects);
        data.Character.WaterEffects = FormatEffectData(humanoid.m_waterEffects);
        data.Character.TarEffects = FormatEffectData(humanoid.m_tarEffects);
        data.Character.SlideEffects = FormatEffectData(humanoid.m_slideEffects);
        data.Character.JumpEffects = FormatEffectData(humanoid.m_jumpEffects);
        data.Character.FlyingContinuousEffects = FormatEffectData(humanoid.m_flyingContinuousEffect);
        // Health & Damages
        data.Character.TolerateWater = humanoid.m_tolerateWater;
        data.Character.TolerateFire = humanoid.m_tolerateFire;
        data.Character.TolerateSmoke = humanoid.m_tolerateSmoke;
        data.Character.TolerateTar = humanoid.m_tolerateTar;
        data.Character.Health = humanoid.m_health;
        data.Character.DamageModifiers = FormatDamageModifiers(humanoid.m_damageModifiers);
        data.Character.StaggerWhenBlocked = humanoid.m_staggerWhenBlocked;
        data.Character.StaggerDamageFactor = humanoid.m_staggerDamageFactor;
        data.Character.DefaultItems = FormatItemList(humanoid.m_defaultItems);
        data.Character.RandomWeapons = FormatItemList(humanoid.m_randomWeapon);
        data.Character.RandomArmors = FormatItemList(humanoid.m_randomArmor);
        data.Character.RandomShields = FormatItemList(humanoid.m_randomShield);
        data.Character.RandomSets = FormatRandomSets(humanoid.m_randomSets);
        data.Character.UnarmedWeapon = humanoid.m_unarmedWeapon ? humanoid.m_unarmedWeapon.name : "";
        data.Character.BeardItem = humanoid.m_beardItem;
        data.Character.HairItem = humanoid.m_hairItem;
        // Effects
        data.Character.PickUpEffects = FormatEffectData(humanoid.m_pickupEffects);
        data.Character.DropEffects = FormatEffectData(humanoid.m_dropEffects);
        data.Character.ConsumeItemEffects = FormatEffectData(humanoid.m_consumeItemEffects);
        data.Character.EquipEffects = FormatEffectData(humanoid.m_equipEffects);
        data.Character.PerfectBlockEffect = FormatEffectData(humanoid.m_perfectBlockEffect);
        
    }

    private static void SaveCharacterData(ref MonsterData data, Character character)
    {
        data.Character.Name = character.m_name;
        data.Character.Group = character.m_group;
        data.Character.Faction = character.m_faction.ToString();
        data.Character.Boss = character.m_boss;
        data.Character.DoNotHideBossHUD = character.m_dontHideBossHud;
        data.Character.BossEvent = character.m_bossEvent;
        data.Character.DefeatSetGlobalKey = character.m_defeatSetGlobalKey;
        // Movement & Physics
        data.Character.CrouchSpeed = character.m_crouchSpeed;
        data.Character.WalkSpeed = character.m_walkSpeed;
        data.Character.Speed = character.m_speed;
        data.Character.TurnSpeed = character.m_turnSpeed;
        data.Character.RunSpeed = character.m_runSpeed;
        data.Character.RunTurnSpeed = character.m_runTurnSpeed;
        data.Character.FlySlowSpeed = character.m_flySlowSpeed;
        data.Character.FlyFastSpeed = character.m_flyFastSpeed;
        data.Character.FlyTurnSpeed = character.m_flyTurnSpeed;
        data.Character.Acceleration = character.m_acceleration;
        data.Character.JumpForce = character.m_jumpForce;
        data.Character.JumpForceForward = character.m_jumpForceForward;
        data.Character.JumpForceTiredFactor = character.m_jumpForceTiredFactor;
        data.Character.AirControl = character.m_airControl;
        data.Character.CanSwim = character.m_canSwim;
        data.Character.SwimDepth = character.m_swimDepth;
        data.Character.SwimSpeed = character.m_swimSpeed;
        data.Character.SwimTurnSpeed = character.m_swimTurnSpeed;
        data.Character.SwimAcceleration = character.m_swimAcceleration;
        data.Character.GroundTilt = character.m_groundTilt.ToString();
        data.Character.GroundTiltSpeed = character.m_groundTiltSpeed;
        data.Character.Flying = character.m_flying;
        data.Character.JumpStaminaUsage = character.m_jumpStaminaUsage;
        data.Character.DisableWhileSleeping = character.m_disableWhileSleeping;
        // Effects
        data.Character.HitEffects = FormatEffectData(character.m_hitEffects);
        data.Character.CriticalHitEffects = FormatEffectData(character.m_critHitEffects);
        data.Character.BackStabHitEffects = FormatEffectData(character.m_backstabHitEffects);
        data.Character.DeathEffects = FormatEffectData(character.m_deathEffects);
        data.Character.WaterEffects = FormatEffectData(character.m_waterEffects);
        data.Character.TarEffects = FormatEffectData(character.m_tarEffects);
        data.Character.SlideEffects = FormatEffectData(character.m_slideEffects);
        data.Character.JumpEffects = FormatEffectData(character.m_jumpEffects);
        data.Character.FlyingContinuousEffects = FormatEffectData(character.m_flyingContinuousEffect);
        // Health & Damages
        data.Character.TolerateWater = character.m_tolerateWater;
        data.Character.TolerateFire = character.m_tolerateFire;
        data.Character.TolerateSmoke = character.m_tolerateSmoke;
        data.Character.TolerateTar = character.m_tolerateTar;
        data.Character.Health = character.m_health;
        data.Character.DamageModifiers = FormatDamageModifiers(character.m_damageModifiers);
        data.Character.StaggerWhenBlocked = character.m_staggerWhenBlocked;
        data.Character.StaggerDamageFactor = character.m_staggerDamageFactor;
    }

    private static void SaveMonsterAIData(ref MonsterData data, MonsterAI monsterAI)
    {
        data.MonsterAI.ViewRange = monsterAI.m_viewRange;
        data.MonsterAI.ViewAngle = monsterAI.m_viewAngle;
        data.MonsterAI.HearRange = monsterAI.m_hearRange;
        data.MonsterAI.MistVision = monsterAI.m_mistVision;
        data.MonsterAI.AlertedEffects = FormatEffectData(monsterAI.m_alertedEffects);
        data.MonsterAI.IdleSound = FormatEffectData(monsterAI.m_idleSound);
        data.MonsterAI.IdleSoundInterval = monsterAI.m_idleSoundInterval;
        data.MonsterAI.IdleSoundChance = monsterAI.m_idleSoundChance;
        data.MonsterAI.PathAgentType = monsterAI.m_pathAgentType.ToString();
        data.MonsterAI.MoveMinimumAngle = monsterAI.m_moveMinAngle;
        data.MonsterAI.smoothMovement = monsterAI.m_smoothMovement;
        data.MonsterAI.SerpentMovement = monsterAI.m_serpentMovement;
        data.MonsterAI.SerpentTurnRadius = monsterAI.m_serpentTurnRadius;
        data.MonsterAI.JumpInterval = monsterAI.m_jumpInterval;
        // Random Circle
        data.MonsterAI.RandomCircleInterval = monsterAI.m_randomCircleInterval;
        // Random Movement
        data.MonsterAI.RandomMoveInterval = monsterAI.m_randomMoveInterval;
        data.MonsterAI.RandomMoveRange = monsterAI.m_randomMoveRange;
        // Fly Behavior
        data.MonsterAI.RandomFly = monsterAI.m_randomFly;
        data.MonsterAI.ChanceToTakeOff = monsterAI.m_chanceToTakeoff;
        data.MonsterAI.ChanceToLand = monsterAI.m_chanceToLand;
        data.MonsterAI.GroundDuration = monsterAI.m_groundDuration;
        data.MonsterAI.AirDuration = monsterAI.m_airDuration;
        data.MonsterAI.MaxLandAltitude = monsterAI.m_maxLandAltitude;
        data.MonsterAI.TakeOffTime = monsterAI.m_takeoffTime;
        data.MonsterAI.FlyAltitudeMinimum = monsterAI.m_flyAltitudeMin;
        data.MonsterAI.FlyAltitudeMaximum = monsterAI.m_flyAltitudeMax;
        data.MonsterAI.FlyAbsoluteMinimumAltitude = monsterAI.m_flyAbsMinAltitude;
        // Other
        data.MonsterAI.AvoidFire = monsterAI.m_avoidFire;
        data.MonsterAI.AfraidOfFire = monsterAI.m_afraidOfFire;
        data.MonsterAI.AvoidWater = monsterAI.m_avoidWater;
        data.MonsterAI.Aggravatable = monsterAI.m_aggravatable;
        data.MonsterAI.PassiveAggressive = monsterAI.m_passiveAggresive;
        data.MonsterAI.SpawnMessage = monsterAI.m_spawnMessage;
        data.MonsterAI.DeathMessage = monsterAI.m_deathMessage;
        data.MonsterAI.AlertedMessage = monsterAI.m_alertedMessage;
        // Monster AI
        data.MonsterAI.AlertRange = monsterAI.m_alertRange;
        data.MonsterAI.FleeIfHurtWhenTargetCannotBeReached = monsterAI.m_fleeIfHurtWhenTargetCantBeReached;
        data.MonsterAI.FleeIfNotAlerted = monsterAI.m_fleeIfNotAlerted;
        data.MonsterAI.FleeIfLowHealth = monsterAI.m_fleeIfLowHealth;
        data.MonsterAI.CirculateWhileCharging = monsterAI.m_circulateWhileCharging;
        data.MonsterAI.CirculateWhileChargingFlying = monsterAI.m_circulateWhileChargingFlying;
        data.MonsterAI.EnableHuntPlayer = monsterAI.m_enableHuntPlayer;
        data.MonsterAI.AttackPlayerObjects = monsterAI.m_attackPlayerObjects;
        data.MonsterAI.PrivateAreaTriggerThreshold = monsterAI.m_privateAreaTriggerTreshold;
        data.MonsterAI.InterceptTimeMaximum = monsterAI.m_interceptTimeMax;
        data.MonsterAI.InterceptTimeMinimum = monsterAI.m_interceptTimeMin;
        data.MonsterAI.MaximumChaseDistance = monsterAI.m_maxChaseDistance;
        data.MonsterAI.MinimumAttackInterval = monsterAI.m_minAttackInterval;
        // Circle Target
        data.MonsterAI.CircleTargetInterval = monsterAI.m_circleTargetInterval;
        data.MonsterAI.CircleTargetDuration = monsterAI.m_circleTargetDuration;
        data.MonsterAI.CircleTargetDistance = monsterAI.m_circleTargetDistance;
        // Sleep
        data.MonsterAI.Sleeping = monsterAI.m_sleeping;
        data.MonsterAI.WakeUpRange = monsterAI.m_wakeupRange;
        data.MonsterAI.NoiseWakeUp = monsterAI.m_noiseWakeup;
        data.MonsterAI.MaximumNoiseWakeUpRange = monsterAI.m_maxNoiseWakeupRange;
        data.MonsterAI.WakeUpEffects = FormatEffectData(monsterAI.m_wakeupEffects);
        data.MonsterAI.WakeUpDelayMinimum = monsterAI.m_wakeUpDelayMin;
        data.MonsterAI.WakeUpDelayMaximum = monsterAI.m_wakeUpDelayMax;
        // Other
        data.MonsterAI.AvoidLand = monsterAI.m_avoidLand;
        // Consume Items
        data.MonsterAI.ConsumeItems = FormatItemList(monsterAI.m_consumeItems);
        data.MonsterAI.ConsumeRange = monsterAI.m_consumeRange;
        data.MonsterAI.ConsumeSearchRange = monsterAI.m_consumeSearchRange;
        data.MonsterAI.ConsumeSearchInterval = monsterAI.m_consumeSearchInterval;
    }

    private static void SaveAnimalAIData(ref MonsterData data, AnimalAI animalAI)
    {
        data.MonsterAI.ViewRange = animalAI.m_viewRange;
        data.MonsterAI.ViewAngle = animalAI.m_viewAngle;
        data.MonsterAI.HearRange = animalAI.m_hearRange;
        data.MonsterAI.MistVision = animalAI.m_mistVision;
        data.MonsterAI.AlertedEffects = FormatEffectData(animalAI.m_alertedEffects);
        data.MonsterAI.IdleSound = FormatEffectData(animalAI.m_idleSound);
        data.MonsterAI.IdleSoundInterval = animalAI.m_idleSoundInterval;
        data.MonsterAI.IdleSoundChance = animalAI.m_idleSoundChance;
        data.MonsterAI.PathAgentType = animalAI.m_pathAgentType.ToString();
        data.MonsterAI.MoveMinimumAngle = animalAI.m_moveMinAngle;
        data.MonsterAI.smoothMovement = animalAI.m_smoothMovement;
        data.MonsterAI.SerpentMovement = animalAI.m_serpentMovement;
        data.MonsterAI.SerpentTurnRadius = animalAI.m_serpentTurnRadius;
        data.MonsterAI.JumpInterval = animalAI.m_jumpInterval;
        // Random Circle
        data.MonsterAI.RandomCircleInterval = animalAI.m_randomCircleInterval;
        // Random Movement
        data.MonsterAI.RandomMoveInterval = animalAI.m_randomMoveInterval;
        data.MonsterAI.RandomMoveRange = animalAI.m_randomMoveRange;
        // Fly Behavior
        data.MonsterAI.RandomFly = animalAI.m_randomFly;
        data.MonsterAI.ChanceToTakeOff = animalAI.m_chanceToTakeoff;
        data.MonsterAI.ChanceToLand = animalAI.m_chanceToLand;
        data.MonsterAI.GroundDuration = animalAI.m_groundDuration;
        data.MonsterAI.AirDuration = animalAI.m_airDuration;
        data.MonsterAI.MaxLandAltitude = animalAI.m_maxLandAltitude;
        data.MonsterAI.TakeOffTime = animalAI.m_takeoffTime;
        data.MonsterAI.FlyAltitudeMinimum = animalAI.m_flyAltitudeMin;
        data.MonsterAI.FlyAltitudeMaximum = animalAI.m_flyAltitudeMax;
        data.MonsterAI.FlyAbsoluteMinimumAltitude = animalAI.m_flyAbsMinAltitude;
        // Other
        data.MonsterAI.AvoidFire = animalAI.m_avoidFire;
        data.MonsterAI.AfraidOfFire = animalAI.m_afraidOfFire;
        data.MonsterAI.AvoidWater = animalAI.m_avoidWater;
        data.MonsterAI.Aggravatable = animalAI.m_aggravatable;
        data.MonsterAI.PassiveAggressive = animalAI.m_passiveAggresive;
        data.MonsterAI.SpawnMessage = animalAI.m_spawnMessage;
        data.MonsterAI.DeathMessage = animalAI.m_deathMessage;
        data.MonsterAI.AlertedMessage = animalAI.m_alertedMessage;
    }

    private static void SaveProcreationData(ref MonsterData data, Procreation procreation)
    {
        data.Procreation.Enabled = true;
        data.Procreation.UpdateInterval = procreation.m_updateInterval;
        data.Procreation.TotalCheckRange = procreation.m_totalCheckRange;
        data.Procreation.MaxCreatures = procreation.m_maxCreatures;
        data.Procreation.PartnerCheckRange = procreation.m_partnerCheckRange;
        data.Procreation.PregnancyChance = procreation.m_pregnancyChance;
        data.Procreation.PregnancyDuration = procreation.m_pregnancyDuration;
        data.Procreation.RequiredLovePoints = procreation.m_requiredLovePoints;
        data.Procreation.OffSpring = procreation.m_offspring ? procreation.m_offspring.name : "";
        data.Procreation.MinimumOffspringLevel = procreation.m_minOffspringLevel;
        data.Procreation.SpawnOffset = procreation.m_spawnOffset;
        data.Procreation.BirthEffects = FormatEffectData(procreation.m_birthEffects);
        data.Procreation.LoveEffects = FormatEffectData(procreation.m_loveEffects);
    }

    private static void SaveTameData(ref MonsterData data, Tameable tameable)
    {
        data.Tameable.Enabled = true;
        data.Tameable.FedDuration = tameable.m_fedDuration;
        data.Tameable.TamingTime = tameable.m_tamingTime;
        data.Tameable.StartsTamed = tameable.m_startsTamed;
        data.Tameable.TamedEffects = FormatEffectData(tameable.m_tamedEffect);
        data.Tameable.SootheEffects = FormatEffectData(tameable.m_sootheEffect);
        data.Tameable.PetEffects = FormatEffectData(tameable.m_petEffect);
        data.Tameable.Commandable = tameable.m_commandable;
        data.Tameable.UnSummonDistance = tameable.m_unsummonDistance;
        data.Tameable.UnSummonOnOwnerLogoutSeconds = tameable.m_unsummonOnOwnerLogoutSeconds;
        data.Tameable.UnSummonEffects = FormatEffectData(tameable.m_unSummonEffect);
        data.Tameable.LevelUpOwnerSkill = tameable.m_levelUpOwnerSkill.ToString();
        data.Tameable.LevelUpFactor = tameable.m_levelUpFactor;
        data.Tameable.SaddleItem = tameable.m_saddleItem ? tameable.m_saddleItem.name : "";
        data.Tameable.DropSaddleOnDeath = tameable.m_dropSaddleOnDeath;
        data.Tameable.DropItemVelocity = tameable.m_dropItemVel;
        data.Tameable.RandomStartingName = tameable.m_randomStartingName;
    }

    private static void SaveMaterialData(ref MonsterData data, GameObject critter)
    {
        SkinnedMeshRenderer renderer = critter.GetComponentInChildren<SkinnedMeshRenderer>();
        if (renderer)
        {
            foreach (var material in renderer.materials)
            {
                var materialData = new MaterialData
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
    }
    
    private static bool GetMonsterData(string creatureName, out MonsterData data)
    {
        data = new();
        if (!ZNetScene.instance) return false;
        GameObject critter = ZNetScene.instance.GetPrefab(creatureName);
        if (!critter)
        {
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find " + creatureName);
            return false;
        }
        // var visual = Utils.FindChild(critter.transform, "Visual");
        // if (!visual)
        // {
        //     MonsterDBPlugin.MonsterDBLogger.LogInfo("Failed to find Visual of " + creatureName);
        //     return false;
        // }

        // var localScale = visual.localScale;
        var localScale = critter.transform.localScale;
        data.Character.Scale = new Scale(){x = localScale.x, y = localScale.y, z = localScale.z};

        if (critter.TryGetComponent(out CapsuleCollider collider))
        {
            data.Character.ColliderHeight = collider.height;
            var center = collider.center;
            data.Character.ColliderCenter = new Scale()
                { x = center.x, y = center.y, z = center.z };
        }
        
        data.Character.PrefabName = creatureName;
        if (critter.TryGetComponent(out Humanoid humanoid)) SaveHumanoidData(ref data, humanoid);
        else if (critter.TryGetComponent(out Character character)) SaveCharacterData(ref data, character);
        else return false;
        if (critter.TryGetComponent(out MonsterAI monsterAI)) SaveMonsterAIData(ref data, monsterAI);
        else if (critter.TryGetComponent(out AnimalAI animalAI)) SaveAnimalAIData(ref data, animalAI);
        else return false;
        if (critter.TryGetComponent(out CharacterDrop characterDrop))
        {
            data.CharacterDrop.Drops = FormatDropData(characterDrop.m_drops);
        }
        if (critter.TryGetComponent(out Procreation procreation)) SaveProcreationData(ref data, procreation);
        if (critter.TryGetComponent(out Tameable tameable)) SaveTameData(ref data, tameable);
        SaveMaterialData(ref data, critter);
        // SaveMonsterTexture(critter);
        
        if (!m_defaultMonsterData.ContainsKey(creatureName))
        {
            m_defaultMonsterData[creatureName] = data;
            MonsterDBPlugin.MonsterDBLogger.LogInfo("Saved default data for " + data.Character.PrefabName);
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
            var attack = component.m_itemData.m_shared.m_attack;
            var itemData = new CreatureItem()
            {
                Name = prefab.name,
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
                Blockable = component.m_itemData.m_shared.m_blockable
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
            output.Add(itemData);
        }

        return output;
    }

    private static AttackDamages FormatDamages(HitData.DamageTypes input)
    {
        return new()
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
        return new()
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