using System.Collections.Generic;
using System.IO;
using BepInEx;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB.Solution.Methods;

public static class NPCTalkMethods
{
    private static Dictionary<string, GameObject> m_newNPCTalkers = new();

    public static void Save(GameObject critter, ref CreatureData creatureData)
    {
        if (!critter.TryGetComponent(out NpcTalk component)) return;
        NPCTalkData data = RecordData(component);

        creatureData.m_npcTalk = data;

        Helpers.SaveEffectList(component.m_randomTalkFX, ref creatureData.m_effects.m_randomTalkFX);
        Helpers.SaveEffectList(component.m_randomGreetFX, ref creatureData.m_effects.m_randomGreetFX);
        Helpers.SaveEffectList(component.m_randomGoodbyeFX, ref creatureData.m_effects.m_randomGoodbyeFX);
    }

    private static NPCTalkData RecordData(NpcTalk component)
    {
        return new NPCTalkData
        {
            Name = component.m_name,
            MaxRange = component.m_maxRange,
            GreetRange = component.m_greetRange,
            ByeRange = component.m_byeRange,
            Offset = component.m_offset,
            MinTalkInterval = component.m_minTalkInterval,
            HideDialogueDelay = component.m_hideDialogDelay,
            RandomTalkInterval = component.m_randomTalkInterval,
            RandomTalkChance = component.m_randomTalkChance,
            RandomTalk = component.m_randomTalk,
            RandomTalkInFactionBase = component.m_randomTalkInFactionBase,
            RandomGreets = component.m_randomGreets,
            RandomGoodbye = component.m_randomGoodbye,
            PrivateAreaAlarm = component.m_privateAreaAlarm,
            Aggravated = component.m_aggravated
        };
    }

    public static void Write(GameObject critter, string folderPath)
    {
        if (!critter.TryGetComponent(out NpcTalk component)) return;
        NPCTalkData data = RecordData(component);
        string fileName = "NPCTalk.yml";
        string filePath = folderPath + Path.DirectorySeparatorChar + fileName;
        var serializer = new SerializerBuilder().Build();
        var serial = serializer.Serialize(data);
        File.WriteAllText(filePath, serial);
        
        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);

        string randomTalkFXPath = effectsFolder + Path.DirectorySeparatorChar + "RandomTalkFX.yml";
        string randomGreetFXPath = effectsFolder + Path.DirectorySeparatorChar + "RandomGreetFX.yml";
        string randomGoodbyeFXPath = effectsFolder + Path.DirectorySeparatorChar + "RandomGoodbyeFX.yml";

        Helpers.WriteEffectList(component.m_randomTalkFX, randomTalkFXPath);
        Helpers.WriteEffectList(component.m_randomGreetFX, randomGreetFXPath);
        Helpers.WriteEffectList(component.m_randomGoodbyeFX, randomGoodbyeFXPath);
    }

    public static void Read(string folderPath, ref CreatureData creatureData)
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
        if (!critter.GetComponent<MonsterAI>()) return;
        NPCTalkData data = creatureData.m_npcTalk;
        if (data.Name.IsNullOrWhiteSpace()) return;
        if (!critter.TryGetComponent(out NpcTalk component))
        {
            component = critter.AddComponent<NpcTalk>();
            m_newNPCTalkers[critter.name] = critter;
        }
        Vector3 scale = Helpers.GetScale(creatureData.m_scale);
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