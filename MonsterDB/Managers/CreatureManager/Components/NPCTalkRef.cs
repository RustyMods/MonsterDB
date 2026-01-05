using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonsterDB;

[Serializable][UsedImplicitly]
public class NPCTalkRef : Reference
{
    public string? m_name;
    public float? m_maxRange;
    public float? m_greetRange;
    public float? m_byeRange;
    public float? m_offset;
    public float? m_minTalkInterval;
    public float? m_hideDialogDelay;
    public float? m_randomTalkInterval;
    public float? m_randomTalkChance;
    public List<string>? m_randomTalk;
    public List<string>? m_randomTalkInFactionBase;
    public List<string>? m_randomGreets;
    public List<string>? m_randomGoodbye;
    public List<string>? m_privateAreaAlarm;
    public List<string>? m_aggravated;
    public EffectListRef? m_randomTalkFX;
    public EffectListRef? m_randomGreetFX;
    public EffectListRef? m_randomGoodbyeFX;

    public static implicit operator NPCTalkRef(NpcTalk npcTalk)
    {
        NPCTalkRef reference = new NPCTalkRef();
        reference.ReferenceFrom(npcTalk);
        return reference;
    }
}