using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class CharacterMethods
{
    public static void Save(GameObject critter, string clonedFrom, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Character component)) return;
        creatureData.m_characterData = RecordCharacterData(component, clonedFrom, critter);;
        SaveEffectList(component.m_hitEffects, ref creatureData.m_effects.m_hitEffects);
        SaveEffectList(component.m_critHitEffects, ref creatureData.m_effects.m_critHitEffects);
        SaveEffectList(component.m_backstabHitEffects, ref creatureData.m_effects.m_backstabHitEffects);
        SaveEffectList(component.m_deathEffects, ref creatureData.m_effects.m_deathEffects);
        SaveEffectList(component.m_waterEffects, ref creatureData.m_effects.m_waterEffects);
        SaveEffectList(component.m_tarEffects, ref creatureData.m_effects.m_tarEffects);
        SaveEffectList(component.m_slideEffects, ref creatureData.m_effects.m_slideEffects);
        SaveEffectList(component.m_jumpEffects, ref creatureData.m_effects.m_jumpEffects);
        SaveEffectList(component.m_flyingContinuousEffect, ref creatureData.m_effects.m_flyingContinuousEffects);
        if (component.m_deathEffects != null)
        {
            foreach (var effect in component.m_deathEffects.m_effectPrefabs)
            {
                if (!effect.m_prefab.GetComponent<Ragdoll>()) continue;
                var scale = effect.m_prefab.transform.localScale;
                VisualMethods.ScaleData scaleData = new VisualMethods.ScaleData()
                {
                    x = scale.x, y = scale.y, z = scale.z
                };
                creatureData.m_ragdollScale = scaleData;
            }
        }
    }
    
    public static void Write(GameObject critter, string folderPath, string clonedFrom = "")
    {
        if (!critter.TryGetComponent(out Character component)) return;
        CharacterData data = RecordCharacterData(component, clonedFrom, critter);
        string fileName = "Character.yml";
        string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);

        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);

        string hitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitEffects.yml";
        string critHitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "CritHitEffects.yml";
        string backstabHitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BackstabHitEffects.yml";
        string deathEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "DeathEffects.yml";
        string waterEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "WaterEffects.yml";
        string tarEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TarEffects.yml";
        string slideEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "SlideEffects.yml";
        string jumpEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "JumpEffects.yml";
        string flyingContinuousEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "FlyingContinuousEffects.yml";
        
        WriteEffectList(component.m_hitEffects, hitEffectsPath);
        WriteEffectList(component.m_critHitEffects, critHitEffectsPath);
        WriteEffectList(component.m_backstabHitEffects, backstabHitEffectsPath);
        WriteEffectList(component.m_deathEffects, deathEffectsPath);
        WriteEffectList(component.m_waterEffects, waterEffectsPath);
        WriteEffectList(component.m_tarEffects, tarEffectsPath);
        WriteEffectList(component.m_slideEffects, slideEffectsPath);
        WriteEffectList(component.m_jumpEffects, jumpEffectsPath);
        WriteEffectList(component.m_flyingContinuousEffect, flyingContinuousEffectsPath);
        
        if (component.m_deathEffects != null)
        {
            foreach (var effect in component.m_deathEffects.m_effectPrefabs)
            {
                if (!effect.m_prefab.GetComponent<Ragdoll>()) continue;
                var scale = effect.m_prefab.transform.localScale;
                VisualMethods.ScaleData scaleData = new VisualMethods.ScaleData()
                {
                    x = scale.x, y = scale.y, z = scale.z
                };
                string visualFolderPath = folderPath + Path.DirectorySeparatorChar + "Visual";
                if (!Directory.Exists(visualFolderPath)) Directory.CreateDirectory(visualFolderPath);
                string ragdollscalePath = visualFolderPath + Path.DirectorySeparatorChar + "RagdollScale.yml";
                File.WriteAllText(ragdollscalePath, serializer.Serialize(scaleData));
            }
        }
    }

    private static CharacterData RecordCharacterData(Character component, string clonedFrom, GameObject critter)
    {
        return new CharacterData
        {
            ClonedFrom = clonedFrom,
            PrefabName = critter.name,
            Name = component.m_name,
            Group = component.m_group,
            Faction = component.m_faction.ToString(),
            Boss = component.m_boss,
            DoNotHideBossHUD = component.m_dontHideBossHud,
            BossEvent = component.m_bossEvent,
            DefeatSetGlobalKey = component.m_defeatSetGlobalKey,
            AISkipTarget = component.m_aiSkipTarget,
            CrouchSpeed = component.m_crouchSpeed,
            WalkSpeed = component.m_walkSpeed,
            Speed = component.m_speed,
            TurnSpeed = component.m_turnSpeed,
            RunSpeed = component.m_runSpeed,
            RunTurnSpeed = component.m_runTurnSpeed,
            FlySlowSpeed = component.m_flySlowSpeed,
            FlyFastSpeed = component.m_flyFastSpeed,
            FlyTurnSpeed = component.m_flyTurnSpeed,
            Acceleration = component.m_acceleration,
            JumpForce = component.m_jumpForce,
            JumpForceForward = component.m_jumpForceForward,
            JumpForceTiredFactor = component.m_jumpForceTiredFactor,
            AirControl = component.m_airControl,
            CanSwim = component.m_canSwim,
            SwimDepth = component.m_swimDepth,
            SwimTurnSpeed = component.m_swimTurnSpeed,
            SwimAcceleration = component.m_swimAcceleration,
            GroundTilt = component.m_groundTilt.ToString(),
            GroundTiltSpeed = component.m_groundTiltSpeed,
            Flying = component.m_flying,
            JumpStaminaUsage = component.m_jumpStaminaUsage,
            DisableWhileSleeping = component.m_disableWhileSleeping,
            TolerateWater = component.m_tolerateWater,
            TolerateFire = component.m_tolerateFire,
            TolerateSmoke = component.m_tolerateSmoke,
            TolerateTar = component.m_tolerateTar,
            Health = component.m_health,
            BluntResistance = component.m_damageModifiers.m_blunt.ToString(),
            SlashResistance = component.m_damageModifiers.m_slash.ToString(),
            PierceResistance = component.m_damageModifiers.m_pierce.ToString(),
            ChopResistance = component.m_damageModifiers.m_chop.ToString(),
            PickaxeResistance = component.m_damageModifiers.m_pickaxe.ToString(),
            FireResistance = component.m_damageModifiers.m_fire.ToString(),
            FrostResistance = component.m_damageModifiers.m_frost.ToString(),
            LightningResistance = component.m_damageModifiers.m_lightning.ToString(),
            PoisonResistance = component.m_damageModifiers.m_poison.ToString(),
            SpiritResistance = component.m_damageModifiers.m_spirit.ToString(),
            StaggerWhenBlocked = component.m_staggerWhenBlocked,
            StaggerDamageFactor = component.m_staggerDamageFactor,
        };
    }
    
    public static void Read(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "Character.yml";
        if (!File.Exists(filePath)) return;
        IDeserializer deserializer = new DeserializerBuilder().Build();
        try
        {
            CharacterData data = deserializer.Deserialize<CharacterData>(File.ReadAllText(filePath));
            creatureData.m_characterData = data;
        }
        catch
        {
            LogParseFailure(filePath);
        }
        
        ReadEffects(folderPath, ref creatureData);
    }

    private static void ReadEffects(string folderPath, ref CreatureData creatureData)
    {
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) return;
        
        string hitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitEffects.yml";
        string critHitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "CritHitEffects.yml";
        string backstabHitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BackstabHitEffects.yml";
        string deathEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "DeathEffects.yml";
        string waterEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "WaterEffects.yml";
        string tarEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TarEffects.yml";
        string slideEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "SlideEffects.yml";
        string jumpEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "JumpEffects.yml";
        string flyingContinuousEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "FlyingContinuousEffects.yml";

        ReadEffectInfo(hitEffectsPath, ref creatureData.m_effects.m_hitEffects);
        ReadEffectInfo(critHitEffectsPath, ref creatureData.m_effects.m_critHitEffects);
        ReadEffectInfo(backstabHitEffectsPath, ref creatureData.m_effects.m_backstabHitEffects);
        ReadEffectInfo(deathEffectsPath, ref creatureData.m_effects.m_deathEffects);
        ReadEffectInfo(waterEffectsPath, ref creatureData.m_effects.m_waterEffects);
        ReadEffectInfo(tarEffectsPath, ref creatureData.m_effects.m_tarEffects);
        ReadEffectInfo(slideEffectsPath, ref creatureData.m_effects.m_slideEffects);
        ReadEffectInfo(jumpEffectsPath, ref creatureData.m_effects.m_jumpEffects);
        ReadEffectInfo(flyingContinuousEffectsPath, ref creatureData.m_effects.m_flyingContinuousEffects);
    }

    public static void Update(GameObject critter, CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out Character component)) return;
        CharacterData data = creatureData.m_characterData;
        Vector3 scale = GetScale(creatureData.m_scale);
        Vector3 ragDollScale = GetScale(creatureData.m_ragdollScale);
        CharacterEffects effectData = creatureData.m_effects;
        
        component.m_name = data.Name;
        component.m_group = data.Group;
        if (Enum.TryParse(data.Faction, true, out Character.Faction faction))
        {
            component.m_faction = faction;
        }
        component.m_boss = data.Boss;
        component.m_dontHideBossHud = data.DoNotHideBossHUD;
        component.m_bossEvent = data.BossEvent;
        component.m_defeatSetGlobalKey = data.DefeatSetGlobalKey;
        component.m_aiSkipTarget = data.AISkipTarget;
        component.m_crouchSpeed = data.CrouchSpeed;
        component.m_walkSpeed = data.WalkSpeed;
        component.m_speed = data.Speed;
        component.m_turnSpeed = data.TurnSpeed;
        component.m_runSpeed = data.RunSpeed;
        component.m_runTurnSpeed = data.RunTurnSpeed;
        component.m_flySlowSpeed = data.FlySlowSpeed;
        component.m_flyFastSpeed = data.FlyFastSpeed;
        component.m_flyTurnSpeed = data.FlyTurnSpeed;
        component.m_acceleration = data.Acceleration;
        component.m_jumpForce = data.JumpForce;
        component.m_jumpForceForward = data.JumpForceForward;
        component.m_jumpForceTiredFactor = data.JumpForceTiredFactor;
        component.m_airControl = data.AirControl;
        component.m_canSwim = data.CanSwim;
        component.m_swimDepth = data.SwimDepth;
        component.m_swimTurnSpeed = data.SwimTurnSpeed;
        component.m_swimAcceleration = data.SwimAcceleration;
        if (Enum.TryParse(data.GroundTilt, true, out Character.GroundTiltType groundTiltType))
        {
            component.m_groundTilt = groundTiltType;
        }

        component.m_swimSpeed = data.SwimSpeed;
        component.m_groundTiltSpeed = data.GroundTiltSpeed;
        component.m_flying = data.Flying;
        component.m_jumpStaminaUsage = data.JumpStaminaUsage;
        component.m_disableWhileSleeping = data.DisableWhileSleeping;
        component.m_tolerateWater = data.TolerateWater;
        component.m_tolerateFire = data.TolerateFire;
        component.m_tolerateSmoke = data.TolerateSmoke;
        component.m_tolerateTar = data.TolerateTar;
        component.m_health = data.Health;
        if (Enum.TryParse(data.BluntResistance, true, out HitData.DamageModifier blunt))
        {
            component.m_damageModifiers.m_blunt = blunt;
        }
        if (Enum.TryParse(data.SlashResistance, true, out HitData.DamageModifier slash))
        {
            component.m_damageModifiers.m_slash = slash;
        }

        if (Enum.TryParse(data.PierceResistance, true, out HitData.DamageModifier pierce))
        {
            component.m_damageModifiers.m_pierce = pierce;
        }

        if (Enum.TryParse(data.ChopResistance, true, out HitData.DamageModifier chop))
        {
            component.m_damageModifiers.m_chop = chop;
        }

        if (Enum.TryParse(data.PickaxeResistance, true, out HitData.DamageModifier pickaxe))
        {
            component.m_damageModifiers.m_pickaxe = pickaxe;
        }

        if (Enum.TryParse(data.FireResistance, true, out HitData.DamageModifier fire))
        {
            component.m_damageModifiers.m_fire = fire;
        }

        if (Enum.TryParse(data.FrostResistance, true, out HitData.DamageModifier frost))
        {
            component.m_damageModifiers.m_frost = frost;
        }

        if (Enum.TryParse(data.LightningResistance, true, out HitData.DamageModifier lightning))
        {
            component.m_damageModifiers.m_lightning = lightning;
        }

        if (Enum.TryParse(data.PoisonResistance, true, out HitData.DamageModifier poison))
        {
            component.m_damageModifiers.m_poison = poison;
        }

        if (Enum.TryParse(data.SpiritResistance, true, out HitData.DamageModifier spirit))
        {
            component.m_damageModifiers.m_spirit = spirit;
        }
        component.m_staggerWhenBlocked = data.StaggerWhenBlocked;
        component.m_staggerDamageFactor = data.StaggerDamageFactor;

        UpdateEffectList(effectData.m_hitEffects, ref component.m_hitEffects, scale);
        UpdateEffectList(effectData.m_critHitEffects, ref component.m_critHitEffects, scale);
        UpdateEffectList(effectData.m_backstabHitEffects, ref component.m_backstabHitEffects, scale);
        UpdateEffectList(effectData.m_deathEffects, ref component.m_deathEffects, ragDollScale);
        UpdateEffectList(effectData.m_waterEffects, ref component.m_waterEffects, scale);
        UpdateEffectList(effectData.m_tarEffects, ref component.m_tarEffects, scale);
        UpdateEffectList(effectData.m_slideEffects, ref component.m_slideEffects, scale);
        UpdateEffectList(effectData.m_jumpEffects, ref component.m_jumpEffects, scale);
        UpdateEffectList(effectData.m_flyingContinuousEffects, ref component.m_flyingContinuousEffect, scale);
    }

    public static void CloneRagDoll(GameObject critter, Dictionary<string, Material> ragDollMats)
    {
        if (!critter.TryGetComponent(out Character component)) return;
        CloneEffectList(critter, ref component.m_deathEffects, ragDollMats);
    }
}