using System;
using System.Collections.Generic;

namespace MonsterDB.Solution;

[Serializable]
public class CharacterData
{
    public string ClonedFrom = "";
    public string PrefabName = "";
    public string Name = "";
    public string Group = "";
    public string Faction = "";
    public bool Boss;
    public bool DoNotHideBossHUD;
    public string BossEvent = "";
    public string DefeatSetGlobalKey = "";
    public bool AISkipTarget;
    public float CrouchSpeed;
    public float WalkSpeed;
    public float Speed;
    public float TurnSpeed;
    public float RunSpeed;
    public float RunTurnSpeed;
    public float FlySlowSpeed;
    public float FlyFastSpeed;
    public float FlyTurnSpeed;
    public float Acceleration;
    public float JumpForce;
    public float JumpForceForward;
    public float JumpForceTiredFactor;
    public float AirControl;
    public bool CanSwim;
    public float SwimDepth;
    public float SwimSpeed;
    public float SwimTurnSpeed;
    public float SwimAcceleration;
    public string GroundTilt = "";
    public float GroundTiltSpeed;
    public bool Flying;
    public float JumpStaminaUsage;
    public bool DisableWhileSleeping;
    public bool TolerateWater;
    public bool TolerateFire;
    public bool TolerateSmoke;
    public bool TolerateTar;
    public float Health;
    public string BluntResistance = "Normal";
    public string SlashResistance = "Normal";
    public string PierceResistance = "Normal";
    public string ChopResistance = "Normal";
    public string PickaxeResistance = "Normal";
    public string FireResistance = "Normal";
    public string FrostResistance = "Normal";
    public string LightningResistance = "Normal";
    public string PoisonResistance = "Normal";
    public string SpiritResistance = "Normal";
    public bool StaggerWhenBlocked;
    public float StaggerDamageFactor;
    // public float MinLavaMaskThreshold;
    // public float MaxLavaMaskThreshold;
    // public float HeatBuildupBase;
    // public float HeatCooldownBase;
    // public float HeatBuildupWater;
    // public float HeatWaterTouchMultiplier;
    // public float LavaDamageTickInterval;
    // public float HeatLevelFirstDamageThreshold;
    // public float LavaFirstDamage;
    // public float LavaFullDamage;
    // public float LavaAirDamageHeight;
    // public float DayHeatGainRunning;
    // public float DayHeatGainStill;
    // public float DayHeatEquipmentStop;
    // public float LavaSlowMax;
    // public float LavaSlowHeight;
    // public float EquipStaminaDrain;
    // public float BlockStaminaDrain;
}

[Serializable]
public class EffectInfo
{
    public string PrefabName = "";
    public bool Enabled;
    public int Variant = -1;
    public bool Attach;
    public bool Follow;
    public bool InheritParentRotation;
    public bool InheritParentScale;
    public bool MultiplyParentVisualScale;
    public bool RandomRotation;
    public bool Scale;
    public string ChildTransform = "";
}

[Serializable]
public class AttackData
{
    public string OriginalPrefab = "";
    public string Name = "";
    public string AnimationState = "";
    public int ToolTier;
    public float Damage;
    public float Blunt;
    public float Slash;
    public float Pierce;
    public float Chop;
    public float Pickaxe;
    public float Fire;
    public float Frost;
    public float Lightning;
    public float Poison;
    public float Spirit;
    public float AttackForce;
    public bool Dodgeable;
    public bool Blockable;
    public string SpawnOnHit = "";
    public string SpawnOnHitTerrain = "";
    public string AttackStatusEffect = "";
    public float AttackStatusEffectChance;
    public string AttackType = "";
    public string AttackAnimation = "";
    public bool HitTerrain;
    public bool HitFriendly;
    public float DamageMultiplier;
    public float DamageMultiplierPerMissingHP;
    public float DamageMultiplierByTotalHealthMissing;
    public float ForceMultiplier;
    public float StaggerMultiplier;
    public float RecoilPushback;
    public int SelfDamage;
    public string AttackOriginJoint = "";
    public float AttackRange;
    public float AttackHeight;
    public float AttackOffset;
    public string SpawnOnTrigger = "";
    public bool ToggleFlying;
    public bool Attach;
    public float AttackAngle;
    public float AttackRayWidth;
    public float MaxYAngle;
    public bool LowerDamagePerHit;
    public bool HitThroughWalls;
    public float AttackRangeMinimum;
    public float AttackInterval;
    public float AttackMaxAngle;
    public string Projectile = "";
}

[Serializable]
public class RandomItemData
{
    public string PrefabName = "";
    public float Chance;
}

[Serializable]
public class ProjectileData
{
    public string Name = "";
    public float Damage;
    public float Blunt;
    public float Slash;
    public float Pierce;
    public float Chop;
    public float Pickaxe;
    public float Fire;
    public float Frost;
    public float Lightning;
    public float Poison;
    public float Spirit;
    public float AOE;
    public bool Dodgeable;
    public bool Blockable;
    public float AttackForce;
    public float BackstabBonus;
    public string StatusEffect = "";
    public float HealthReturn;
    public bool CanHitWater;
    public float Duration;
    public float Gravity;
    public float Drag;
    public float RayRadius;
    public bool StayAfterHitStatic;
    public bool StayAfterHitDynamic;
    public float StayDuration;
    public bool Bounce;
    public bool BounceOnWater;
    public float BouncePower;
    public float BounceRoughness;
    public int MaxBounces;
    public float MinBounceVelocity;
    public bool RespawnItemOnHit;
    public bool SpawnOnDuration;
    public string SpawnOnHit = "";
    public float SpawnOnHitChance;
    public int SpawnCount;
    public List<string> RandomSpawnOnHit = new();
    public int RandomSpawnOnHitCount;
    public bool RandomSpawnSkipLava;
    public bool StaticHitOnly;
    public bool GroundHitOnly;
    public bool SpawnRandomRotation;
}

