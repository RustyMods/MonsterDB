using System;
using System.Collections.Generic;
using UnityEngine;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseCharacter : Base
{
    [YamlMember(Order = 6)] public CharacterRef? Character;
    [YamlMember(Order = 7)] public AnimalAIRef? AI;
    
    public override void Setup(GameObject prefab, bool isClone = false, string source = "")
    {
        Character? character = prefab.GetComponent<Character>();
        AnimalAI? ai = prefab.GetComponent<AnimalAI>();
        base.Setup(prefab, isClone, source);
        Type = BaseType.Character;
        Character = new();
        AI = new();
        Character.SetFrom(character);
        AI.SetFrom(ai);
    }

    protected override void UpdatePrefab(GameObject prefab, bool isInstance = false)
    {
        base.UpdatePrefab(prefab, isInstance);
        UpdateCharacter(prefab);
        UpdateAnimalTameable(prefab);
    }

    private void UpdateCharacter(GameObject prefab)
    {
        Character? character = prefab.GetComponent<Character>();
        AnimalAI? ai = character.GetComponent<AnimalAI>();
        if (character == null || ai == null) return;
        if (Character != null) character.SetFieldsFrom(Character);
        if (AI != null) ai.SetFieldsFrom(AI);
    }

    private void UpdateAnimalTameable(GameObject prefab)
    {
        if (AI == null) return;
        
        bool hasTameableComponent = prefab.GetComponent<Tameable>();

        if (!hasTameableComponent)
        {
            prefab.Remove<AnimalTameable>();
        }
        else
        {
            if (!prefab.TryGetComponent(out AnimalTameable animalTameable))
            {
                animalTameable = prefab.AddComponent<AnimalTameable>();
            }
            animalTameable.SetFieldsFrom(AI);
        }
    }
}