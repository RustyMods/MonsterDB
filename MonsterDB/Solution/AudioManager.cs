using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Networking;

namespace MonsterDB.Solution;

public static class AudioManager
{
    private static readonly string CustomAudioFolderPath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "CustomAudio";
    private static readonly Dictionary<string, AudioClip> CustomAudio = new();
    private static GameObject sfx_Bonemass_idle = null!;
    private static readonly Dictionary<string, GameObject> RegisteredSFX = new();

    [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
    private static class ZNetScene_Awake_Patch
    {
        private static void Postfix(ZNetScene __instance)
        {
            sfx_Bonemass_idle = __instance.GetPrefab("sfx_Bonemass_idle");
            if (!sfx_Bonemass_idle) return;
            foreach (var kvp in CustomAudio) Create(kvp.Key, kvp.Value);
        }
    }

    private static void Create(string name, AudioClip clip)
    {
        GameObject clone = Object.Instantiate(sfx_Bonemass_idle, MonsterDBPlugin.m_root.transform, false);
        clip.name = name;
        if (!clone.TryGetComponent(out ZSFX component)) return;
        component.m_captionType = ClosedCaptions.CaptionType.Enemy;
        component.m_closedCaptionToken = name;
        component.m_secondaryCaptionToken = "";
        component.m_audioClips = new AudioClip[] { clip };
        Methods.Helpers.RegisterToZNetScene(clone);
        RegisteredSFX[name] = clone;
    }

    public static void Setup()
    {
        if (!Directory.Exists(CreatureManager.m_folderPath)) Directory.CreateDirectory(CreatureManager.m_folderPath);
        if (!Directory.Exists(CustomAudioFolderPath)) Directory.CreateDirectory(CustomAudioFolderPath);
        string[] files = Directory.GetFiles(CustomAudioFolderPath);
        foreach (string file in files) GetCustomAudio(file);
    }
    
    private static void GetCustomAudio(string file)
    {
        using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip("file:///" + file.Replace("\\","/"), AudioType.UNKNOWN);
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
        }

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Failed to load audio file: " + webRequest.error);
        }
        else
        {
            DownloadHandlerAudioClip downloadHandlerAudioClip = (DownloadHandlerAudioClip)webRequest.downloadHandler;
            AudioClip clip = downloadHandlerAudioClip.audioClip;
            clip.name = Path.GetFileNameWithoutExtension(file);
            CustomAudio[clip.name] = clip;
            MonsterDBPlugin.MonsterDBLogger.LogDebug("Successfully added audio: " + clip.name);
        }
    }
}