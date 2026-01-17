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
    [YamlMember(Order = 14)] public Dictionary<string, Dictionary<string, string>>? translations;

    public string? PrefabToUpdate;

    public bool GetPrefabToUpdate(out Header header)
    {
        header = null;
        if (PrefabToUpdate == null || string.IsNullOrEmpty(PrefabToUpdate)) return false;
        if (humanoids != null && humanoids.TryGetValue(PrefabToUpdate, out var humanoid))
        {
            header = humanoid;
            return true;
        }

        if (characters != null && characters.TryGetValue(PrefabToUpdate, out var character))
        {
            header = character;
            return true;
        }

        if (humans != null && humans.TryGetValue(PrefabToUpdate, out var human))
        {
            header = human;
            return true;
        }

        if (eggs != null && eggs.TryGetValue(PrefabToUpdate, out var egg))
        {
            header = egg;
            return true;
        }

        if (items != null && items.TryGetValue(PrefabToUpdate, out var item))
        {
            header = item;
            return true;
        }

        if (fishes != null && fishes.TryGetValue(PrefabToUpdate, out var fish))
        {
            header = fish;
            return true;
        }

        if (projectiles != null && projectiles.TryGetValue(PrefabToUpdate, out var projectile))
        {
            header = projectile;
            return true;
        }

        if (ragdolls != null && ragdolls.TryGetValue(PrefabToUpdate, out var ragdoll))
        {
            header = ragdoll;
            return true;
        }

        return false;
    }
    
    public void Init(string dirPath)
    {
        SetupVersions();
        Type = BaseType.All;

        if (!Directory.Exists(dirPath)) return;

        string[] files = Directory.GetFiles(dirPath, "*.yml", SearchOption.AllDirectories);

        for (int i = 0; i < files.Length; ++i)
        {
            string filePath = files[i];
            string? fileName = Path.GetFileNameWithoutExtension(filePath);
            if (fileName.StartsWith("translations."))
            {
                if (translations == null) translations = new Dictionary<string, Dictionary<string, string>>();
                string[] parts = fileName.Split('.');
                if (parts.Length == 2)
                {
                    string language = parts[1];
                    string[] lines = File.ReadAllLines(filePath);
                    if (lines.Length <= 0) continue;
                    translations[language] = new Dictionary<string, string>();
                    for (int y = 0; y < lines.Length; ++y)
                    {
                        string line = lines[y];
                        if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
                        var lineParts = line.Split(':');
                        if (lineParts.Length < 2) continue;
                        var key = lineParts[0].Trim();
                        var value = lineParts[1].Trim();
                        translations[language][key] = value;
                    }
                }

                continue;
            }

            string text = File.ReadAllText(filePath);
            Header header = ConfigManager.Deserialize<Header>(text);
            switch (header.Type)
            {
                case BaseType.Humanoid:
                    var humanoid = ConfigManager.Deserialize<BaseHumanoid>(text);
                    Add(humanoid);
                    break;
                case BaseType.Character:
                    var character = ConfigManager.Deserialize<BaseCharacter>(text);
                    Add(character);
                    break;
                case BaseType.Human:
                    var human = ConfigManager.Deserialize<BaseHuman>(text);
                    Add(human);
                    break;
                case BaseType.Egg:
                    var egg = ConfigManager.Deserialize<BaseEgg>(text);
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
            }
        }
    }

    public void Load()
    {
        if (humanoids != null)
        {
            foreach (BaseHumanoid? humanoid in humanoids.Values)
            {
                SyncManager.loadList.Add(humanoid);
            }
        }

        if (characters != null)
        {
            foreach (BaseCharacter? character in characters.Values)
            {
                SyncManager.loadList.Add(character);
            }
        }

        if (humans != null)
        {
            foreach (var human in humans.Values)
            {
                SyncManager.loadList.Add(human);
            }
        }

        if (eggs != null)
        {
            foreach (BaseEgg? egg in eggs.Values)
            {
                SyncManager.loadList.Add(egg);
            }
        }

        if (items != null)
        {
            foreach (BaseItem item in items.Values)
            {
                SyncManager.loadList.Add(item);
            }
        }

        if (fishes != null)
        {
            foreach (BaseFish fish in fishes.Values)
            {
                SyncManager.loadList.Add(fish);
            }
        }

        if (projectiles != null)
        {
            foreach (BaseProjectile projectile in projectiles.Values)
            {
                SyncManager.loadList.Add(projectile);
            }
        }

        if (ragdolls != null)
        {
            foreach (BaseRagdoll ragdoll in ragdolls.Values)
            {
                SyncManager.loadList.Add(ragdoll);
            }
        }

        if (translations != null)
        {
            foreach (KeyValuePair<string, Dictionary<string, string>> kvp in translations)
            {
                string? lang = kvp.Key;
                Dictionary<string, string>? lines = kvp.Value;

                LocalizationManager.Register(lang, lines);
            }
        }
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
            var line = lines[i];
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;
            var parts = line.Split(':');
            if (parts.Length < 2) continue;
            var key = parts[0].Trim();
            var value = parts[1].Trim();
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

        if (fishes != null)
        {
            foreach (var fish in fishes.Values)
            {
                Add(fish);
            }
        }

        if (other.translations != null)
        {
            if (translations == null) translations = other.translations;
            else
            {
                foreach (var kvp in other.translations)
                {
                    if (translations.TryGetValue(kvp.Key, out var translation))
                    {
                        foreach (var dict in kvp.Value)
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