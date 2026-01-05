using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class CharacterCreature : Creature
{
    [YamlMember(Order = 6)] public CharacterRef? Character;
    [YamlMember(Order = 7)] public AnimalAIRef? AI;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        Character? character = prefab.GetComponent<Character>();
        AnimalAI? ai = prefab.GetComponent<AnimalAI>();
        base.Setup(prefab, isClone, source);
        Type = CreatureType.Humanoid;
        Character = new();
        AI = new();
        Character.ReferenceFrom(character);
        AI.ReferenceFrom(ai);
    }
    
    public override void Update()
    {
        GameObject? prefab = PrefabManager.GetPrefab(Prefab);
        if (prefab == null) return;
        
        CreatureManager.Save(prefab, IsCloned, ClonedFrom);
        
        Character? character = prefab.GetComponent<Character>();
        UpdateScale(prefab, character);
        UpdateCharacter(character, prefab.GetComponent<AnimalAI>());
        UpdateLevelEffects(prefab.GetComponentInChildren<LevelEffects>(), out Renderer? mainRenderer);
        if (mainRenderer == null && prefab.TryGetComponent(out VisEquipment visEq)) mainRenderer = visEq.m_bodyModel;
        if (mainRenderer == null) mainRenderer = prefab.GetComponentInChildren<SkinnedMeshRenderer>();
        UpdateMaterials(mainRenderer);
        UpdateCharacterDrop(prefab);
        UpdateGrowUp(prefab);
        UpdateTameable(prefab);
        UpdateProcreation(prefab);
        UpdateNpcTalk(prefab);
        UpdateMovementDamage(prefab);
        UpdateSaddle(prefab);
        
        List<Character>? characters = global::Character.GetAllCharacters();
        foreach (Character? c in characters)
        {
            string? prefabName = Utils.GetPrefabName(c.name);
            if (prefabName != Prefab) continue;
            UpdateScale(c.gameObject, c);
            UpdateCharacter(c, c.GetBaseAI() as AnimalAI);
            UpdateLevelEffects(c.GetComponentInChildren<LevelEffects>(), out Renderer? instancedRenderer);
            if (instancedRenderer == null && c.TryGetComponent(out VisEquipment instanceVisEq)) instancedRenderer = instanceVisEq.m_bodyModel;
            if (instancedRenderer == null) instancedRenderer = c.GetComponentInChildren<SkinnedMeshRenderer>();
            UpdateMaterials(instancedRenderer);
            UpdateCharacterDrop(c.gameObject);
            UpdateGrowUp(c.gameObject);
            UpdateTameable(c.gameObject);
            UpdateProcreation(c.gameObject);
            UpdateNpcTalk(c.gameObject);
            UpdateMovementDamage(c.gameObject);
            UpdateSaddle(c.gameObject);
        }
        
        base.Update();
    }
    
    private void UpdateCharacter(Character character, AnimalAI? ai)
    {
        if (character == null || ai == null) return;
        if (Character != null) character.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);
    } 
}