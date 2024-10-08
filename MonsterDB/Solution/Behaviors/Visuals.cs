﻿using System.Collections.Generic;
using BepInEx;
using HarmonyLib;
using UnityEngine;

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
    public Vector3 m_hairColor = Vector3.one;
    public Vector3 m_skinColor = Vector3.one;
    public void Awake()
    {
        m_nview = GetComponent<ZNetView>();
        if (!TryGetComponent(out VisEquipment visEquipment)) return;
        if (!TryGetComponent(out Humanoid component)) return;
        Randomize(component, visEquipment, out bool female);
        if (MonsterDBPlugin.UseNames()) RandomName(component, female);
        m_nview.GetZDO().Set("RandomHuman", true);
    }

    public void RandomName(Humanoid component, bool isFemale)
    {
        if (!m_nview.IsValid()) return;
        string vikingName = m_nview.GetZDO().GetString(ZDOVars.s_tamedName);
        // string? vikingName = m_nview.GetZDO().GetString("RandomName".GetStableHashCode());
        if (vikingName.IsNullOrWhiteSpace())
        {
            var firstName = isFemale
                ? m_femaleFirstNames[Random.Range(0, m_femaleFirstNames.Count)]
                : m_maleFirstNames[Random.Range(0, m_maleFirstNames.Count)];
            var lastName = m_lastNames[Random.Range(0, m_lastNames.Count)];
            component.m_name = $"{firstName} {lastName}";
            if (TryGetComponent(out Tameable tameable))
            {
                if (tameable.m_randomStartingName.Count > 0)
                {
                    component.m_name =
                        tameable.m_randomStartingName[Random.Range(0, tameable.m_randomStartingName.Count)];
                }
            }
            m_nview.GetZDO().Set(ZDOVars.s_tamedName, component.m_name);
            // m_nview.GetZDO().Set("RandomName".GetStableHashCode(), component.m_name);
        }
        else
        {
            component.m_name = vikingName;
        }
    }

    private Vector3 GetHairColor() => m_nview.GetZDO().GetVec3("HairColor", Vector3.zero);
    private int GetModelIndex() => m_nview.GetZDO().GetInt("ModelIndex");
    private int GetHairType() => m_nview.GetZDO().GetInt("HairNumber");

    public void SetRagdoll()
    {
        if (!m_nview.GetZDO().GetBool("RandomHuman")) return;
        int model = GetModelIndex();
        int hair = GetHairType();
        Vector3 color = GetHairColor();
        
    }

    public void Randomize(Humanoid humanoid, VisEquipment visEquipment, out bool female)
    {
        bool hasData = m_nview.GetZDO().GetBool("RandomHuman");
        int modelIndex;
        int random;
        Vector3 color;

        if (hasData)
        {
            modelIndex = GetModelIndex();
            random = GetHairType();
            color = GetHairColor();
        }
        else
        {
            modelIndex = Random.Range(0, 2);
            random = Random.Range(0, 20);
            color = HairColors.GetHairColor();
            m_nview.GetZDO().Set("ModelIndex", modelIndex);
            m_nview.GetZDO().Set("HairNumber", random);
            m_nview.GetZDO().Set("HairColor", color);
        }
        
        female = modelIndex == 1;

        if (!female)
        {
            if (humanoid.m_beardItem.IsNullOrWhiteSpace())
            {
                visEquipment.SetBeardItem("Beard" + random);
            }
            else
            {
                visEquipment.SetBeardItem(humanoid.m_beardItem);
            }
        }

        if (humanoid.m_hairItem.IsNullOrWhiteSpace())
        {
            visEquipment.SetHairItem("Hair" + random);
        }
        else
        {
            visEquipment.SetHairItem(humanoid.m_hairItem);
        }
        humanoid.m_beardItem = visEquipment.m_beardItem;
        humanoid.m_hairItem = visEquipment.m_hairItem;
        visEquipment.SetHairColor(color);
        visEquipment.SetModel(modelIndex);
    }
}