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
    [YamlMember(Order = 14)] public Dictionary<string, Dictionary<string, string>>? translations;

    public string? PrefabToUpdate;

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

    public void Add(BaseSpawnAbility spawnAbility)
    {
        if (spawnAbilities == null) spawnAbilities = new();
        spawnAbilities[spawnAbility.Prefab] = spawnAbility;
    }

    public void Add(BaseHumanoid humanoid)
    {
        if (humanoids == null) humanoids = new();
        humanoids[humanoid.Prefab] = humanoid;
    }

    public void Add(BaseCharacter character)
    {
        if (characters == null) characters = new();
        characters[character.Prefab] = character;
    }

    public void Add(BaseHuman human)
    {
        if (humans == null) humans = new();
        humans[human.Prefab] = human;
    }

    public void Add(BaseFish fish)
    {
        if (fishes == null) fishes = new();
        fishes[fish.Prefab] = fish;
    }

    public void Add(BaseProjectile projectile)
    {
        if (projectiles == null) projectiles = new();
        projectiles[projectile.Prefab] = projectile;
    }

    public void Add(BaseRagdoll ragdoll)
    {
        if (ragdolls == null) ragdolls = new();
        ragdolls[ragdoll.Prefab] = ragdoll;
    }

    public void Add(BaseEgg egg)
    {
        if (eggs == null) eggs = new();
        eggs[egg.Prefab] = egg;
    }

    public void Add(BaseItem item)
    {
        if (items == null) items = new();
        items[item.Prefab] = item;
    }

    public void AddTranslations(string language, string[] lines)
    {
        if (lines.Length <= 0) return;

        if (translations == null) translations = new();

        if (!translations.TryGetValue(language, out var translation))
        {
            translation = new();
        }

        for (int i = 0; i < lines.Length; ++i)
        {
            string line = lines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
            var parts = line.Split(':');
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
            foreach (var humanoid in other.humanoids.Values)
            {
                Add(humanoid);
            }
        }

        if (other.characters != null)
        {
            foreach (var character in other.characters.Values)
            {
                Add(character);
            }
        }

        if (other.humans != null)
        {
            foreach (var human in other.humans.Values)
            {
                Add(human);
            }
        }

        if (other.ragdolls != null)
        {
            foreach (var ragdoll in other.ragdolls.Values)
            {
                Add(ragdoll);
            }
        }

        if (other.items != null)
        {
            foreach (var item in other.items.Values)
            {
                Add(item);
            }
        }

        if (other.eggs != null)
        {
            foreach (var egg in other.eggs.Values)
            {
                Add(egg);
            }
        }

        if (other.projectiles != null)
        {
            foreach (var projectile in other.projectiles.Values)
            {
                Add(projectile);
            }
        }

        if (other.fishes != null)
        {
            foreach (var fish in other.fishes.Values)
            {
                Add(fish);
            }
        }

        if (other.spawnAbilities != null)
        {
            foreach (var spawnAbility in other.spawnAbilities.Values)
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