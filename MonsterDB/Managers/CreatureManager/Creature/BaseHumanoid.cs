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
        base.Setup(prefab, isClone, source);
        Type = BaseType.Humanoid;
        Character = new HumanoidRef();
        AI = new MonsterAIRef();
        Character.ReferenceFrom(character);
        Character.m_attacks = character.GetAttacks().ToRef();
        AI.ReferenceFrom(ai);
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        
        CreatureManager.Save(prefab,IsCloned, ClonedFrom);
        
        Humanoid? humanoid = prefab.GetComponent<Humanoid>();
        UpdateScale(prefab, humanoid);
        UpdateLevelEffects(prefab);
        UpdateVisual(prefab);
        UpdateCharacterDrop(prefab);
        UpdateGrowUp(prefab);
        UpdateHumanoid(humanoid, prefab.GetComponent<MonsterAI>(), out bool attacksChanged);
        UpdateTameable(prefab);
        UpdateProcreation(prefab);
        UpdateNpcTalk(prefab);
        UpdateMovementDamage(prefab);
        UpdateSaddle(prefab);
        UpdateDropProjectile(prefab);
        UpdateCinderSpawner(prefab);
        UpdateTimedDestruction(prefab);
        UpdateRandomAnimation(prefab);

        List<Character>? characters = global::Character.GetAllCharacters();
        foreach (Character? c in characters)
        {
            string? prefabName = Utils.GetPrefabName(c.name);
            if (prefabName != Prefab) continue;
            if (c is not Humanoid instanceHumanoid) continue;
            UpdateScale(c.gameObject, instanceHumanoid);
            UpdateInstanceHumanoid(instanceHumanoid, humanoid, attacksChanged);
            UpdateLevelEffects(c.gameObject);
            UpdateVisual(c.gameObject);
            UpdateCharacterDrop(c.gameObject);
            UpdateGrowUp(c.gameObject);
            UpdateTameable(c.gameObject);
            UpdateProcreation(c.gameObject);
            UpdateNpcTalk(c.gameObject);
            UpdateMovementDamage(c.gameObject);
            UpdateSaddle(c.gameObject);
            UpdateDropProjectile(c.gameObject);
            UpdateCinderSpawner(c.gameObject);
            UpdateTimedDestruction(c.gameObject);
            UpdateRandomAnimation(c.gameObject);
        }
        base.Update();
    }
    
    private void UpdateHumanoid(Humanoid humanoid, MonsterAI ai, out bool attacksChanged)
    {
        attacksChanged = false;
        if (humanoid == null || ai == null) return;
        if (Character != null) humanoid.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);

        attacksChanged = UpdateAttacks(humanoid);
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
    
    private void UpdateInstanceHumanoid(Humanoid instanceHumanoid,  Humanoid humanoid, bool attacksChanged)
    {
        MonsterAI? instanceAI = instanceHumanoid.GetBaseAI() as MonsterAI;
        
        if (instanceHumanoid == null || instanceAI == null) return;

        if (Character != null) instanceHumanoid.SetFieldsFrom(Character);
        if (AI != null) instanceAI.SetFieldsFrom(AI);

        if (attacksChanged)
        {
            instanceHumanoid.m_defaultItems = humanoid.m_defaultItems;
            instanceHumanoid.m_randomWeapon = humanoid.m_randomWeapon;
            instanceHumanoid.m_randomSets = humanoid.m_randomSets;

            instanceHumanoid.m_inventory.RemoveAll();
            instanceHumanoid.GiveDefaultItems();
        }
    }
    
}