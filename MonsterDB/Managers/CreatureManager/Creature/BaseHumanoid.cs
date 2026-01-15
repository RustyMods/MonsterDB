using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseHumanoid : Base
{
    [YamlMember(Order = 6)] public HumanoidRef? Character;
    [YamlMember(Order = 7)] public MonsterAIRef? AI;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Humanoid? character = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        Type = BaseType.Humanoid;
        Character = new HumanoidRef();
        AI = new MonsterAIRef();
        Character.SetFrom(character);
        AI.SetFrom(ai);
    }

    protected override void UpdatePrefab(GameObject prefab, bool isInstance = false)
    {
        base.UpdatePrefab(prefab, isInstance);
        if (isInstance)
        {
            UpdateIHumanoid(prefab);
        }
        else
        {
            UpdateHumanoid(prefab);
        }
    }

    private void UpdateHumanoid(GameObject prefab)
    {
        Humanoid? humanoid = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        if (humanoid == null || ai == null) return;
        if (Character != null) humanoid.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);
    }

    private void UpdateIHumanoid(GameObject prefab)
    {
        Humanoid? IHumanoid = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        
        if (IHumanoid == null || ai == null) return;
        if (Character != null) IHumanoid.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);

        GameObject? source = PrefabManager.GetPrefab(Prefab);
        if (source == null) return;
        Humanoid? humanoid = source.GetComponent<Humanoid>();
        if (humanoid == null) return;

        IHumanoid.m_defaultItems = humanoid.m_defaultItems;
        IHumanoid.m_randomWeapon = humanoid.m_randomWeapon;
        IHumanoid.m_randomSets = humanoid.m_randomSets;

        IHumanoid.m_inventory.RemoveAll();
        IHumanoid.Start();
    }
}