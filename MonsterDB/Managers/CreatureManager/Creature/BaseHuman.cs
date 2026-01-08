using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseHuman : Base
{
    [YamlMember(Order = 6)] public HumanoidRef? Character;
    [YamlMember(Order = 7)] public MonsterAIRef? AI;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        Human? character = prefab.GetComponent<Human>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        base.Setup(prefab, isClone, source);
        Type = BaseType.Human;
        Character = new HumanoidRef();
        AI = new MonsterAIRef();
        Character.ReferenceFrom(character);
        AI.ReferenceFrom(ai);
        Visuals?.SetDefaultHumanFields();
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        
        CreatureManager.Save(prefab,IsCloned, ClonedFrom);
        
        Human? human = prefab.GetComponent<Human>();
        UpdateScale(prefab, human);
        UpdateHuman(human);
        UpdateVisual(prefab);
        UpdateCharacterDrop(prefab);
        UpdateGrowUp(prefab);
        UpdateHumanoid(human, prefab.GetComponent<MonsterAI>(), out bool attacksChanged);
        UpdateTameable(prefab);
        UpdateProcreation(prefab);
        UpdateNpcTalk(prefab);
        UpdateMovementDamage(prefab);
        UpdateDropProjectile(prefab);
        UpdateCinderSpawner(prefab);
        UpdateTimedDestruction(prefab);
        UpdateRandomAnimation(prefab);
        
        List<Character>? characters = global::Character.GetAllCharacters();
        foreach (Character? c in characters)
        {
            string? prefabName = Utils.GetPrefabName(c.name);
            if (prefabName != Prefab) continue;
            if (c is not Human instanceHuman) continue;

            UpdateScale(c.gameObject, instanceHuman);
            UpdateInstanceHumanoid(instanceHuman, human, attacksChanged);
            UpdateHuman(instanceHuman);
            UpdateVisual(c.gameObject);
            UpdateCharacterDrop(c.gameObject);
            UpdateGrowUp(c.gameObject);
            UpdateTameable(c.gameObject);
            UpdateProcreation(c.gameObject);
            UpdateNpcTalk(c.gameObject);
            UpdateMovementDamage(c.gameObject);
            UpdateDropProjectile(c.gameObject);
            UpdateCinderSpawner(c.gameObject);
            UpdateTimedDestruction(c.gameObject);
            UpdateRandomAnimation(c.gameObject);
        }
        
        base.Update();
    }

    private void UpdateHuman(Human human)
    {
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
    
    protected void UpdateVisual(GameObject prefab)
    {
        if (Visuals != null)
        {
            Visuals.UpdateRenderers(prefab);
        }
    }
    
    protected void UpdateScale(GameObject prefab, Character character)
    {
        if (Visuals == null || !Visuals.m_scale.HasValue) return;
        prefab.transform.localScale = Visuals.m_scale.Value;
        if (character != null)
        {
            foreach (EffectList.EffectData? effect in character.m_deathEffects.m_effectPrefabs)
            {
                if (effect.m_prefab != null && effect.m_prefab.TryGetComponent(out Ragdoll doll))
                {
                    doll.transform.localScale = Visuals.m_scale.Value;
                    break;
                }
            }
        }
    }
}