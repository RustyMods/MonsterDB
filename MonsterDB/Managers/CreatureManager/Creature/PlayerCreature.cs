using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class PlayerCreature : BaseCreature
{
    [YamlMember(Order = 6)] public HumanoidRef? Character;
    [YamlMember(Order = 7)] public MonsterAIRef? AI;
    [YamlMember(Order = 8)] public PlayerVisualRef? Visuals;

    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        Human? character = prefab.GetComponent<Human>();
        MonsterAI? ai = prefab.GetComponent<MonsterAI>();
        base.Setup(prefab, isClone, source);
        Type = CreatureType.Human;
        Character = new HumanoidRef();
        AI = new MonsterAIRef();
        Visuals = new PlayerVisualRef();
        Character.ReferenceFrom(character);
        AI.ReferenceFrom(ai);
        Visuals.m_scale = prefab.transform.localScale;

        Renderer? renderer = null;
        if (prefab.TryGetComponent(out VisEquipment visEq))
        {
            renderer = visEq.m_bodyModel;
        }

        if (renderer != null)
        {
            Visuals.m_materials = renderer.sharedMaterials.ToRef();
        }
    }

    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        
        CreatureManager.Save(prefab,IsCloned, ClonedFrom);
        
        Human? human = prefab.GetComponent<Human>();
        VisEquipment? visEq = prefab.GetComponent<VisEquipment>();
        UpdateScale(prefab, human);
        UpdatePlayer(human);
        UpdateMaterials(visEq.m_bodyModel);
        UpdateCharacterDrop(prefab);
        UpdateGrowUp(prefab);
        UpdateHumanoid(human, prefab.GetComponent<MonsterAI>(), out bool attacksChanged);
        UpdateTameable(prefab);
        UpdateProcreation(prefab);
        UpdateNpcTalk(prefab);
        UpdateMovementDamage(prefab);
        
        List<Character>? characters = global::Character.GetAllCharacters();
        foreach (Character? c in characters)
        {
            string? prefabName = Utils.GetPrefabName(c.name);
            if (prefabName != Prefab) continue;
            if (c is not Human instanceHuman) continue;

            UpdateScale(c.gameObject, instanceHuman);
            UpdateInstanceHumanoid(instanceHuman, human, attacksChanged);
            UpdatePlayer(instanceHuman);
            UpdateMaterials(instanceHuman.GetComponent<VisEquipment>().m_bodyModel);
            UpdateCharacterDrop(c.gameObject);
            UpdateGrowUp(c.gameObject);
            UpdateTameable(c.gameObject);
            UpdateProcreation(c.gameObject);
            UpdateNpcTalk(c.gameObject);
            UpdateMovementDamage(c.gameObject);
        }
        
        base.Update();
    }

    private void UpdatePlayer(Human human)
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
    
    protected void UpdateMaterials(Renderer? renderer)
    {
        if (Visuals != null && Visuals.m_materials != null && renderer != null)
        {
            Dictionary<string, MaterialRef> dict = Visuals.m_materials
                .ToDictionary(f => f.m_name);
            Material[]? materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; ++i)
            {
                Material? mat = materials[i];
                if (mat == null) continue;
                if (!dict.TryGetValue(mat.name, out MaterialRef matRef)) continue;
                if (matRef.m_color != null && mat.HasProperty(ShaderRef._Color))
                {
                    mat.color = matRef.m_color.FromHex(mat.color);
                }
                if (matRef.m_hue != null && mat.HasProperty(ShaderRef._Hue))
                {
                    mat.SetFloat(ShaderRef._Hue, matRef.m_hue.Value);
                }
                if (matRef.m_saturation != null && mat.HasProperty(ShaderRef._Saturation))
                {
                    mat.SetFloat(ShaderRef._Saturation, matRef.m_saturation.Value);
                }
                if (matRef.m_value != null && mat.HasProperty(ShaderRef._Value))
                {
                    mat.SetFloat(ShaderRef._Value, matRef.m_value.Value);
                }
                if (matRef.m_emissionColor != null && mat.HasProperty(ShaderRef._EmissionColor))
                {
                    mat.SetColor(ShaderRef._EmissionColor, matRef.m_emissionColor.FromHex(mat.GetColor(ShaderRef._EmissionColor)));
                }
                if (matRef.m_mainTexture != null && matRef.m_mainTexture != mat.mainTexture?.name)
                {
                    mat.mainTexture = TextureManager.GetTexture(matRef.m_mainTexture, mat.mainTexture);;
                }
            }
            renderer.materials = materials;
            renderer.sharedMaterials = materials;
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