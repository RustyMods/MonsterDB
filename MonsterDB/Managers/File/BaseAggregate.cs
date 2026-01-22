using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class BaseAggregate : Header
{
    [YamlMember(Order = 6)] public Dictionary<string, BaseHumanoid>? humanoids;
    [YamlMember(Order = 7)] public Dictionary<string, BaseCharacter>? characters;
    [YamlMember(Order = 8)] public Dictionary<string, BaseHuman>? humans;
    [YamlMember(Order = 9)] public Dictionary<string, BaseEgg>? eggs;
    [YamlMember(Order = 10)] public Dictionary<string, BaseItem>? items;
    [YamlMember(Order = 11)] public Dictionary<string, BaseFish>? fishes;
    [YamlMember(Order = 12)] public Dictionary<string, BaseProjectile>? projectiles;
    [YamlMember(Order = 13)] public Dictionary<string, BaseRagdoll>? ragdolls;
    [YamlMember(Order = 14)] public Dictionary<string, BaseSpawnAbility>? spawnAbilities;
    [YamlMember(Order = 15)] public Dictionary<string, BaseVisual>? visuals;
    [YamlMember(Order = 16)] public Dictionary<string, Dictionary<string, string>>? translations;

    public string? PrefabToUpdate;

    public int Count() => humanoids?.Count + characters?.Count + humans?.Count + eggs?.Count + items?.Count +
        fishes?.Count + projectiles?.Count + ragdolls?.Count + spawnAbilities?.Count + visuals?.Count ?? 0;

    public bool GetPrefabToUpdate(out Header header)
    {
        header = null!;
        if (PrefabToUpdate == null || string.IsNullOrEmpty(PrefabToUpdate)) return false;
        if (humanoids != null && humanoids.TryGetValue(PrefabToUpdate, out BaseHumanoid? humanoid))
        {
            header = humanoid;
            return true;
        }

        if (characters != null && characters.TryGetValue(PrefabToUpdate, out BaseCharacter? character))
        {
            header = character;
            return true;
        }

        if (humans != null && humans.TryGetValue(PrefabToUpdate, out BaseHuman? human))
        {
            header = human;
            return true;
        }

        if (eggs != null && eggs.TryGetValue(PrefabToUpdate, out BaseEgg? egg))
        {
            header = egg;
            return true;
        }

        if (items != null && items.TryGetValue(PrefabToUpdate, out BaseItem? item))
        {
            header = item;
            return true;
        }

        if (fishes != null && fishes.TryGetValue(PrefabToUpdate, out BaseFish? fish))
        {
            header = fish;
            return true;
        }

        if (projectiles != null && projectiles.TryGetValue(PrefabToUpdate, out BaseProjectile? projectile))
        {
            header = projectile;
            return true;
        }

        if (ragdolls != null && ragdolls.TryGetValue(PrefabToUpdate, out BaseRagdoll? ragdoll))
        {
            header = ragdoll;
            return true;
        }

        if (spawnAbilities != null && spawnAbilities.TryGetValue(PrefabToUpdate, out BaseSpawnAbility? spawnability))
        {
            header = spawnability;
            return true;
        }

        if (visuals != null && visuals.TryGetValue(PrefabToUpdate, out BaseVisual? visual))
        {
            header = visual;
            return true;
        }

        return false;
    }

    private void ParseTranslations(string filePath, string fileName)
    {
        if (translations == null) translations = new Dictionary<string, Dictionary<string, string>>();
        string[] parts = fileName.Split('.');
        if (parts.Length == 2)
        {
            string language = parts[1];
            string[] lines = File.ReadAllLines(filePath);
            if (lines.Length <= 0) return;
            translations[language] = new Dictionary<string, string>();
            for (int y = 0; y < lines.Length; ++y)
            {
                string line = lines[y];
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                string[] lineParts = line.Split(':');
                if (lineParts.Length < 2) continue;
                string key = lineParts[0].Trim();
                string value = lineParts[1].Trim();
                translations[language][key] = value;
            }
        }
    }
    
    public void Read(string directoryPath)
    {
        SetupVersions();
        Type = BaseType.All;

        if (!Directory.Exists(directoryPath)) return;

        string[] files = Directory.GetFiles(directoryPath, "*.yml", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            string? fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.StartsWith("translations."))
            {
                ParseTranslations(filePath, fileName);
                continue;
            }

            string text = File.ReadAllText(filePath);
            Header header = ConfigManager.Deserialize<Header>(text);
            switch (header.Type)
            {
                case BaseType.Humanoid:
                    BaseHumanoid humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                    Add(humanoid);
                    break;
                case BaseType.Character:
                    BaseCharacter character = ConfigManager.Deserialize<BaseCharacter>(text);
                    Add(character);
                    break;
                case BaseType.Human:
                    BaseHuman human = ConfigManager.Deserialize<BaseHuman>(text);
                    Add(human);
                    break;
                case BaseType.Egg:
                    BaseEgg egg = ConfigManager.Deserialize<BaseEgg>(text);
                    Add(egg);
                    break;
                case BaseType.Item:
                    BaseItem item = ConfigManager.Deserialize<BaseItem>(text);
                    Add(item);
                    break;
                case BaseType.Fish:
                    BaseFish fish = ConfigManager.Deserialize<BaseFish>(text);
                    Add(fish);
                    break;
                case BaseType.Projectile:
                    BaseProjectile projectile = ConfigManager.Deserialize<BaseProjectile>(text);
                    Add(projectile);
                    break;
                case BaseType.Ragdoll:
                    BaseRagdoll ragdoll = ConfigManager.Deserialize<BaseRagdoll>(text);
                    Add(ragdoll);
                    break;
                case BaseType.SpawnAbility:
                    BaseSpawnAbility spawnAbility = ConfigManager.Deserialize<BaseSpawnAbility>(text);
                    Add(spawnAbility);
                    break;
                case BaseType.Visual:
                    BaseVisual visual = ConfigManager.Deserialize<BaseVisual>(text);
                    Add(visual);
                    break;
            }
        }
    }

    public List<Header> Load()
    {
        List<Header> list = new();
        if (humanoids != null)
        {
            foreach (BaseHumanoid? humanoid in humanoids.Values)
            {
                list.Add(humanoid);
            }
        }

        if (characters != null)
        {
            foreach (BaseCharacter? character in characters.Values)
            {
                list.Add(character);
            }
        }

        if (humans != null)
        {
            foreach (var human in humans.Values)
            {
                list.Add(human);
            }
        }

        if (eggs != null)
        {
            foreach (BaseEgg? egg in eggs.Values)
            {
                list.Add(egg);
            }
        }

        if (items != null)
        {
            foreach (BaseItem item in items.Values)
            {
                list.Add(item);
            }
        }

        if (fishes != null)
        {
            foreach (BaseFish fish in fishes.Values)
            {
                list.Add(fish);
            }
        }

        if (projectiles != null)
        {
            foreach (BaseProjectile projectile in projectiles.Values)
            {
                list.Add(projectile);
            }
        }

        if (ragdolls != null)
        {
            foreach (BaseRagdoll ragdoll in ragdolls.Values)
            {
                list.Add(ragdoll);
            }
        }

        if (spawnAbilities != null)
        {
            foreach (BaseSpawnAbility spawnAbility in spawnAbilities.Values)
            {
                list.Add(spawnAbility);
            }
        }

        if (visuals != null)
        {
            foreach (BaseVisual visual in visuals.Values)
            {
                list.Add(visual);
            }
        }

        if (translations != null)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in translations)
            {
                string? lang = kvp.Key;
                Dictionary<string, string>? lines = kvp.Value;

                LocalizationManager.AddWords(lang, lines);
            }
        }

        return list;
    }

    public void Add(BaseVisual visual)
    {
        visuals ??= new Dictionary<string, BaseVisual>();
        visuals[visual.Prefab] = visual;
    }

    public void Add(BaseSpawnAbility spawnAbility)
    {
        spawnAbilities ??= new Dictionary<string, BaseSpawnAbility>();
        spawnAbilities[spawnAbility.Prefab] = spawnAbility;
    }

    public void Add(BaseHumanoid humanoid)
    {
        humanoids ??= new Dictionary<string, BaseHumanoid>();
        humanoids[humanoid.Prefab] = humanoid;
    }

    public void Add(BaseCharacter character)
    {
        characters ??= new Dictionary<string, BaseCharacter>();
        characters[character.Prefab] = character;
    }

    public void Add(BaseHuman human)
    {
        humans ??= new Dictionary<string, BaseHuman>();
        humans[human.Prefab] = human;
    }

    public void Add(BaseFish fish)
    {
        fishes ??= new Dictionary<string, BaseFish>();
        fishes[fish.Prefab] = fish;
    }

    public void Add(BaseProjectile projectile)
    {
        projectiles ??= new Dictionary<string, BaseProjectile>();
        projectiles[projectile.Prefab] = projectile;
    }

    public void Add(BaseRagdoll ragdoll)
    {
        ragdolls ??= new Dictionary<string, BaseRagdoll>();
        ragdolls[ragdoll.Prefab] = ragdoll;
    }

    public void Add(BaseEgg egg)
    {
        eggs ??= new Dictionary<string, BaseEgg>();
        eggs[egg.Prefab] = egg;
    }

    public void Add(BaseItem item)
    {
        items ??= new Dictionary<string, BaseItem>();
        items[item.Prefab] = item;
    }

    public void AddTranslations(string language, string[] lines)
    {
        if (lines.Length <= 0) return;

        translations ??= new Dictionary<string, Dictionary<string, string>>();

        if (!translations.TryGetValue(language, out var translation))
        {
            translation = new Dictionary<string, string>();
        }

        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
            string[] parts = line.Split(':');
            if (parts.Length < 2) continue;
            string key = parts[0].Trim();
            string value = parts[1].Trim();
            translation[key] = value;
        }

        translations[language] = translation;
    }

    public void Add(BaseAggregate other)
    {
        if (other.humanoids != null)
        {
            foreach (BaseHumanoid? humanoid in other.humanoids.Values)
            {
                Add(humanoid);
            }
        }

        if (other.characters != null)
        {
            foreach (BaseCharacter? character in other.characters.Values)
            {
                Add(character);
            }
        }

        if (other.humans != null)
        {
            foreach (BaseHuman? human in other.humans.Values)
            {
                Add(human);
            }
        }

        if (other.ragdolls != null)
        {
            foreach (BaseRagdoll? ragdoll in other.ragdolls.Values)
            {
                Add(ragdoll);
            }
        }

        if (other.items != null)
        {
            foreach (BaseItem? item in other.items.Values)
            {
                Add(item);
            }
        }

        if (other.eggs != null)
        {
            foreach (BaseEgg? egg in other.eggs.Values)
            {
                Add(egg);
            }
        }

        if (other.projectiles != null)
        {
            foreach (BaseProjectile? projectile in other.projectiles.Values)
            {
                Add(projectile);
            }
        }

        if (other.fishes != null)
        {
            foreach (BaseFish? fish in other.fishes.Values)
            {
                Add(fish);
            }
        }

        if (other.spawnAbilities != null)
        {
            foreach (BaseSpawnAbility? spawnAbility in other.spawnAbilities.Values)
            {
                Add(spawnAbility);
            }
        }
        

        if (other.translations != null)
        {
            if (translations == null) translations = other.translations;
            else
            {
                foreach (KeyValuePair<string, Dictionary<string, string>> kvp in other.translations)
                {
                    if (translations.TryGetValue(kvp.Key, out var translation))
                    {
                        foreach (KeyValuePair<string, string> dict in kvp.Value)
                        {
                            translation[dict.Key] = dict.Value;
                        }
                    }
                    else
                    {
                        translations.Add(kvp.Key, kvp.Value);
                    }
                }
            }
        }
    }
}