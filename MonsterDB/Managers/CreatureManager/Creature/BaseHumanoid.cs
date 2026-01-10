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

    [YamlIgnore, NonSerialized] private bool updateAttacks;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        base.Setup(prefab, isClone, source);
        Humanoid? character = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        Type = BaseType.Humanoid;
        Character = new HumanoidRef();
        AI = new MonsterAIRef();
        Character.SetFrom(character);
        Character.m_attacks = character.GetAttacks().ToRef();
        AI.SetFrom(ai);
    }

    protected override void UpdatePrefab(GameObject prefab, bool isInstance = false)
    {
        if (isInstance)
        {
            UpdateIHumanoid(prefab);
        }
        else
        {
            UpdateHumanoid(prefab);
        }
        base.UpdatePrefab(prefab, isInstance);
    }

    private bool UpdateAttacks(Humanoid humanoid)
    {
        if (Character == null) return false;
        ItemDataSharedRef[]? items = Character.GetAttacks();
        if (items != null)
        {
            bool attacksChanged = false;
            Dictionary<string, GameObject> attacks = GetDefaultItems(humanoid);
            foreach (ItemDataSharedRef attack in items)
            {
                if (!attacks.TryGetValue(attack.m_prefab, out GameObject? item)) continue;
                if (!item.TryGetComponent(out ItemDrop component)) continue;
                component.m_itemData.m_shared.SetFieldsFrom(attack);
                attacksChanged = true;
                if (component.m_itemData.m_shared.m_attack.m_attackProjectile != null &&
                    component.m_itemData.m_shared.m_attack.m_attackProjectile.TryGetComponent(out Projectile prj))
                {
                    if (attack.m_attack != null && attack.m_attack.m_projectileRef != null && attack.m_attack.m_projectileRef.m_prefab == prj.name)
                    {
                        prj.SetFieldsFrom(attack.m_attack.m_projectileRef);
                    }
                }
            }
            return attacksChanged;
        }

        return false;
    }

    private void UpdateHumanoid(GameObject prefab)
    {
        Humanoid? humanoid = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = humanoid.GetComponent<MonsterAI>();
        if (humanoid == null || ai == null) return;
        if (Character != null) humanoid.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);
        updateAttacks = UpdateAttacks(humanoid);
    }

    private void UpdateIHumanoid(GameObject prefab)
    {
        Humanoid? IHumanoid = prefab.GetComponent<Humanoid>();
        MonsterAI? ai = IHumanoid.GetBaseAI() as MonsterAI;
        
        if (IHumanoid == null || ai == null) return;
        if (Character != null) IHumanoid.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);

        if (updateAttacks)
        {
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
}