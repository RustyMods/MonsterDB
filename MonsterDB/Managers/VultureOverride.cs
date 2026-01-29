using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public class VultureOverride : MonoBehaviour
{
    private static AnimatorOverrideController? overrideController;

    public void Start()
    {
        string? prefabName = Utils.GetPrefabName(name);
        if (LoadManager.originals.ContainsKey(prefabName))
        {
            Character? character = GetComponent<Character>();
            character.m_animator.runtimeAnimatorController = overrideController;
            MonsterDBPlugin.LogDebug($"Overriding {name} controller");
            bool land = character.IsTamed() || character.GetComponent<Growup>();
            if (land) character.Land();
            else character.TakeOff();
        }
    }
    
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
            AnimationClip[]? clips = controller.animationClips;
            for (int i = 0; i < clips.Length; ++i)
            {
                AnimationClip clip = clips[i];
                string name = clip.name.Replace("MOCK ", string.Empty);
                overrides.Add(originalClips.TryGetValue(name, out AnimationClip? anim)
                    ? new KeyValuePair<AnimationClip, AnimationClip>(clip, anim)
                    : new KeyValuePair<AnimationClip, AnimationClip>(clip, clip));
            }
            
            overrideController.ApplyOverrides(overrides);

            volture.AddComponent<VultureOverride>();
        }
    }
}