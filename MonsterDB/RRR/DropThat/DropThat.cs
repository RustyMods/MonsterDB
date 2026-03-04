namespace MonsterDB;

public class DropThat
{
    public string creature = "";
    public int index = 0;
    public string prefab = "";
    public int min = 1;
    public int max = 1;
    public float chance = 100f;
    public bool onePerPlayer = false;
    public bool scaleByLevel = true;
    public bool enabled = true;

    public bool isValid => !string.IsNullOrEmpty(prefab) && !string.IsNullOrEmpty(creature);

    public DropRef ToDropRef() => new DropRef
    {
        m_prefab = prefab,
        m_amountMin = min,
        m_amountMax = max,
        m_chance = chance,
        m_onePerPlayer = onePerPlayer,
        m_dontScale = scaleByLevel,
    };
}