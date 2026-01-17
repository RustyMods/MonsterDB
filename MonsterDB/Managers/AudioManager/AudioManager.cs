using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace MonsterDB;

public static class AudioManager
{
    public static readonly Dictionary<string, AudioRef> clips;

    static AudioManager()
    {
        clips = new Dictionary<string, AudioRef>();
    }

    public static void Start()
    {
        HashSet<string> audioExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".wav", ".mp3", ".ogg", ".flac", ".aac", ".m4a", ".wma", ".aiff", ".aif"
        };
        
        string[] files = Directory.GetFiles(FileManager.ImportFolder, "*", SearchOption.AllDirectories)    
            .Where(f => audioExtensions.Contains(Path.GetExtension(f)))
            .ToArray();
        
        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            _ = new AudioRef(filePath);
        }
    }
    
    public static AudioClip? ReadAudioFile(string file)
    {
        using UnityWebRequest webRequest = UnityWebRequestMultimedia.GetAudioClip("file:///" + file.Replace("\\","/"), AudioType.UNKNOWN);
        webRequest.SendWebRequest();
        while (!webRequest.isDone)
        {
        }

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            MonsterDBPlugin.LogWarning("Failed to load audio file: " + webRequest.error);
        }
        else
        {
            DownloadHandlerAudioClip downloadHandlerAudioClip = (DownloadHandlerAudioClip)webRequest.downloadHandler;
            AudioClip clip = downloadHandlerAudioClip.audioClip;
            clip.name = Path.GetFileNameWithoutExtension(file);
            MonsterDBPlugin.LogDebug("Successfully loaded audio: " + clip.name);
            return clip;
        }

        return null;
    }
}