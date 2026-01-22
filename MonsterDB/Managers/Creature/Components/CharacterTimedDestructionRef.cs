using System;

namespace MonsterDB;

[Serializable]
public class CharacterTimedDestructionRef : Reference
{
    public float? m_timeoutMin;
    public float? m_timeoutMax;
    public bool? m_triggerOnAwake;
    
    public CharacterTimedDestructionRef(){}
    public CharacterTimedDestructionRef(CharacterTimedDestruction component) => Setup(component);
}