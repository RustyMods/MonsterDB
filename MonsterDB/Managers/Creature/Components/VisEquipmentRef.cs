using System;

using HarmonyLib;
using UnityEngine;

namespace MonsterDB;

[Serializable]
public class VisEquipmentRef : Reference
{
    public TransformRef? m_leftHand;
    public TransformRef? m_rightHand;
    public TransformRef? m_helmet;
    public TransformRef? m_backShield;
    public TransformRef? m_backMelee;
    public TransformRef? m_backTwohandedMelee;
    public TransformRef? m_backBow;
    public TransformRef? m_backTool;
    public TransformRef? m_backAtgeir;
    
    public VisEquipmentRef(){}
    
    public VisEquipmentRef(VisEquipment comp)
    {
        m_leftHand = comp.m_leftHand ? new TransformRef(comp.m_leftHand) : null;
        m_rightHand = comp.m_rightHand ? new TransformRef(comp.m_rightHand) : null;
        m_helmet = comp.m_helmet ? new TransformRef(comp.m_helmet) : null;
        m_backShield = comp.m_backShield ? new TransformRef(comp.m_backShield) : null;
        m_backMelee = comp.m_backMelee ? new TransformRef(comp.m_backMelee) : null;
        m_backTwohandedMelee = comp.m_backTwohandedMelee ? new TransformRef(comp.m_backTwohandedMelee) : null;
        m_backBow = comp.m_backBow ? new TransformRef(comp.m_backBow) : null;
        m_backTool = comp.m_backTool ? new TransformRef(comp.m_backTool) : null;
        m_backAtgeir = comp.m_backAtgeir ? new TransformRef(comp.m_backAtgeir) : null;
    }

    public TransformRef? Get(Transform joint)
    {
        if (m_leftHand != null && m_leftHand.m_name == joint.name) return m_leftHand;
        if (m_rightHand != null && m_rightHand.m_name == joint.name) return m_rightHand;
        if (m_helmet != null && m_helmet.m_name == joint.name) return m_helmet;
        if (m_backShield != null && m_backShield.m_name == joint.name) return m_backShield;
        if (m_backMelee != null && m_backMelee.m_name == joint.name) return m_backMelee;
        if (m_backTwohandedMelee != null && m_backTwohandedMelee.m_name == joint.name) return m_backTwohandedMelee;
        if (m_backBow != null && m_backBow.m_name == joint.name) return m_backBow;
        if (m_backTool != null && m_backTool.m_name == joint.name) return m_backTool;
        if (m_backAtgeir != null && m_backAtgeir.m_name == joint.name) return m_backAtgeir;
        return null;
    }

    public override void UpdateFields<T>(T target, string targetName, bool log)
    {
        if (target is not VisEquipment visEq) return;

        if (m_leftHand != null)
        {
            Transform? transform = m_leftHand.Find(visEq.gameObject, visEq.m_leftHand);
            if (transform != null)
            {
                visEq.m_leftHand = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_leftHand: {transform.name}");
                m_leftHand.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_leftHand = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_leftHand: null");
            }
        }

        if (m_rightHand != null)
        {
            Transform? transform = m_rightHand.Find(visEq.gameObject, visEq.m_rightHand);
            if (transform != null)
            {
                visEq.m_rightHand = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_rightHand: {transform.name}");
                m_rightHand.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_rightHand = null;
            }
        }

        if (m_helmet != null)
        {
            Transform? transform = m_helmet.Find(visEq.gameObject, visEq.m_helmet);
            if (transform != null)
            {
                visEq.m_helmet = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_helmet: {transform.name}");
                m_helmet.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_helmet = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_helmet: null");
            }
        }

        if (m_backShield != null)
        {
            Transform? transform = m_backShield.Find(visEq.gameObject, visEq.m_backShield);
            if (transform != null)
            {
                visEq.m_backShield = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backShield: {transform.name}");
                m_backShield.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_backShield = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backShield: null");
            }
        }

        if (m_backMelee != null)
        {
            Transform? transform = m_backMelee.Find(visEq.gameObject, visEq.m_backMelee);
            if (transform != null)
            {
                visEq.m_backMelee = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backMelee: {transform.name}");
                m_backMelee.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_backMelee = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backMelee: null");
            }
        }

        if (m_backTwohandedMelee != null)
        {
            Transform? transform = m_backTwohandedMelee.Find(visEq.gameObject, visEq.m_backTwohandedMelee);
            if (transform != null)
            {
                visEq.m_backTwohandedMelee = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backTwohandedMelee: {transform.name}");
                m_backTwohandedMelee.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_backTwohandedMelee = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backTwohandedMelee: null");
            }
        }

        if (m_backBow != null)
        {
            Transform? transform = m_backBow.Find(visEq.gameObject, visEq.m_backBow);
            if (transform != null)
            {
                visEq.m_backBow = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backBow: {transform.name}");
                m_backBow.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_backBow = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backBow: null");
            }
        }

        if (m_backTool != null)
        {
            Transform? transform = m_backTool.Find(visEq.gameObject, visEq.m_backTool);
            if (transform != null)
            {
                visEq.m_backTool = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backTool: {transform.name}");
                m_backTool.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_backTool = null;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backTool: null");
            }
        }

        if (m_backAtgeir != null)
        {
            Transform? transform = m_backAtgeir.Find(visEq.gameObject, visEq.m_backAtgeir);
            if (transform != null)
            {
                visEq.m_backAtgeir = transform;
                if (log) MonsterDBPlugin.LogDebug($"[{targetName}] m_backAtgeir: {transform.name}");
                m_backAtgeir.UpdateFields(transform, targetName, log);
            }
            else
            {
                visEq.m_backAtgeir = null;
                if (log) MonsterDBPlugin.LogDebug($"{targetName}] m_backAtgeir: null");
            }
        }
    }

