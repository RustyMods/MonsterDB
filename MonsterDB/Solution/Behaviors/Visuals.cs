using System.Collections.Generic;
using MonsterDB.Solution.Methods;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonsterDB.Solution.Behaviors;

public class Visuals : MonoBehaviour
{
    public List<string> m_maleFirstNames = new()
    {
        "Bjorn", "Harald", "Bo", "Frode", 
        "Birger", "Arne", "Erik", "Kare", 
        "Loki", "Thor", "Odin", "Ragnar", 
        "Sigurd", "Ivar", "Gunnar", "Sven",
        "Hakon", "Leif", "Magnus", "Rolf", 
        "Ulf", "Vidar", "Ingvar"
    };

    public List<string> m_femaleFirstNames = new()
    {
        "Gudrun", "Hilda", "Ingrid", "Freya", 
        "Astrid", "Sigrid", "Thora", "Runa", 
        "Ylva", "Sif", "Helga", "Eira", 
        "Brynja", "Ragnhild", "Solveig", "Bodil", 
        "Signy", "Frida", "Alva", "Liv", 
        "Estrid", "Jorunn", "Aslaug", "Torunn"
    };
    
    public List<string> m_lastNames = new()
    {
        "Ironside", "Fairhair", "Thunderfist", "Bloodaxe", 
        "Longsword", "Ravenheart", "Dragonslayer", "Stormborn", 
        "Shadowblade", "Thunderstruck", "Allfather", "Lothbrok", 
        "Snake-in-the-Eye", "the Boneless", "Ironhand", "Forkbeard",
        "the Good", "the Lucky", "the Strong", "the Walker", 
        "Ironbeard", "the Silent", "the Fearless", "Shieldmaiden",
        "Bloodfury", "Snowdrift", "Wildheart", "Battleborn", 
        "Stormshield", "Frosthammer", "Moonshadow", "Wolfsbane"
    };
    
    public ZNetView m_nview = null!;
    public VisEquipment m_visEquipment = null!;
    public Human m_human = null!;
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        m_visEquipment = GetComponent<VisEquipment>();
        m_human = GetComponent<Human>();
        if (!m_nview.IsValid()) return;
        
        int modelIndex = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex, Random.Range(0, 2));
        int hairItem = m_nview.GetZDO().GetInt(ZDOVars.s_hairItem, Random.Range(0, 20));
        int beardItem = m_nview.GetZDO().GetInt(ZDOVars.s_beardItem, Random.Range(0, 20));
        Vector3 hairColor = m_nview.GetZDO().GetVec3(ZDOVars.s_hairColor, Vector3.zero);
        
        if (CreatureManager.m_data.TryGetValue(name.Replace("(Clone)", string.Empty), out CreatureData data))
        {
            m_nview.GetZDO().Set(ZDOVars.s_modelIndex, data.m_humanData.ModelIndex);
            m_nview.GetZDO().Set(ZDOVars.s_hairItem, data.m_humanData.HairIndex);
            m_nview.GetZDO().Set(ZDOVars.s_beardItem, data.m_humanData.BeardIndex);

            modelIndex = data.m_humanData.ModelIndex;
            hairItem = data.m_humanData.HairIndex;
            beardItem = data.m_humanData.BeardIndex;
        }
        
        m_visEquipment.SetHairItem("Hair" + hairItem);
        if (modelIndex == 0) m_visEquipment.SetBeardItem("Beard" + beardItem);
        if (m_human != null)
        {
            m_human.m_beardItem = m_visEquipment.m_beardItem;
            m_human.m_hairItem = m_visEquipment.m_hairItem;
            if (MonsterDBPlugin.UseNames()) m_human.m_name = GenerateName();
        }
        m_visEquipment.SetHairColor(hairColor);
        m_visEquipment.SetModel(modelIndex);

        CheckMonsterDB();
    }

    private void CheckMonsterDB()
    {
        if (!CreatureManager.m_data.TryGetValue(name.Replace("(Clone)", string.Empty), out CreatureData data)) return;
        if (!data.m_materials.TryGetValue("PlayerMaterial", out VisualMethods.MaterialData playerMat)) return;
        m_nview.GetZDO().Set(ZDOVars.s_skinColor, Convert(playerMat._Color));
        if (!data.m_materials.TryGetValue("PlayerHair", out VisualMethods.MaterialData hairMat)) return;
        m_nview.GetZDO().Set(ZDOVars.s_hairColor, Convert(hairMat._Color));
        m_visEquipment.SetHairColor(Convert(hairMat._Color));
    }

    private Vector3 Convert(VisualMethods.ColorData color) => new Vector3(color.r, color.g, color.b);
    private string GenerateName()
    {
        if (TryGetComponent(out Tameable component) && component.m_randomStartingName.Count > 0)
            return component.m_randomStartingName[Random.Range(0, component.m_randomStartingName.Count)];
        bool isFemale = m_nview.GetZDO().GetInt(ZDOVars.s_modelIndex) == 0;
        var firstName = isFemale
            ? m_femaleFirstNames[Random.Range(0, m_femaleFirstNames.Count)]
            : m_maleFirstNames[Random.Range(0, m_maleFirstNames.Count)];
        var lastName = m_lastNames[Random.Range(0, m_lastNames.Count)];
        return $"{firstName} {lastName}";
    }
}