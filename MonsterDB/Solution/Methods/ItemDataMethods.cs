using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.Methods.Helpers;

namespace MonsterDB.Solution.Methods;

public static class ItemDataMethods
{
    private static readonly string m_folderPath = CreatureManager.m_folderPath + Path.DirectorySeparatorChar + "Items";
    public static readonly Dictionary<string, GameObject> m_clonedItems = new();
    public static void Write(GameObject item, string originalPrefab = "")
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (item == null) return;
        if (!item.TryGetComponent(out ItemDrop component)) return;
        ISerializer serializer = new SerializerBuilder().Build();
        
        ItemDrop.ItemData.SharedData shared = component.m_itemData.m_shared;
        AttackData data = HumanoidMethods.RecordAttackData(component);

        data.OriginalPrefab = originalPrefab;

        string serial = serializer.Serialize(data);
        string folderPath = m_folderPath + Path.DirectorySeparatorChar + item.name;
        if (Directory.Exists(folderPath)) return;
        
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);
        string filePath = folderPath + Path.DirectorySeparatorChar + "ItemData.yml";
        File.WriteAllText(filePath, serial);

        string effectsFolder = folderPath + Path.DirectorySeparatorChar + "Effects";
        if (!Directory.Exists(effectsFolder)) Directory.CreateDirectory(effectsFolder);
        
        string hitEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitEffects.yml";
        string hitTerrainEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HitTerrainEffects.yml";
        string blockEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "BlockEffects.yml";
        string startEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "StartEffects.yml";
        string holdStartEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "HoldStartEffects.yml";
        string equipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "EquipEffects.yml";
        string unequipEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "UnequipEffects.yml";
        string triggerEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TriggerEffects.yml";
        string trailStartEffectsPath = effectsFolder + Path.DirectorySeparatorChar + "TrailStartEffects.yml";
        
        WriteEffectList(shared.m_hitEffect, hitEffectsPath);
        WriteEffectList(shared.m_hitTerrainEffect, hitTerrainEffectsPath);
        WriteEffectList(shared.m_blockEffect, blockEffectsPath);
        WriteEffectList(shared.m_startEffect, startEffectsPath);
        WriteEffectList(shared.m_holdStartEffect, holdStartEffectsPath);
        WriteEffectList(shared.m_equipEffect, equipEffectsPath);
        WriteEffectList(shared.m_unequipEffect, unequipEffectsPath);
        WriteEffectList(shared.m_triggerEffect, triggerEffectsPath);
        WriteEffectList(shared.m_trailStartEffect, trailStartEffectsPath);
        
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Wrote ItemData to file: ");
        MonsterDBPlugin.MonsterDBLogger.LogDebug(folderPath);
    }

    public static void Clone(GameObject item, string name, bool write = true, bool register = false)
    {
        if (item == null) return;
        if (!item.GetComponent<ItemDrop>()) return;
        GameObject clone = Object.Instantiate(item, MonsterDBPlugin.m_root.transform, false);
        clone.name = name;
        RegisterToObjectDB(clone);
        if (register) RegisterToZNetScene(clone);
        m_clonedItems[clone.name] = clone;
        if (write)
        {
            Write(clone, item.name);
        }
    }
}