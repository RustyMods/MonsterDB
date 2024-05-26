using System;
using BepInEx;
using UnityEngine;

namespace MonsterDB.Behaviors;

public class RandomHuman : MonoBehaviour
{
    public bool m_isRagDoll;
    public void Awake()
    {
        if (m_isRagDoll)
        {
            RandomRagDoll();
            return;
        }
        if (!TryGetComponent(out Humanoid humanoid)) return;
        RandomHairStyles(humanoid);
        RandomName(humanoid);
    }

    public void RandomRagDoll()
    {
        if (!TryGetComponent(out VisEquipment visEquipment)) return;
        var modelIndex = UnityEngine.Random.Range(0, 2);
        var random = UnityEngine.Random.Range(0, 20);
        if (modelIndex == 0) visEquipment.SetBeardItem("Beard" + random);
        visEquipment.SetHairItem("Hair" + random);
        visEquipment.SetHairColor(new Vector3(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f)));
        visEquipment.SetModel(modelIndex);
    }

    public void RandomName(Humanoid humanoid)
    {
        if (!TryGetComponent(out ZNetView znv)) return;
        if (!TryGetComponent(out Tameable tameable)) return;
        if (tameable.m_randomStartingName.Count <= 0 || !znv.IsValid()) return;
        var random = UnityEngine.Random.Range(0, tameable.m_randomStartingName.Count - 1);
        humanoid.m_name = tameable.m_randomStartingName[random];
    }

    public void RandomHairStyles(Humanoid humanoid)
    {
        if (!TryGetComponent(out VisEquipment visEquipment)) return;
        var modelIndex = UnityEngine.Random.Range(0, 2);
        var random = UnityEngine.Random.Range(0, 20);
        if (humanoid.m_beardItem.IsNullOrWhiteSpace())
        {
            if (modelIndex == 0) visEquipment.SetBeardItem("Beard" + random);
        }
        else
        {
            if (modelIndex == 0) visEquipment.SetBeardItem(humanoid.m_beardItem);
        }

        if (humanoid.m_hairItem.IsNullOrWhiteSpace())
        {
            visEquipment.SetHairItem("Hair" + random);
        }
        else
        {
            visEquipment.SetHairItem(humanoid.m_hairItem);
        }
        visEquipment.SetHairColor(new Vector3(UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f),UnityEngine.Random.Range(0f,1f)));
        
        visEquipment.SetModel(modelIndex);
    }
}