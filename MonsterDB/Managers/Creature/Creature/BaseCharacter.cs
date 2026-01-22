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
        if (character == null || ai == null) return;
        base.Setup(prefab, isClone, source);
        Type = BaseType.Character;
        Character = new CharacterRef(character);
        AI = new AnimalAIRef(ai);
    }

    public override void Update()
    {
        base.Update();
        LoadManager.files.PrefabToUpdate = Prefab;
        LoadManager.files.Add(this);
    }

    public override void CopyFields(Header original)
    {
        base.CopyFields(original);
        if (original is not BaseCharacter originalCharacter) return;
        if (Character != null && originalCharacter.Character != null) Character.ResetTo(originalCharacter.Character);
        if (AI != null && originalCharacter.AI != null) AI.ResetTo(originalCharacter.AI);
    }

    protected override void UpdatePrefab(GameObject prefab, bool isInstance = false)
    {
        base.UpdatePrefab(prefab, isInstance);
        UpdateCharacter(prefab, isInstance);
        UpdateAnimalTameable(prefab, isInstance);
    }

    private void UpdateCharacter(GameObject prefab, bool isInstance = false)
    {
        Character? character = prefab.GetComponent<Character>();
        AnimalAI? ai = character.GetComponent<AnimalAI>();
        if (character == null || ai == null) return;
        if (Character != null)
        {
            Character.UpdateFields(character, prefab.name, !isInstance);
        }

        if (AI != null)
        {
            AI.UpdateFields(ai, prefab.name, !isInstance);
        }
    }

    private void UpdateAnimalTameable(GameObject prefab, bool isInstance = false)
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
            AI.UpdateFields(animalTameable, prefab.name, !isInstance);
        }
    }
}