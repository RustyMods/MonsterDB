using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB.GlobalModifiers;

public class SE_GlobalModifiers : StatusEffect
{
    private string prefab = "";
    
    public override void Setup(Character character)
    {
        base.Setup(character);
        prefab = Utils.GetPrefabName(character.name);
        
        ModifyHealth(character);
    }

    private void ModifyHealth(Character character)
    {
        if (GlobalManager.mods.GetHealthModifier(prefab, out float mod))
        {
            float maxHealth = character.GetMaxHealth();
            float modifiedHealth = maxHealth * mod;
            character.SetMaxHealth(modifiedHealth);
        }
    }

    public override void ModifyAttack(Skills.SkillType skill, ref HitData hitData)
    {
        if (GlobalManager.mods.GetDamageModifier(prefab, out float mod))
        {
            hitData.ApplyModifier(mod);
        }
    }

    public override void ModifyDamageMods(ref HitData.DamageModifiers modifiers)
    {
        if (GlobalManager.mods.GetDamageModifiers(prefab, out List<HitData.DamageModPair> mods))
        {
            modifiers.Apply(mods);
        }
    }

    public override void ModifySpeed(float baseSpeed, ref float speed, Character character, Vector3 dir)
    {
        if (GlobalManager.mods.GetSpeed(prefab, out float mod))
        {
            speed *= mod;
        }
    }

    public override void ModifyStagger(float baseValue, ref float use)
    {
        if (GlobalManager.mods.GetStaggerDamage(prefab, out float mod))
        {
            use *= mod;
        }
    }
}