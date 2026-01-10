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
    
    
}