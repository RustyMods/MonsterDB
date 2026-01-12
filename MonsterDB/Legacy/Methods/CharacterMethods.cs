using System;
using System.Collections.Generic;
using System.IO;
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
    
}