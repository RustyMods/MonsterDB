using System;
using YamlDotNet.Serialization;

namespace MonsterDB;

[Serializable]
public class TeleportAbilityRef : Reference
{
    [YamlMember(Description = "Untagged, Respawn, Finish, Player, snappoint, StationUseArea, roof, minerock, antimist, SeekerQueenTeleportTarget")]
    public string? m_targetTag = "";
    public string? m_message = "";
    public float? m_maxTeleportRange = 100f;

    public static implicit operator TeleportAbilityRef(TeleportAbility ta)
    {
        TeleportAbilityRef reference = new TeleportAbilityRef();
        reference.Setup(ta);
        return reference;
    }

    public void Update(TeleportAbility ta, string targetName)
    {
        UpdateFields(ta, targetName, true);
    }
}