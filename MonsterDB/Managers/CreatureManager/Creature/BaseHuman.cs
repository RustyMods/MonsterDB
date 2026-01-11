using System;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class BaseHuman : BaseHumanoid
{
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        SetupVersions();
        SetupSharedFields(prefab, isClone, source);
        
        Human? character = prefab.GetComponent<Human>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        Type = BaseType.Human;
        Character = new HumanoidRef();
        AI = new MonsterAIRef();
        Character.SetFrom(character);
        AI.SetFrom(ai);
        Visuals?.SetDefaultHumanFields();
        
        if (isClone) SetupSpawnData();
    }

    protected override void UpdatePrefab(GameObject prefab, bool isInstance = false)
    {
        base.UpdatePrefab(prefab, isInstance);
        UpdateHuman(prefab);
    }

    private void UpdateHuman(GameObject prefab)
    {
        Human? human = prefab.GetComponent<Human>();
        if (Visuals == null) return;
        if (Visuals.m_beards != null)
        {
            human.m_beards = Visuals.m_beards;
        }

        if (Visuals.m_hairs != null)
        {
            human.m_hairs = Visuals.m_hairs;
        }

        if (Visuals.m_modelIndex != null)
        {
            human.m_models = Visuals.m_modelIndex;
        }

        if (Visuals.m_skinColors != null)
        {
            human.m_skinColors = Visuals.m_skinColors;
        }
        if (Visuals.m_hairColors != null)
        {
            human.m_hairColors = Visuals.m_hairColors;
        }
    }
}