[Serializable]
public class MonsterAIData
{
    public float ViewRange;
    public float ViewAngle;
    public float HearRange;
    public bool MistVision;
    public float IdleSoundInterval;
    public float IdleSoundChance;
    public string PathAgentType = "";
    public float MoveMinAngle;
    public bool SmoothMovement;
    public bool SerpentMovement;
    public float SerpentTurnRadius;
    public float JumpInterval;
    public float RandomCircleInterval;
    public float RandomMoveInterval;
    public float RandomMoveRange;
    public bool RandomFly;
    public float ChanceToTakeOff;
    public float ChanceToLand;
    public float GroundDuration;
    public float AirDuration;
    public float MaxLandAltitude;
    public float TakeoffTime;
    public float FlyAltitudeMin;
    public float FlyAltitudeMax;
    public float FlyAbsMinAltitude;
    public bool AvoidFire;
    public bool AfraidOfFire;
    public bool AvoidWater;
    public bool AvoidLava;
    public bool SkipLavaTargets;
    public bool Aggravatable;
    public bool PassiveAggressive;
    public string SpawnMessage = "";
    public string DeathMessage = "";
    public string AlertedMessage = "";
    public float FleeRange;
    public float FleeAngle;
    public float FleeInterval;
    public float AlertRange;
    public bool FleeIfHurtWhenTargetCannotBeReached;
    public float FleeUnreachableSinceAttack;
    public float FleeUnreachableSinceHurt;
    public bool FleeIfNotAlerted;
    public float FleeIfLowHealth;
    public float FleeTimeSinceHurt;
    public bool FleeInLava;
    public bool CirculateWhileCharging;
    public bool CirculateWhileChargingFlying;
    public bool EnableHuntPlayer;
    public bool AttackPlayerObjects;
    public int PrivateAreaTriggerThreshold;
    public float InterceptTimeMax;
    public float InterceptTimeMin;
    public float MaxChaseDistance;
    public float MinAttackInterval;
    public float CircleTargetInterval;
    public float CircleTargetDuration;
    public float CircleTargetDistance;
    public bool Sleeping;
    public float WakeupRange;
    public bool NoiseWakeup;
    public float MaxNoiseWakeupRange;
    public float WakeupDelayMin;
    public float WakeupDelayMax;
    public bool AvoidLand;
    public List<string> ConsumeItems = new();
    public float ConsumeRange;
    public float ConsumeSearchRange;
    public float ConsumeSearchInterval;
}

[Serializable]
public class AnimalAIData
{
    public float ViewRange;
    public float ViewAngle;
    public float HearRange;
    public bool MistVision;
    public float IdleSoundInterval;
    public float IdleSoundChance;
    public string PathAgentType = "";
    public float MoveMinAngle;
    public bool SmoothMovement;
    public bool SerpentMovement;
    public float SerpentTurnRadius;
    public float JumpInterval;
    public float RandomCircleInterval;
    public float RandomMoveInterval;
    public float RandomMoveRange;
    public bool RandomFly;
    public float ChanceToTakeOff;
    public float ChanceToLand;
    public float GroundDuration;
    public float AirDuration;
    public float MaxLandAltitude;
    public float TakeoffTime;
    public float FlyAltitudeMin;
    public float FlyAltitudeMax;
    public float FlyAbsMinAltitude;
    public bool AvoidFire;
    public bool AfraidOfFire;
    public bool AvoidWater;
    public bool AvoidLava;
    public bool SkipLavaTargets;
    public bool Aggravatable;
    public bool PassiveAggressive;
    public string SpawnMessage = "";
    public string DeathMessage = "";
    public string AlertedMessage = "";
    public float FleeRange;
    public float FleeAngle;
    public float FleeInterval;
}

[Serializable]
public class CharacterDropData
{
    public string PrefabName = "";
    public int AmountMin;
    public int AmountMax;
    public float Chance;
    public bool OnePerPlayer;
    public bool LevelMultiplier;
    public bool DoNotScale;
}

[Serializable]
public class TameableData
{
    public float FedDuration;
    public float TamingTime;
    public bool StartTamed;
    public bool Commandable;
    public float UnsummonDistance;
    public float UnsummonOnOwnerLogoutSeconds;
    public string LevelUpOwnerSkill = "";
    public float LevelUpFactor;
    public bool DropSaddleOnDeath;
    public List<string> RandomStartingName = new();
}

[Serializable]
public class ProcreationData
{
    public float UpdateInterval;
    public float TotalCheckRange;
    public int MaxCreatures;
    public float PartnerCheckRange;
    public float PregnancyChance;
    public float PregnancyDuration;
    public int RequiredLovePoints;
    public string Offspring = "";
    public int MinOffspringLevel;
    public float SpawnOffset;
}

[Serializable]
public class NPCTalkData
{
    public string Name = "";
    public float MaxRange;
    public float GreetRange;
    public float ByeRange;
    public float Offset;
    public float MinTalkInterval;
    public float HideDialogueDelay;
    public float RandomTalkInterval;
    public float RandomTalkChance;
    public List<string> RandomTalk = new();
    public List<string> RandomTalkInFactionBase = new();
    public List<string> RandomGreets = new();
    public List<string> RandomGoodbye = new();
    public List<string> PrivateAreaAlarm = new();
    public List<string> Aggravated = new();
}