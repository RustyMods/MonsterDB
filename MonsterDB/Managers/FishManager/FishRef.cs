using System;
using System.Collections.Generic;

namespace MonsterDB;

[Serializable]
public class FishRef : Reference
{
    public string? m_name;
    public float? m_swimRange;
    public float? m_minDepth;
    public float? m_maxDepth;
    public float? m_speed;
    public float? m_acceleration;
    public float? m_turnRate;
    public float? m_wpDurationMin;
    public float? m_wpDurationMax;
    public float? m_avoidSpeedScale;
    public float? m_avoidRange;
    public float? m_height;
    public float? m_hookForce;
    public float? m_staminaUse;
    public float? m_escapeStaminaUse;
    public float? m_escapeMin;
    public float? m_escapeMax;
    public float? m_escapeWaitMin;
    public float? m_escapeWaitMax;
    public float? m_escapeMaxPerLevel;
    public float? m_baseHookChance;
    public int? m_pickupItemStackSize;
    public float? m_blockChangeDurationMin;
    public float? m_blockChangeDurationMax;
    public float? m_collisionFleeTimeout;
    public List<BaitSettingRef>? m_baits;
    public DropTableRef? m_extraDrops;
    public float? m_jumpSpeed;
    public float? m_jumpHeight;
    public float? m_jumpForwardStrength;
    public float? m_jumpHeightLand;
    public float? m_jumpChance;
    public float? m_jumpOnLandChance;
    public float? m_jumpOnLandDecay;
    public float? m_maxJumpDepthOffset;
    public float? m_jumpFrequencySeconds;
    public float? m_jumpOnLandRotation;
    public float? m_waveJumpMultiplier;
    public float? m_jumpMaxLevel;
    public EffectListRef? m_jumpEffects;

    public static implicit operator FishRef(Fish fish)
    {
        FishRef reference = new FishRef();
        reference.SetFrom(fish);
        return reference;
    }
}