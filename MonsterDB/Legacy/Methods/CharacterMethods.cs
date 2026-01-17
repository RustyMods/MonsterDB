using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class CharacterMethods
{
    public static void ReadCharacter(string folderPath, ref CreatureData creatureData)
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
        Vector3 scale = creatureData.m_scale.ToRef();
        Vector3 ragDollScale = creatureData.m_ragdollScale.ToRef();
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
    
}