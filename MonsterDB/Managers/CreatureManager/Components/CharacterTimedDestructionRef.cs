namespace MonsterDB;

public class CharacterTimedDestructionRef : Reference
{
    public float? m_timeoutMin;
    public float? m_timeoutMax;
    public bool? m_triggerOnAwake;

    public static implicit operator CharacterTimedDestructionRef(CharacterTimedDestruction ctd)
    {
        CharacterTimedDestructionRef reference = new CharacterTimedDestructionRef();
        reference.ReferenceFrom(ctd);
        return reference;
    }
}