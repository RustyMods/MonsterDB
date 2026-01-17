using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class NPCTalkMethods
{
    public static void ReadNPCTalk(string folderPath, ref CreatureData creatureData)
    {
        string filePath = folderPath + Path.DirectorySeparatorChar + "NPCTalk.yml";
        if (!File.Exists(filePath)) return;
        var deserializer = new DeserializerBuilder().Build();
        string serial = File.ReadAllText(filePath);
        if (serial.IsNullOrWhiteSpace()) return;
        try
        {
            creatureData.m_npcTalk = deserializer.Deserialize<NPCTalkData>(serial);
        }
        catch
        {
            Helpers.LogParseFailure(filePath);
        }
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) return;
        string randomTalkFXPath = effectsFolder + Path.DirectorySeparatorChar + "RandomTalkFX.yml";
        string randomGreetFXPath = effectsFolder + Path.DirectorySeparatorChar + "RandomGreetFX.yml";
        string randomGoodbyeFXPath = effectsFolder + Path.DirectorySeparatorChar + "RandomGoodbyeFX.yml";
        Helpers.ReadEffectInfo(randomTalkFXPath, ref creatureData.m_effects.m_randomTalkFX);
        Helpers.ReadEffectInfo(randomGreetFXPath, ref creatureData.m_effects.m_randomGreetFX);
        Helpers.ReadEffectInfo(randomGoodbyeFXPath, ref creatureData.m_effects.m_randomGoodbyeFX);
    }
    
    public static void Update(GameObject critter, CreatureData creatureData)
    {
        if (!critter.GetComponent<MonsterAI>() || !critter.TryGetComponent(out NpcTalk component)) return;
        NPCTalkData data = creatureData.m_npcTalk;

        Vector3 scale = creatureData.m_scale.ToRef();
        component.m_name = data.Name;
        component.m_maxRange = data.MaxRange;
        component.m_greetRange = data.GreetRange;
        component.m_byeRange = data.ByeRange;
        component.m_offset = data.Offset;
        component.m_minTalkInterval = data.MinTalkInterval;
        component.m_hideDialogDelay = data.HideDialogueDelay;
        component.m_randomTalkInterval = data.RandomTalkInterval;
        component.m_randomTalkChance = data.RandomTalkChance;
        component.m_randomTalk = data.RandomTalk;
        component.m_randomTalkInFactionBase = data.RandomTalkInFactionBase;
        component.m_randomGreets = data.RandomGreets;
        component.m_randomGoodbye = data.RandomGoodbye;
        component.m_privateAreaAlarm = data.PrivateAreaAlarm;
        component.m_aggravated = data.Aggravated;
        
        Helpers.UpdateEffectList(creatureData.m_effects.m_randomTalkFX, ref component.m_randomTalkFX, scale);
        Helpers.UpdateEffectList(creatureData.m_effects.m_randomGreetFX, ref component.m_randomGreetFX, scale);
        Helpers.UpdateEffectList(creatureData.m_effects.m_randomGoodbyeFX, ref component.m_randomGoodbyeFX, scale);
    }
}