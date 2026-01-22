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
        Humanoid? character = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        if (character == null || ai == null) return;
        base.Setup(prefab, isClone, source);
        Type = BaseType.Humanoid;
        Character = new HumanoidRef(character);
        AI = new MonsterAIRef(ai);
    }
    
    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseHumanoid originalCharacter) return;
        if (Character != null && originalCharacter.Character != null) Character.ResetTo(originalCharacter.Character);
        if (AI != null && originalCharacter.AI != null) AI.ResetTo(originalCharacter.AI);
    }

    public override void Update()
    {
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
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
        if (Character != null)
        {
            Character.UpdateFields(humanoid, prefab.name, true);
        }

        if (AI != null)
        {
            AI.UpdateFields(ai, prefab.name, true);
        }
    }

    private void UpdateIHumanoid(GameObject prefab)
    {
        Humanoid? IHumanoid = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        
        if (IHumanoid == null || ai == null) return;
        if (Character != null)
        {
            Character.UpdateFields(IHumanoid, prefab.name, false);
        }

        if (AI != null)
        {
            AI.UpdateFields(ai, prefab.name, false);
        }

        GameObject? source = PrefabManager.GetPrefab(Prefab);
        if (source == null) return;
        Humanoid? humanoid = source.GetComponent<Humanoid>();
        if (humanoid == null) return;

        IHumanoid.m_defaultItems = humanoid.m_defaultItems;
        IHumanoid.m_randomWeapon = humanoid.m_randomWeapon;
        IHumanoid.m_randomSets = humanoid.m_randomSets;
        IHumanoid.m_randomArmor = humanoid.m_randomArmor;
        IHumanoid.m_randomShield =  humanoid.m_randomShield;

        IHumanoid.m_inventory.RemoveAll();
        IHumanoid.Start();
    }
}