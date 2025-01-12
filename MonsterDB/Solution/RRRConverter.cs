using System.Collections.Generic;
using System.IO;
using BepInEx;
using JetBrains.Annotations;
using MonsterDB.Solution.Methods;
using Newtonsoft.Json;
using UnityEngine;
using YamlDotNet.Serialization;
using static MonsterDB.Solution.CreatureManager;

namespace MonsterDB.Solution;

public static class RRRConverter
{
    public static void ReadAllRRRFiles()
    {
        if (!ZNetScene.instance || !ObjectDB.instance) return;
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_importFolderPath)) Directory.CreateDirectory(m_importFolderPath);
        foreach (string file in Directory.GetFiles(m_importFolderPath, "*.json"))
        {
            ReadRRRFile(file);
        }
    }

    private static void ReadRRRFile(string filePath)
    {
        if (!File.Exists(filePath)) return;
        var data = File.ReadAllText(filePath);
        RRRFormat? info = JsonConvert.DeserializeObject<RRRFormat>(data, new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        });
        if (info is null) return;
        if (!Convert(info, out CreatureData creatureData))
        {
            MonsterDBPlugin.MonsterDBLogger.LogWarning("Failed to convert " + Path.GetFileName(filePath));
            return;
        }
        Export(creatureData);
    }

    private static void Export(CreatureData data)
    {
        if (!Directory.Exists(m_folderPath)) Directory.CreateDirectory(m_folderPath);
        if (!Directory.Exists(m_importFolderPath)) Directory.CreateDirectory(m_importFolderPath);
        ISerializer serializer = new SerializerBuilder().Build();
        string serial = serializer.Serialize(data);
        string filePath = m_importFolderPath + Path.DirectorySeparatorChar + data.m_characterData.PrefabName + ".yml";
        File.WriteAllText(filePath, serial);
        MonsterDBPlugin.MonsterDBLogger.LogDebug("Exported creature data: ");
        MonsterDBPlugin.MonsterDBLogger.LogDebug(filePath);
    }

    private static bool Convert(RRRFormat? json, out CreatureData data)
    {
        data = new CreatureData();
        var original = json?.sOriginalPrefabName ?? "";
        if (original.StartsWith("MLNPC")) original = "Human";
        if (original.IsNullOrWhiteSpace()) return false;
        GameObject prefab = ZNetScene.instance.GetPrefab(original);
        if (!prefab) return false;
        if (json?.Category_Appearance != null) ConvertAppearance(json.Category_Appearance, ref data);
        ConvertCharacter(json, ref data);
        if (json?.Category_Humanoid != null)
        {
            ConvertItemArray(json.Category_Humanoid.aRandomArmor, ref data.m_randomArmors);
            ConvertItemArray(json.Category_Humanoid.aRandomShield, ref data.m_randomShields);
            ConvertEffects(json.Category_Humanoid, ref data);
            // Advanced attacks allow to manipulate attack data, thus only should affect items that are weapons, ie defaultItems and randomSets
            data.m_defaultItems.Clear();
            data.m_randomSets.Clear();
            Dictionary<string, ItemAttackData> advancedItems = new();
            ConvertAdvancedAttacks(json.Category_Humanoid.aAdvancedCustomAttacks, json.Category_Humanoid.aDefaultItems, data.m_characterData.PrefabName, ref data.m_defaultItems, ref advancedItems);
            ConvertRandomSetArray(json.Category_Humanoid.aAdvancedCustomAttacks, json.Category_Humanoid.aaRandomSets, data.m_characterData.PrefabName, ref data.m_randomSets, ref advancedItems);
        }
        ConvertBaseAI(json?.Category_BaseAI, ref data.m_monsterAIData, prefab, ref data);
        ConvertBaseAI(json?.Category_BaseAI, ref data.m_animalAIData, prefab, ref data);
        if (json?.Category_MonsterAI != null) ConvertMonsterAI(json.Category_MonsterAI, ref data.m_monsterAIData);
        if (json?.Category_CharacterDrops != null) ConvertCharacterDrops(json.Category_CharacterDrops, ref data.m_characterDrops);
        data.m_humanData.ModelIndex = json?.Category_NpcOnly?.bIsFemale ?? false ? 1 : 0;
        return true;
    }

    private static void ConvertAdvancedAttacks(List<AdvancedCustomAttacks>? attacks, List<string>? itemList, string creatureName, ref List<ItemAttackData> data, ref Dictionary<string, ItemAttackData> advancedItems)
    {
        if (itemList == null) return;
        foreach (var itemName in itemList)
        {
            if (itemName.StartsWith("@"))
            {
                try
                {
                    if (attacks == null) continue;
                    if (!int.TryParse(itemName.Substring(1), out int i)) continue;
                    AdvancedCustomAttacks? attack = attacks[i];
                    CreateAdvancedCustomAttack(attack, creatureName, i, ref data, ref advancedItems);
                }
                catch
                {
                    MonsterDBPlugin.MonsterDBLogger.LogDebug($"Failed to convert advanced custom attack: {creatureName} - {itemName}");
                }
            }
            else
            {
                var prefab = DataBase.TryGetGameObject(itemName);
                if (prefab == null) continue;
                if (!ItemDataMethods.Save(prefab, out ItemAttackData itemData)) continue;
                data.Add(itemData);
            }
        }
    }

    private static bool CreateAdvancedCustomAttack(AdvancedCustomAttacks? attack, string creatureName, int index, ref List<ItemAttackData> data, ref Dictionary<string, ItemAttackData> advancedItems)
    {
        if (attack?.sOriginalPrefabName == null) return false;
        GameObject? prefab = DataBase.TryGetGameObject(attack.sOriginalPrefabName);
        if (prefab == null) return false;
        var name = creatureName + "_" + prefab.name + "_@" + index;
        if (advancedItems.TryGetValue(name, out ItemAttackData advancedData))
        {
            data.Add(advancedData);
        }
        else
        {
            GameObject clone = CloneAttack(prefab, name);
            if (!clone.TryGetComponent(out ItemDrop component)) return false;
            component.m_itemData.m_shared.m_attack.m_attackAnimation = attack.sTargetAnim;
            component.m_itemData.m_shared.m_aiAttackInterval = attack.fAIAttackInterval;
            component.m_itemData.m_shared.m_damages = new HitData.DamageTypes()
            {
                m_damage = attack.dtAttackDamageOverride?.fDamage ?? 0f,
                m_blunt = attack.dtAttackDamageOverride?.fBlunt ?? 0f,
                m_slash = attack.dtAttackDamageOverride?.fSlash ?? 0f,
                m_pierce = attack.dtAttackDamageOverride?.fPierce ?? 0f,
                m_chop = attack.dtAttackDamageOverride?.fChop ?? 0f,
                m_pickaxe = attack.dtAttackDamageOverride?.fPickaxe ?? 0f,
                m_fire = attack.dtAttackDamageOverride?.fFire ?? 0f,
                m_frost = attack.dtAttackDamageOverride?.fFrost ?? 0f,
                m_lightning = attack.dtAttackDamageOverride?.fLightning ?? 0f,
                m_poison = attack.dtAttackDamageOverride?.fPoison ?? 0f,
                m_spirit = attack.dtAttackDamageOverride?.fSpirit ?? 0f
            };
            if (attack.sAttackProjectileOverride != null)
            {
                if (!attack.sAttackProjectileOverride.IsNullOrWhiteSpace())
                {
                    var projectile = DataBase.TryGetGameObject(attack.sAttackProjectileOverride);
                    if (projectile != null)
                    {
                        component.m_itemData.m_shared.m_attack.m_attackProjectile = projectile;
                    }
                }
            }

            if (attack.aHitEffects != null) UpdateEffectList(attack.aHitEffects, ref component.m_itemData.m_shared.m_attack.m_hitEffect);
            if (attack.aTriggerEffects != null) UpdateEffectList(attack.aTriggerEffects, ref component.m_itemData.m_shared.m_triggerEffect);

            if (!ItemDataMethods.Save(clone, out ItemAttackData attackData)) return false;
            attackData.m_attackData.OriginalPrefab = attack.sOriginalPrefabName;
            data.Add(attackData);
            advancedItems[name] = attackData;
        }
        return true;
    }

    private static void UpdateEffectList(List<string> list, ref EffectList data)
    {
        if (list.Count == 0) return;
        List<EffectList.EffectData> effects = new();
        foreach (string effect in list)
        {
            GameObject? prefab = DataBase.TryGetGameObject(effect);
            if (prefab == null) continue;
            effects.Add(new EffectList.EffectData()
            {
                m_prefab = prefab,
                m_enabled = true,
            });
        }

        data.m_effectPrefabs = effects.ToArray();
    }
    
    private static GameObject CloneAttack(GameObject item, string itemName, bool register = false)
    {
        var clone = Object.Instantiate(item, MonsterDBPlugin.m_root.transform, false);
        clone.name = itemName;
        ItemDataMethods.m_clonedItems[itemName] = clone;
        Helpers.RegisterToObjectDB(clone);
        if (register) Helpers.RegisterToZNetScene(clone);
        return clone;
    }

    private static void ConvertEffects(CategoryHumanoid? humanoid, ref CreatureData data)
    {
        if (humanoid == null) return;
        if (humanoid.aHitEffects != null) 
            ConvertEffectList(humanoid.aHitEffects, ref data.m_effects.m_hitEffects);
        if (humanoid.aDeathEffects != null)
            ConvertEffectList(humanoid.aDeathEffects, ref data.m_effects.m_deathEffects);
        if (humanoid.aConsumeItemEffects != null)
            ConvertEffectList(humanoid.aConsumeItemEffects, ref data.m_effects.m_consumeItemEffects);
    }

    private static void ConvertEffectList(List<string> effects, ref List<EffectInfo> data)
    {
        foreach (var effectName in effects)
        {
            var prefab = ZNetScene.instance.GetPrefab(effectName);
            if (!prefab) continue;
            data.Add(new EffectInfo()
            {
                PrefabName = prefab.name,
                Enabled = true,
                Variant = -1
            });
        }
    }

    private static void ConvertCharacterDrops(CategoryCharacterDrop? drops, ref List<CharacterDropData> data)
    {
        if (drops?.drDrops == null) return;
        foreach (var drop in drops.drDrops)
        {
            if (drop.sPrefabName == null) continue;
            var prefab = ZNetScene.instance.GetPrefab(drop.sPrefabName);
            if (!prefab) continue;
            data.Add(new CharacterDropData()
            {
                PrefabName = prefab.name,
                AmountMin = drop.iAmountMin,
                AmountMax = drop.iAmountMax,
                LevelMultiplier = drop.bLevelMultiplier,
                OnePerPlayer = drop.bOnePerPlayer
            });
        }
    }

    private static void ConvertMonsterAI(CategoryMonsterAI monsterAI, ref MonsterAIData data)
    {
        data.AlertRange = monsterAI.fAlertRange;
        data.FleeIfHurtWhenTargetCannotBeReached = monsterAI.bFleeIfHurtWhenTargetCantBeReached;
        data.FleeIfNotAlerted = monsterAI.bFleeIfNotAlerted;
        data.FleeIfLowHealth = monsterAI.fFleeIfLowHealth;
        data.CirculateWhileCharging = monsterAI.bCirculateWhileCharging;
        data.CirculateWhileChargingFlying = monsterAI.bCirculateWhileChargingFlying;
        data.EnableHuntPlayer = monsterAI.bEnableHuntPlayer;
        data.AttackPlayerObjects = monsterAI.bAttackPlayerObjects;
        data.AttackPlayerObjects = monsterAI.bAttackPlayerObjectsWhenAlerted;
        data.InterceptTimeMax = monsterAI.fInterceptTimeMax;
        data.InterceptTimeMin = monsterAI.fInterceptTimeMin;
        data.MaxChaseDistance = monsterAI.fMaxChaseDistance;
        data.MinAttackInterval = monsterAI.fMinAttackInterval;
        data.CircleTargetInterval = monsterAI.fCircleTargetInterval;
        data.CircleTargetDuration = monsterAI.fCircleTargetDuration;
        data.ConsumeItems = monsterAI.aConsumeItems ?? new();
    }

    private static void ConvertBaseAI(CategoryBaseAI? baseAI, ref MonsterAIData data, GameObject prefab, ref CreatureData creatureData)
    {
        if (!prefab.GetComponent<MonsterAI>()) return;
        MonsterAIMethods.Save(prefab, ref creatureData);
        if (baseAI == null) return;
        data.ViewAngle = baseAI.fViewAngle;
        data.ViewAngle = baseAI.fViewAngle;
        data.HearRange = baseAI.fHearRange;
        data.PathAgentType = baseAI.sPathAgentType?? "Humanoid";
        data.RandomCircleInterval = baseAI.fRandomCircleInterval;
        data.RandomMoveInterval = baseAI.fRandomMoveInterval;
        data.RandomMoveRange = baseAI.fRandomMoveRange;
        data.AvoidFire = baseAI.bAvoidFire;
        data.AfraidOfFire = baseAI.bAfraidOfFire;
        data.AvoidWater = baseAI.bAvoidWater;
        data.SpawnMessage = baseAI.sSpawnMessage?? "Normal";
        data.DeathMessage = baseAI.sDeathMessage?? "Normal";
    }

    private static void ConvertBaseAI(CategoryBaseAI? baseAI, ref AnimalAIData data, GameObject prefab, ref CreatureData creatureData)
    {
        if (!prefab.GetComponent<AnimalAI>()) return;
        AnimalAIMethods.Save(prefab, ref creatureData);
        if (baseAI == null) return;
        data.ViewAngle = baseAI.fViewAngle;
        data.ViewAngle = baseAI.fViewAngle;
        data.HearRange = baseAI.fHearRange;
        data.PathAgentType = baseAI.sPathAgentType?? "Humanoid";
        data.RandomCircleInterval = baseAI.fRandomCircleInterval;
        data.RandomMoveInterval = baseAI.fRandomMoveInterval;
        data.RandomMoveRange = baseAI.fRandomMoveRange;
        data.AvoidFire = baseAI.bAvoidFire;
        data.AfraidOfFire = baseAI.bAfraidOfFire;
        data.AvoidWater = baseAI.bAvoidWater;
        data.SpawnMessage = baseAI.sSpawnMessage?? "Normal";
        data.DeathMessage = baseAI.sDeathMessage?? "Normal";
    }

    private static void ConvertCharacter(RRRFormat? json, ref CreatureData data)
    {
        var original = json?.sOriginalPrefabName ?? "";
        if (original.StartsWith("MLNPC")) original = "Human";
        if (original.IsNullOrWhiteSpace()) return;
        var prefab = ZNetScene.instance.GetPrefab(original);
        if (!prefab) return;
        if (prefab.GetComponent<Humanoid>())
        {
            HumanoidMethods.Save(prefab, "", ref data);
        }
        else
        {
            CharacterMethods.Save(prefab, "", ref data);
        }
        data.m_characterData.PrefabName = json?.sNewPrefabName ?? "";
        if ((json?.sOriginalPrefabName ?? "").StartsWith("MLNPC"))
        {
            data.m_characterData.ClonedFrom = "Human";
        }
        else
        {
            data.m_characterData.ClonedFrom = json?.sOriginalPrefabName ?? "";
        }
        if (json == null) return;
        ConvertCharacter(json.Category_Character, ref data.m_characterData);
    }

    private static void ConvertCharacter(CategoryCharacter? character, ref CharacterData data)
    {
        if (character == null) return;
        data.Name = character.sName ?? "";
        data.Faction = character.sFaction ?? "";
        data.Boss = character.sBoss ?? false;
        data.BossEvent = character.sBossEvent ?? "";
        data.DefeatSetGlobalKey = character.sDefeatSetGlobalKey ?? "";
        data.Health = character.fHealth;
        data.TolerateWater = character.bTolerateWater;
        data.TolerateFire = character.bTolerateFire;
        data.TolerateSmoke = character.bTolerateSmoke;
        data.StaggerWhenBlocked = character.bStaggerWhenBlocked;
        
        ConvertResistance(character.DamageTaken, ref data);
    }

    private static void ConvertResistance(DamageTaken? resistance, ref CharacterData data)
    {
        if (resistance == null) return;
        data.BluntResistance = resistance.m_blunt ?? "Normal";
        data.SlashResistance = resistance.m_slash?? "Normal";
        data.PierceResistance = resistance.m_pierce?? "Normal";
        data.ChopResistance = resistance.m_chop?? "Normal";
        data.PickaxeResistance = resistance.m_pickaxe?? "Normal";
        data.FireResistance = resistance.m_fire?? "Normal";
        data.FrostResistance = resistance.m_frost?? "Normal";
        data.LightningResistance = resistance.m_lightning?? "Normal";
        data.PoisonResistance = resistance.m_poison?? "Normal";
        data.SpiritResistance = resistance.m_spirit?? "Normal";
    }
    private static void ConvertAppearance(CategoryAppearance? appearance, ref CreatureData data)
    {
        if (appearance?.vScale == null) return;
        var scale = appearance.vScale;
        data.m_scale.x = scale.fX;
        data.m_scale.y = scale.fY;
        data.m_scale.z = scale.fZ;
        data.m_ragdollScale.x = scale.fX;
        data.m_ragdollScale.y = scale.fY;
        data.m_ragdollScale.z = scale.fZ;
    }

    private static void ConvertItemArray(List<string>? list, ref List<ItemAttackData> data)
    {
        if (list == null) return;
        foreach (var item in list)
        {
            if (item.StartsWith("@")) continue;
            var prefab = DataBase.TryGetGameObject(item);
            if (prefab == null) continue;
            if (!ItemDataMethods.Save(prefab, out ItemAttackData itemData)) continue;
            data.Add(itemData);
        }
    }

    private static void ConvertRandomSetArray(List<AdvancedCustomAttacks>? advancedCustomAttacks, List<RandomSet>? setList, string creatureName, ref List<RandomItemSetsData> data, ref Dictionary<string, ItemAttackData> advancedItems)
    {
        if (setList == null) return;
        for (var index = 0; index < setList.Count; index++)
        {
            var set = setList[index];
            var setData = new RandomItemSetsData()
            {
                m_name = creatureName + "_set_" + index
            };
            ConvertAdvancedAttacks(advancedCustomAttacks, set.aSetItems, creatureName, ref setData.m_items, ref advancedItems);
            // ConvertItemArray(set.aSetItems, ref setData.m_items);
            data.Add(setData);
        }
    }
    
    [JsonObject][CanBeNull]
    public class RRRFormat
    {
        public string? sOriginalPrefabName { get;set; }
        public string? sNewPrefabName { get;set; }
        public CategoryAppearance? Category_Appearance { get;set; }
        public CategoryCharacter? Category_Character{ get;set; }
        public CategoryHumanoid? Category_Humanoid { get;set; }
        public CategoryBaseAI? Category_BaseAI{ get;set; }
        public CategoryMonsterAI? Category_MonsterAI { get;set; }
        public CategoryCharacterDrop? Category_CharacterDrops { get;set; }
        public CategorySpecial? Category_Special { get;set; }
        public CategoryNpcOnly? Category_NpcOnly { get;set; }
        public string? sConfigFormatVersion { get;set; }
    }
    [JsonObject][CanBeNull]
    public class CategoryNpcOnly
    {
        public bool bIsFemale { get; set; }
        public bool bNoBeardIfFemale { get; set; }
        public bool bRandomGender { get; set; }
        public List<string>? aBeardIds { get; set; }
        public List<string>? aHairIds { get; set; }
        public Color? cSkinColorMin { get; set; }
        public Color? cSkinColorMax { get; set; }
        public Color? cHairColorMin { get; set; }
        public Color? cHairColorMax { get; set; }
        public float fHairValueMin { get; set; }
        public float fHairValueMax { get; set; }
    }
    [JsonObject][CanBeNull]
    public class CategorySpecial
    {
        public bool bCanTame{ get;set; }
        public bool bAlwaysTame{ get;set; }
        public bool bCommandableWhenTame{ get;set; }
        public bool bCanProcreateWhenTame{ get;set; }
        public bool bFxNoTame{ get;set; }
        public bool bSfxNoAlert{ get;set; }
        public bool bSfxNoIdle{ get;set; }
        public bool bSfxNoHit{ get;set; }
        public bool bSfxNoDeath{ get;set; }
    }
    [JsonObject][CanBeNull]
    public class CategoryCharacterDrop
    {
        public bool bReplaceOriginalDrops{ get;set; }
        public List<Drop>? drDrops { get;set; }
    }

    [JsonObject][CanBeNull]
    public class Drop
    {
        public string? sPrefabName { get; set; }
        public int iAmountMin { get; set; }
        public int iAmountMax { get; set; }
        public float fChance { get; set; }
        public bool bOnePerPlayer { get; set; }
        public bool bLevelMultiplier { get; set; }
    }
    [JsonObject][CanBeNull]
    public class CategoryAppearance
    {
        public Scale? vScale { get;set; }
        public Color? cTintColor { get; set; }
        public Color? cTintItems { get; set; }
        public Color? cBodySmoke { get; set; }
        public string? sCustomTexture { get; set; }
    }

    [JsonObject][CanBeNull]
    public class Color
    {
        public float? fRed { get; set; }
        public float? fGreen { get; set; }
        public float? fBlue { get; set; }
    }
    
    [JsonObject][CanBeNull]
    public class CategoryCharacter
    {
        public string? sName { get;set; }
        public string? sFaction { get;set; }
        public bool? sBoss{ get;set; }
        public string? sBossEvent { get;set; }
        public string? sDefeatSetGlobalKey { get;set; }
        public float fMoveSpeedMulti{ get;set; }
        public bool bTolerateWater{ get;set; }
        public bool bTolerateFire{ get;set; }
        public bool bTolerateSmoke{ get;set; }
        public bool bStaggerWhenBlocked{ get;set; }
        public float fHealth{ get;set; }
        public DamageTaken? DamageTaken { get;set; }
    }
    [JsonObject][CanBeNull]
    public class CategoryHumanoid
    {
        public AttackDamages? dtAttackDamageOverride { get;set; }
        public float fAttackDamageTotalOverride{ get;set; }
        public string? sAttackProjectileOverride { get;set; }
        public List<string>? aDefaultItems { get;set; }
        public List<string>? aRandomArmor { get;set; }
        public List<string>? aRandomShield { get;set; }
        public List<RandomSet>? aaRandomSets { get;set; }
        public List<AdvancedCustomAttacks>? aAdvancedCustomAttacks { get;set; }
        public List<string>? aHitEffects { get; set; }
        public List<string>? aDeathEffects { get; set; }
        public List<string>? aConsumeItemEffects { get; set; }
    }
    [JsonObject][CanBeNull]
    public class CategoryBaseAI
    {
        public float fViewRange{ get;set; }
        public float fViewAngle{ get;set; }
        public float fHearRange{ get;set; }
        public string? sPathAgentType { get;set; }
        public float fRandomCircleInterval{ get;set; }
        public float fRandomMoveInterval{ get;set; }
        public float fRandomMoveRange{ get;set; }
        public bool bAvoidFire{ get;set; }
        public bool bAfraidOfFire{ get;set; }
        public bool bAvoidWater{ get;set; }
        public string? sSpawnMessage { get;set; }
        public string? sDeathMessage { get;set; }
    }
    [JsonObject][CanBeNull]
    public class CategoryMonsterAI
    {
        public float fAlertRange{ get;set; }
        public bool bFleeIfHurtWhenTargetCantBeReached{ get;set; }
        public bool bFleeIfNotAlerted{ get;set; }
        public float fFleeIfLowHealth{ get;set; }
        public bool bCirculateWhileCharging{ get;set; }
        public bool bCirculateWhileChargingFlying{ get;set; }
        public bool bEnableHuntPlayer{ get;set; }
        public bool bAttackPlayerObjects{ get;set; }
        public bool bAttackPlayerObjectsWhenAlerted{ get;set; }
        public float fInterceptTimeMax{ get;set; }
        public float fInterceptTimeMin{ get;set; }
        public float fMaxChaseDistance{ get;set; }
        public float fMinAttackInterval{ get;set; }
        public float fCircleTargetInterval{ get;set; }
        public float fCircleTargetDuration{ get;set; }
        public List<string>? aConsumeItems { get;set; }
    }
    [JsonObject][CanBeNull]
    public class RandomSet
    {
        public List<string>? aSetItems { get;set; }
    }
    [JsonObject][CanBeNull]
    public class AdvancedCustomAttacks
    {
        public string? sOriginalPrefabName { get;set; }
        public string? sTargetAnim { get;set; }
        public float fAIAttackInterval{ get;set; }
        public bool bAIPrioritized{ get;set; }
        public AttackDamages? dtAttackDamageOverride { get;set; }
        public float fAttackDamageTotalOverride { get;set; }
        public string? sAttackProjectileOverride { get; set; }
        public List<string>? aStartEffects { get;set; }
        public List<string>? aHitEffects { get;set; }
        public List<string>? aTriggerEffects { get;set;}
    }
    [JsonObject][CanBeNull]
    public class AttackDamages
    {
        public float fDamage { get;set; }
        public float fBlunt { get; set;}
        public float fSlash { get;set; }
        public float fPierce { get;set; }
        public float fChop { get;set; }
        public float fPickaxe { get;set; }
        public float fFire { get;set; }
        public float fFrost { get;set; }
        public float fLightning { get;set; }
        public float fPoison { get;set; }
        public float fSpirit { get;set; }
    }
    [JsonObject][CanBeNull]
    public class DamageTaken
    {
        public string? m_blunt { get; set; }
        public string? m_slash { get; set; }
        public string? m_pierce { get; set; }
        public string? m_chop { get; set; }
        public string? m_pickaxe { get; set; }
        public string? m_fire { get; set; }
        public string? m_frost { get; set; }
        public string? m_lightning { get; set; }
        public string? m_poison { get; set; }
        public string? m_spirit { get; set; }
    }
    [JsonObject][CanBeNull]
    public class Scale
    {
        public float fX { get; set; }
        public float fY { get; set; }
        public float fZ { get; set; }
    }
    
}