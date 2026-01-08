using System;
using System.Collections.Generic;
using System.Linq;

namespace MonsterDB;

[Serializable]
public class BaitSettingRef : Reference
{
    public string m_bait = "";
    public float m_chance;
}

public static partial class Extensions
{
    public static List<BaitSettingRef> ToRef(this List<Fish.BaitSetting> baitSettings)
    {
        return baitSettings
            .Where(x => x.m_bait != null)
            .Select(x => new BaitSettingRef()
            {
                m_bait = x.m_bait.name,
                m_chance = x.m_chance,
            })
            .ToList();
    }

    public static List<Fish.BaitSetting> FromRef(this List<BaitSettingRef> reference)
    {
        return reference
            .Where(x => !string.IsNullOrEmpty(x.m_bait))
            .Select(x => new Fish.BaitSetting()
            {
                m_bait = PrefabManager.GetPrefab(x.m_bait)?.GetComponent<ItemDrop>(),
                m_chance = x.m_chance
            })
            .Where(x => x.m_bait != null)
            .ToList();
    }
}