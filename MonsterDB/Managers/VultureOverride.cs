using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

public static class VultureOverride
{
    public static AnimatorOverrideController? overrideController;
    private static readonly List<string> Vultures = new List<string>()
    {
        "Volture"
    };

    [HarmonyPatch(typeof(Character), nameof(Character.Awake))]
    private static class Character_Awake
    {
        private static void Postfix(Character __instance)
        {
            string? prefabName = Utils.GetPrefabName(__instance.name);
            if (!ShouldOverride(prefabName) || !LoadManager.originals.ContainsKey(prefabName)) return;
            __instance.m_animator.runtimeAnimatorController = overrideController;
            __instance.Land();
            MonsterDBPlugin.LogDebug($"Overriding {__instance.name} controller");
        }
    }
    
    public static void Register(string prefab)
    {
        if (Vultures.Contains(prefab)) return;
        Vultures.Add(prefab);
    }
    
    public static bool ShouldOverride(string prefab) => Vultures.Contains(prefab);
    
    public static void Setup()
    {
        GameObject? volture = PrefabManager.GetPrefab("Volture");
        if (volture == null) return;

        Animator? animator = volture.GetComponentInChildren<Animator>();
        RuntimeAnimatorController? originalController = animator.runtimeAnimatorController;
        
        GameObject? prefab = AssetBundleManager.LoadAsset<GameObject>("volture_fix", "Volture_temp");
        if (prefab == null)
        {
            MonsterDBPlugin.LogWarning("Volture_temp not found");
            return;
        }

        if (!prefab.TryGetComponent(out Animator temp))
        {
            MonsterDBPlugin.LogWarning("animator not found");
            return;
        }

        RuntimeAnimatorController? controller = temp.runtimeAnimatorController;
        if (controller != null)
        {
            overrideController = new AnimatorOverrideController(controller);
            
            Dictionary<string, AnimationClip> originalClips = new Dictionary<string, AnimationClip>();
            foreach (AnimationClip clip in originalController.animationClips)
            {
                if (clip != null)
                {
                    originalClips[clip.name] = clip;
                }
            }

            List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(overrides);
        
            List<KeyValuePair<AnimationClip, AnimationClip>> newOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            
            foreach (KeyValuePair<AnimationClip, AnimationClip> clip in overrides)
            {
                if (clip.Key != null)
                {
                    string lookUp = clip.Key.name.Replace("MOCK ", string.Empty);
                    if (originalClips.TryGetValue(lookUp, out var anim))
                    {
                        newOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip.Key, anim));
                    }
                    else
                    {
                        newOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(clip.Key, clip.Value));
                    }
                }
            }
            
            overrideController.ApplyOverrides(newOverrides);
        }
    }
}