    [HarmonyPatch(typeof(VisEquipment), nameof(VisEquipment.AttachItem))]
    private static class VisEquipment_AttachItem_Patch
    {
        private static void Postfix(VisEquipment __instance, Transform joint, GameObject __result)
        {
            if (__result == null) return;
            if (LoadManager.files.humanoids == null) return;
            if (!LoadManager.files.humanoids.TryGetValue(Utils.GetPrefabName(__instance.name), out BaseHumanoid? data)) return;
            if (data.VisEquipment == null) return;
            TransformRef? reference = data.VisEquipment.Get(joint);
            if (reference == null || !reference.m_scale.HasValue) return;
            __result.transform.localScale = Vector3.Scale(__result.transform.localScale, reference.m_scale.Value);
        }
    }
    
}

[Serializable]
public class TransformRef : Reference
{
    public string? m_name;
    public string? m_parent;
    public int? m_index;
    public Vector3Ref? m_position;
    public Vector3Ref? m_rotation;
    public Vector3Ref? m_scale;

    public TransformRef(){}
    
    public TransformRef(Transform transform)
    {
        m_name = transform.name;
        m_parent = transform.parent.name;
        m_index = transform.GetSiblingIndex();
        m_position = new Vector3Ref(transform.localPosition);
        m_rotation = new Vector3Ref(transform.localRotation.eulerAngles);
        m_scale = new Vector3Ref(transform.localScale);
    }

    public Transform Find(GameObject prefab, Transform defaultValue)
    {
        if (m_name == null) return defaultValue;
        if (m_parent == null || m_index == null)
        {
            if (defaultValue.name == m_name) return defaultValue;
            return Utils.FindChild(prefab.transform, m_name) ?? defaultValue;
        }

        if (defaultValue.name == m_name && 
            defaultValue.parent?.name == m_parent &&
            defaultValue.GetSiblingIndex() == m_index)
        {
            return defaultValue;
        }
        Transform[]? transforms = prefab.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < transforms.Length; ++i)
        {
            Transform transform = transforms[i];
            if (transform.name == m_name && transform.parent?.name == m_parent &&
                transform.GetSiblingIndex() == m_index)
            {
                return transform;
            }
        }

        return Utils.FindChild(prefab.transform, m_name) ?? defaultValue;
    }

    public override void UpdateFields<T>(T target, string targetName, bool log)
    {
        if (target is not Transform transform) return;
        if (m_position.HasValue)
        {
            transform.localPosition = m_position.Value;
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{transform.name}] m_localPosition: {m_position.Value}");
            }
        }

        if (m_rotation.HasValue)
        {
            transform.localRotation = Quaternion.Euler(m_rotation.Value);
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{transform.name}] m_localRotation: {m_rotation.Value}");
            }
        }

        if (m_scale.HasValue)
        {
            transform.localScale = m_scale.Value;
            if (log)
            {
                MonsterDBPlugin.LogDebug($"[{targetName}][{transform.name}] m_localScale: {m_scale.Value}");
            }
        }
    }
}