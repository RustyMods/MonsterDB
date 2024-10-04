using System.Collections.Generic;
using MonsterDB.Solution.Behaviors;
using MonsterDB.Solution.Methods;
using UnityEngine;

namespace MonsterDB.Solution;

public static class HumanMan
{
    public static readonly Dictionary<string, GameObject> m_newHumans = new();
    public static GameObject? Create(string name)
    {
        if (!ZNetScene.instance) return null;
        GameObject player = ZNetScene.instance.GetPrefab("Player");
        if (!player) return null;
        if (!player.TryGetComponent(out Player component)) return null;
        GameObject human = Object.Instantiate(player, MonsterDBPlugin.m_root.transform, false);
        human.name = name;

        Object.Destroy(human.GetComponent<Player>());
        Object.Destroy(human.GetComponent<PlayerController>());
        Object.Destroy(human.GetComponent<Talker>());
        Object.Destroy(human.GetComponent<Skills>());
        
        if (human.TryGetComponent(out ZNetView zNetView))
        {
            zNetView.m_persistent = true;
            zNetView.m_type = ZDO.ObjectType.Default;
        }
        
        Human humanoid = human.AddComponent<Human>();
        humanoid.m_eye = Utils.FindChild(human.transform, "EyePos");
        MonsterAI monsterAI = human.AddComponent<MonsterAI>();
        CharacterDrop characterDrop = human.AddComponent<CharacterDrop>();
        Tameable tameable = human.AddComponent<Tameable>();
        human.AddComponent<Visuals>();
        GameObject newRagDoll = Object.Instantiate(ZNetScene.instance.GetPrefab("Player_ragdoll"), MonsterDBPlugin.m_root.transform, false);
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
        newRagDoll.name = name + "_ragdoll";
        newRagDoll.AddComponent<Visuals>();
        Helpers.RegisterToZNetScene(newRagDoll);
        humanoid.m_deathEffects = new EffectList
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
        humanoid.m_name = "Human";
        humanoid.m_group = "Humans";
        humanoid.m_faction = Character.Faction.ForestMonsters;
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
        humanoid.m_hitEffects = component.m_hitEffects;
        humanoid.m_critHitEffects = component.m_critHitEffects;
        humanoid.m_backstabHitEffects = component.m_backstabHitEffects;
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
        humanoid.m_randomWeapon = component.m_randomWeapon;
        humanoid.m_randomArmor = component.m_randomArmor;
        humanoid.m_randomShield = component.m_randomShield;
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
        humanoid.m_randomItems = component.m_randomItems;
        humanoid.m_unarmedWeapon = component.m_unarmedWeapon;
        humanoid.m_pickupEffects = component.m_autopickupEffects;
        humanoid.m_dropEffects = component.m_dropEffects;
        humanoid.m_consumeItemEffects = component.m_consumeItemEffects;
        humanoid.m_equipEffects = component.m_equipStartEffects;
        humanoid.m_perfectBlockEffect = component.m_perfectBlockEffect;
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
        GameObject boar = ZNetScene.instance.GetPrefab("Boar");
        if (boar.TryGetComponent(out Tameable loxTame))
        {
            tameable.m_fedDuration = 600f;
            tameable.m_tamingTime = 1800f;
            tameable.m_commandable = true;
            tameable.m_tamedEffect = loxTame.m_tamedEffect;
            tameable.m_sootheEffect = loxTame.m_sootheEffect;
            tameable.m_petEffect = loxTame.m_petEffect;
            tameable.m_commandable = true;
            tameable.m_unsummonDistance = 0f;
            tameable.m_randomStartingName = new List<string>();
        }
        Helpers.RegisterToZNetScene(human);
        if (name != "Human") CreatureManager.m_clones[human.name] = human;
        m_newHumans[human.name] = human;
        return human;
    }
}