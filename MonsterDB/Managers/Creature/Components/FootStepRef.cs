using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class FootStepRef : Reference
{
    public bool? m_footlessFootsteps;
    public float? m_footlessTriggerDistance;
    public float? m_footstepCullDistance;
    public List<StepEffectRef>? m_effects;
    
    public FootStepRef(){}

    public FootStepRef(FootStep comp) => Setup(comp);
}

[Serializable]
public class StepEffectRef : Reference
{
    public string m_name = "";
    public FootStep.MotionType m_motionType = FootStep.MotionType.Land;
    public FootStep.GroundMaterial m_material = FootStep.GroundMaterial.Default;
    public string[]? m_effectPrefabs;

    public StepEffectRef(){}
    
    public StepEffectRef(FootStep.StepEffect effect)
    {
        m_name = effect.m_name;
        m_motionType = effect.m_motionType;
        m_material = effect.m_material;
        m_effectPrefabs = effect.m_effectPrefabs.ToGameObjectNameArray();
    }
}

public static partial class Extensions
{
    public static List<StepEffectRef> ToStepEffectRefList(this List<FootStep.StepEffect> effects) =>
        effects.Select(f => new StepEffectRef(f)).ToList();

    public static List<FootStep.StepEffect> ToFootStepEffects(this List<StepEffectRef> effects, List<FootStep.StepEffect> defaultValues)
    {
        Dictionary<(string m_name, FootStep.MotionType m_motionType, FootStep.GroundMaterial m_material),
            FootStep.StepEffect> map = defaultValues.ToDictionary(x => (x.m_name, x.m_motionType, x.m_material));

        List<FootStep.StepEffect> output = new List<FootStep.StepEffect>();
        for (int i = 0; i < effects.Count; ++i)
        {
            StepEffectRef? reference = effects[i];
            if (reference.m_effectPrefabs == null) continue;
            (string, FootStep.MotionType, FootStep.GroundMaterial) key = (reference.m_name, reference.m_motionType, reference.m_material);
            if (!map.TryGetValue(key, out FootStep.StepEffect? effect))
            {
                effect = new FootStep.StepEffect
                {
                    m_name = reference.m_name,
                    m_motionType = reference.m_motionType,
                    m_material = reference.m_material,
                };
            }
            effect.m_effectPrefabs = CleanupFootStepEffects(reference.m_effectPrefabs.ToGameObjectArray());
            map[key] = effect;
            output.Add(effect);
        }

        return output;
    }

    private static GameObject[] CleanupFootStepEffects(GameObject[] effects)
    {
        List<GameObject> cleanedEffects = new List<GameObject>();

        for (int i = 0; i < effects.Length; ++i)
        {
            GameObject effect = effects[i];
            if (effect == null) continue;
            if (effect.GetComponent<ZNetView>())
            {
                string newName = $"{effect.name}_No_ZNetView";
                if (CloneManager.prefabs.TryGetValue(newName, out GameObject? prefab))
                {
                    cleanedEffects.Add(prefab);
                }
                else
                {
                    Clone clone = new Clone(effect, newName);
                    clone.OnCreated += p =>
                    {
                        p.Remove<ZNetView>();
                    };
                    prefab = clone.Create();
                    if (prefab == null) continue;
                    cleanedEffects.Add(prefab);
                }
            }
            else
            {
                cleanedEffects.Add(effect);
            }
        }
        
        return cleanedEffects.ToArray();
    }
}