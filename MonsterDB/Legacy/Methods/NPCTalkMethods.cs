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
}