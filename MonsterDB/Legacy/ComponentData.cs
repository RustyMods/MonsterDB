using System;
using System.Collections.Generic;
using System.Linq;
using MonsterDB.Solution.Methods;

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

    public void Set(ref CharacterRef? r)
    {
        if (r == null)
        {
            r = new  CharacterRef();
        }
        r.m_name = Name;
        r.m_group = Group;

        r.m_faction = FactionManager.GetFaction(Faction);

        r.m_boss = Boss;
        r.m_dontHideBossHud = DoNotHideBossHUD;
        r.m_bossEvent = BossEvent;
        r.m_defeatSetGlobalKey = DefeatSetGlobalKey;
        r.m_aiSkipTarget = AISkipTarget;

        r.m_crouchSpeed = CrouchSpeed;
        r.m_walkSpeed = WalkSpeed;
        r.m_speed = Speed;
        r.m_turnSpeed = TurnSpeed;
        r.m_runSpeed = RunSpeed;
        r.m_runTurnSpeed = RunTurnSpeed;
        r.m_acceleration = Acceleration;

        r.m_jumpForce = JumpForce;
        r.m_jumpForceForward = JumpForceForward;
        r.m_jumpForceTiredFactor = JumpForceTiredFactor;
        r.m_airControl = AirControl;
        r.m_jumpStaminaUsage = JumpStaminaUsage;

        r.m_flying = Flying;
        r.m_flySlowSpeed = FlySlowSpeed;
        r.m_flyFastSpeed = FlyFastSpeed;
        r.m_flyTurnSpeed = FlyTurnSpeed;

        r.m_canSwim = CanSwim;
        r.m_swimDepth = SwimDepth;
        r.m_swimSpeed = SwimSpeed;
        r.m_swimTurnSpeed = SwimTurnSpeed;
        r.m_swimAcceleration = SwimAcceleration;

        if (!string.IsNullOrEmpty(GroundTilt) &&
            Enum.TryParse(GroundTilt, true, out Character.GroundTiltType groundTilt))
        {
            r.m_groundTilt = groundTilt;
        }

        r.m_groundTiltSpeed = GroundTiltSpeed;

        r.m_disableWhileSleeping = DisableWhileSleeping;
        r.m_tolerateWater = TolerateWater;
        r.m_tolerateFire = TolerateFire;
        r.m_tolerateSmoke = TolerateSmoke;
        r.m_tolerateTar = TolerateTar;

        r.m_health = Health;
        r.m_staggerWhenBlocked = StaggerWhenBlocked;
        r.m_staggerDamageFactor = StaggerDamageFactor;

        r.m_damageModifiers = new HitData.DamageModifiers()
        {
            m_blunt = Enum.TryParse(BluntResistance, true, out HitData.DamageModifier b) ? b : HitData.DamageModifier.Normal,
            m_pierce = Enum.TryParse(PierceResistance, true, out HitData.DamageModifier p) ? p : HitData.DamageModifier.Normal,
            m_slash = Enum.TryParse(SlashResistance, true, out HitData.DamageModifier s) ? s : HitData.DamageModifier.Normal,
            m_pickaxe = Enum.TryParse(PickaxeResistance, true, out HitData.DamageModifier pa) ? pa : HitData.DamageModifier.Normal,
            m_chop = Enum.TryParse(ChopResistance, true, out HitData.DamageModifier c) ? c : HitData.DamageModifier.Normal,
            m_fire = Enum.TryParse(FireResistance, true, out HitData.DamageModifier f) ? f : HitData.DamageModifier.Normal,
            m_frost = Enum.TryParse(FrostResistance, true, out HitData.DamageModifier fr) ? fr : HitData.DamageModifier.Normal,
            m_poison = Enum.TryParse(PoisonResistance, true, out HitData.DamageModifier po) ? po : HitData.DamageModifier.Normal,
            m_lightning = Enum.TryParse(LightningResistance, true, out HitData.DamageModifier l) ? l : HitData.DamageModifier.Normal,
            m_spirit = Enum.TryParse(SpiritResistance, true, out HitData.DamageModifier sp) ? sp : HitData.DamageModifier.Normal,
        };
    }
    
    public void Set(ref HumanoidRef? r)
    {
        if (r == null)
        {
            r = new  HumanoidRef();
        }
        r.m_name = Name;
        r.m_group = Group;

        r.m_faction = FactionManager.GetFaction(Faction);

        r.m_boss = Boss;
        r.m_dontHideBossHud = DoNotHideBossHUD;
        r.m_bossEvent = BossEvent;
        r.m_defeatSetGlobalKey = DefeatSetGlobalKey;
        r.m_aiSkipTarget = AISkipTarget;

        r.m_crouchSpeed = CrouchSpeed;
        r.m_walkSpeed = WalkSpeed;
        r.m_speed = Speed;
        r.m_turnSpeed = TurnSpeed;
        r.m_runSpeed = RunSpeed;
        r.m_runTurnSpeed = RunTurnSpeed;
        r.m_acceleration = Acceleration;

        r.m_jumpForce = JumpForce;
        r.m_jumpForceForward = JumpForceForward;
        r.m_jumpForceTiredFactor = JumpForceTiredFactor;
        r.m_airControl = AirControl;
        r.m_jumpStaminaUsage = JumpStaminaUsage;

        r.m_flying = Flying;
        r.m_flySlowSpeed = FlySlowSpeed;
        r.m_flyFastSpeed = FlyFastSpeed;
        r.m_flyTurnSpeed = FlyTurnSpeed;

        r.m_canSwim = CanSwim;
        r.m_swimDepth = SwimDepth;
        r.m_swimSpeed = SwimSpeed;
        r.m_swimTurnSpeed = SwimTurnSpeed;
        r.m_swimAcceleration = SwimAcceleration;

        if (!string.IsNullOrEmpty(GroundTilt) &&
            Enum.TryParse(GroundTilt, true, out Character.GroundTiltType groundTilt))
        {
            r.m_groundTilt = groundTilt;
        }

        r.m_groundTiltSpeed = GroundTiltSpeed;

        r.m_disableWhileSleeping = DisableWhileSleeping;
        r.m_tolerateWater = TolerateWater;
        r.m_tolerateFire = TolerateFire;
        r.m_tolerateSmoke = TolerateSmoke;
        r.m_tolerateTar = TolerateTar;

        r.m_health = Health;
        r.m_staggerWhenBlocked = StaggerWhenBlocked;
        r.m_staggerDamageFactor = StaggerDamageFactor;

        r.m_damageModifiers = new HitData.DamageModifiers()
        {
            m_blunt = Enum.TryParse(BluntResistance, true, out HitData.DamageModifier b) ? b : HitData.DamageModifier.Normal,
            m_pierce = Enum.TryParse(PierceResistance, true, out HitData.DamageModifier p) ? p : HitData.DamageModifier.Normal,
            m_slash = Enum.TryParse(SlashResistance, true, out HitData.DamageModifier s) ? s : HitData.DamageModifier.Normal,
            m_pickaxe = Enum.TryParse(PickaxeResistance, true, out HitData.DamageModifier pa) ? pa : HitData.DamageModifier.Normal,
            m_chop = Enum.TryParse(ChopResistance, true, out HitData.DamageModifier c) ? c : HitData.DamageModifier.Normal,
            m_fire = Enum.TryParse(FireResistance, true, out HitData.DamageModifier f) ? f : HitData.DamageModifier.Normal,
            m_frost = Enum.TryParse(FrostResistance, true, out HitData.DamageModifier fr) ? fr : HitData.DamageModifier.Normal,
            m_poison = Enum.TryParse(PoisonResistance, true, out HitData.DamageModifier po) ? po : HitData.DamageModifier.Normal,
            m_lightning = Enum.TryParse(LightningResistance, true, out HitData.DamageModifier l) ? l : HitData.DamageModifier.Normal,
            m_spirit = Enum.TryParse(SpiritResistance, true, out HitData.DamageModifier sp) ? sp : HitData.DamageModifier.Normal,
        };
    }
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

    public EffectListRef.EffectDataRef ToRef()
    {
        var data = new EffectListRef.EffectDataRef();
        data.m_prefab = PrefabName;
        data.m_variant = Variant;
        data.m_attach = Attach;
        data.m_follow = Follow;
        data.m_inheritParentRotation = InheritParentRotation;
        data.m_inheritParentScale = InheritParentScale;
        data.m_multiplyParentVisualScale = MultiplyParentVisualScale;
        data.m_randomRotation = RandomRotation;
        data.m_scale = Scale;
        data.m_childTransform = ChildTransform;
        return data;
    }
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
    public string MaterialOverride = "";

    public void Set(ref ItemDataSharedRef r)
    {
        r.m_prefab = Name;
        r.m_toolTier = ToolTier;
        r.m_damages = new HitData.DamageTypes()
        {
            m_damage = Damage,
            m_blunt = Blunt,
            m_slash = Slash,
            m_pierce = Pierce,
            m_chop = Chop,
            m_pickaxe = Pickaxe,
            m_fire = Fire,
            m_frost = Frost,
            m_lightning = Lightning,
            m_poison = Poison,
            m_spirit = Spirit,
        };
        r.m_attackForce = AttackForce;
        r.m_dodgeable = Dodgeable;
        r.m_blockable = Blockable;
        
        if (r.m_attack == null)
        {
            r.m_attack = new AttackRef();
        }

        r.m_attack.m_attackAnimation = AttackAnimation;
        r.m_attack.m_spawnOnHit = SpawnOnHit;
        r.m_attack.m_spawnOnTrigger = SpawnOnTrigger;
        
        r.m_attackStatusEffect = AttackStatusEffect;
        r.m_attackStatusEffectChance = AttackStatusEffectChance;
        
        if (Enum.TryParse(AttackType, true, out Attack.AttackType at))
        {
            r.m_attack.m_attackType = at;
        }

        r.m_attack.m_hitTerrain = HitTerrain;
        r.m_attack.m_damageMultiplier = DamageMultiplier;
        r.m_attack.m_damageMultiplierPerMissingHP = DamageMultiplierPerMissingHP;
        r.m_attack.m_damageMultiplierByTotalHealthMissing = DamageMultiplierByTotalHealthMissing;
        r.m_attack.m_forceMultiplier = ForceMultiplier;
        r.m_attack.m_staggerMultiplier = StaggerMultiplier;
        r.m_attack.m_recoilPushback = RecoilPushback;
        r.m_attack.m_selfDamage = SelfDamage;
        r.m_attack.m_attackOriginJoint = AttackOriginJoint;
        r.m_attack.m_attackRange = AttackRange;
        r.m_attack.m_attackHeight = AttackHeight;
        r.m_attack.m_attackOffset = AttackOffset;
        r.m_attack.m_toggleFlying = ToggleFlying;
        r.m_attack.m_attach = Attach;
        r.m_attack.m_attackAngle = AttackAngle;
        r.m_attack.m_attackRayWidth = AttackRayWidth;
        r.m_attack.m_maxYAngle = MaxYAngle;
        r.m_attack.m_lowerDamagePerHit = LowerDamagePerHit;
        r.m_attack.m_hitThroughWalls = HitThroughWalls;
        
        r.m_aiAttackRangeMin = AttackRangeMinimum;
        r.m_aiAttackInterval = AttackInterval;
        r.m_aiAttackMaxAngle = AttackMaxAngle;
    }
}

[Serializable]
public class RandomItemData
{
    public string PrefabName = "";
    public float Chance;

    public void Set(ref HumanoidRef.RandomItem r)
    {
        r.m_prefab = PrefabName;
        r.m_chance = Chance;
    }

    public HumanoidRef.RandomItem ToRef()
    {
        var r = new HumanoidRef.RandomItem();
        r.m_prefab = PrefabName;
        r.m_chance = Chance;
        return r;
    }
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

    public void Set(ref ProjectileRef? r)
    {
        if (r == null) return;
        r.m_damage = new HitData.DamageTypes()
        {
            m_damage = Damage,
            m_blunt = Blunt,
            m_slash = Slash,
            m_pierce = Pierce,
            m_chop = Chop,
            m_pickaxe = Pickaxe,
            m_fire = Fire,
            m_frost = Frost,
            m_lightning = Lightning,
            m_poison = Poison,
            m_spirit = Spirit,
        };
        r.m_aoe = AOE;
        r.m_dodgeable = Dodgeable;
        r.m_blockable = Blockable;
        r.m_attackForce = AttackForce;
        r.m_backstabBonus = BackstabBonus;
        r.m_statusEffect =  StatusEffect;
        r.m_healthReturn = HealthReturn;
        r.m_bounce = Bounce;
        r.m_bounceOnWater = BounceOnWater;
        r.m_respawnItemOnHit =  RespawnItemOnHit;
        r.m_spawnOnHit = SpawnOnHit;
        r.m_spawnOnHitChance =  SpawnOnHitChance;
        r.m_spawnCount = SpawnCount;
        r.m_randomSpawnOnHit = RandomSpawnOnHit;
        r.m_randomSpawnOnHitCount = RandomSpawnOnHitCount;
        r.m_randomSpawnSkipLava = RandomSpawnSkipLava;
        r.m_staticHitOnly = StaticHitOnly;
        r.m_groundHitOnly = GroundHitOnly;
        r.m_spawnRandomRotation = SpawnRandomRotation;
    }
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

    public void Set(ref MonsterAIRef? r)
    {
        if (r == null)
        {
            r = new  MonsterAIRef();
        }
        r.m_viewRange =  ViewRange;
        r.m_viewAngle =  ViewAngle;
        r.m_hearRange =  HearRange;
        r.m_mistVision =  MistVision;
        r.m_idleSoundInterval =  IdleSoundInterval;
        r.m_idleSoundChance =  IdleSoundChance;
        if (Enum.TryParse(PathAgentType, true, out Pathfinding.AgentType pa))
        {
            r.m_pathAgentType = pa;
        }
        r.m_moveMinAngle =  MoveMinAngle;
        r.m_smoothMovement =  SmoothMovement;
        r.m_serpentMovement =  SerpentMovement;
        r.m_serpentTurnRadius =  SerpentTurnRadius;
        r.m_jumpInterval =  JumpInterval;
        r.m_randomCircleInterval =  RandomCircleInterval;
        r.m_randomMoveInterval =  RandomMoveInterval;
        r.m_randomMoveRange =  RandomMoveRange;
        r.m_randomFly =  RandomFly;
        r.m_chanceToTakeoff = ChanceToTakeOff;
        r.m_chanceToLand =  ChanceToLand;
        r.m_groundDuration =  GroundDuration;
        r.m_airDuration =  AirDuration;
        r.m_maxLandAltitude =  MaxLandAltitude;
        r.m_takeoffTime =  TakeoffTime;
        r.m_flyAltitudeMin  =  FlyAltitudeMin;
        r.m_flyAltitudeMax  =  FlyAltitudeMax;
        r.m_flyAbsMinAltitude = FlyAbsMinAltitude;
        r.m_avoidFire =  AvoidFire;
        r.m_afraidOfFire  =  AfraidOfFire;
        r.m_avoidWater =  AvoidWater;
        r.m_avoidLava =  AvoidLava;
        r.m_skipLavaTargets = SkipLavaTargets;
        r.m_aggravatable =   Aggravatable;
        r.m_passiveAggresive =  PassiveAggressive;
        r.m_spawnMessage = SpawnMessage;
        r.m_deathMessage = DeathMessage;
        r.m_alertedMessage = AlertedMessage;
        r.m_fleeRange =  FleeRange;
        r.m_fleeAngle =  FleeAngle;
        r.m_fleeInterval =  FleeInterval;
        r.m_alertRange =  AlertRange;
        r.m_fleeIfHurtWhenTargetCantBeReached = FleeIfHurtWhenTargetCannotBeReached;
        r.m_fleeUnreachableSinceAttacking = FleeUnreachableSinceAttack;
        r.m_fleeUnreachableSinceHurt = FleeUnreachableSinceHurt;
        r.m_fleeIfNotAlerted =  FleeIfNotAlerted;
        r.m_fleeIfLowHealth =  FleeIfLowHealth;
        r.m_fleeTimeSinceHurt = FleeTimeSinceHurt;
        r.m_fleeInLava =  FleeInLava;
        r.m_circulateWhileCharging =   CirculateWhileCharging;
        r.m_circulateWhileChargingFlying  =   CirculateWhileChargingFlying;
        r.m_enableHuntPlayer = EnableHuntPlayer;
        r.m_attackPlayerObjects = AttackPlayerObjects;
        r.m_privateAreaTriggerTreshold = PrivateAreaTriggerThreshold;
        r.m_interceptTimeMax =  InterceptTimeMax;
        r.m_interceptTimeMin =  InterceptTimeMin;
        r.m_maxChaseDistance =  MaxChaseDistance;
        r.m_minAttackInterval = MinAttackInterval;
        r.m_circleTargetInterval =  CircleTargetInterval;
        r.m_circleTargetDuration =   CircleTargetDuration;
        r.m_circleTargetDistance =   CircleTargetDistance;
        r.m_sleeping = Sleeping;
        r.m_wakeupRange =   WakeupRange;
        r.m_noiseWakeup =  NoiseWakeup;
        r.m_maxNoiseWakeupRange =  MaxNoiseWakeupRange;
        r.m_wakeUpDelayMin = WakeupDelayMin;
        r.m_wakeUpDelayMax = WakeupDelayMax;
        r.m_avoidLand =  AvoidLand;
        r.m_consumeItems = ConsumeItems;
        r.m_consumeRange =  ConsumeRange;
        r.m_consumeSearchRange =   ConsumeSearchRange;
        r.m_consumeSearchInterval = ConsumeSearchInterval;
    }
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

    public void Set(ref AnimalAIRef? reference)
    {
        if (reference == null)
        {
            reference = new AnimalAIRef();
        }

        reference.m_viewRange = ViewRange;
        reference.m_viewAngle = ViewAngle;
        reference.m_hearRange = HearRange;
        reference.m_mistVision = MistVision;
        reference.m_idleSoundInterval = IdleSoundInterval;
        reference.m_idleSoundChance = IdleSoundChance;
        reference.m_pathAgentType = Enum.TryParse(PathAgentType, true, out Pathfinding.AgentType t) ? t : Pathfinding.AgentType.Humanoid;
        reference.m_moveMinAngle = MoveMinAngle;
        reference.m_smoothMovement = SmoothMovement;
        reference.m_serpentMovement = SerpentMovement;
        reference.m_serpentTurnRadius = SerpentTurnRadius;
        reference.m_jumpInterval = JumpInterval;
        reference.m_randomCircleInterval = RandomCircleInterval;
        reference.m_randomMoveInterval = RandomMoveInterval;
        reference.m_randomMoveRange = RandomMoveRange;
        reference.m_randomFly = RandomFly;
        reference.m_chanceToTakeoff = ChanceToTakeOff;
        reference.m_chanceToLand = ChanceToLand;
        reference.m_groundDuration = GroundDuration;
        reference.m_airDuration = AirDuration;
        reference.m_maxLandAltitude = MaxLandAltitude;
        reference.m_takeoffTime = TakeoffTime;
        reference.m_flyAltitudeMin = FlyAltitudeMin;
        reference.m_flyAltitudeMax = FlyAltitudeMax;
        reference.m_flyAbsMinAltitude = FlyAbsMinAltitude;
        reference.m_avoidFire = AvoidFire;
        reference.m_afraidOfFire = AfraidOfFire;
        reference.m_avoidWater = AvoidWater;
        reference.m_avoidLava = AvoidLava;
        reference.m_skipLavaTargets = SkipLavaTargets;
        reference.m_aggravatable = Aggravatable;
        reference.m_passiveAggresive = PassiveAggressive;
        reference.m_spawnMessage = SpawnMessage;
        reference.m_deathMessage = DeathMessage;
        reference.m_alertedMessage = AlertedMessage;
        reference.m_fleeRange = FleeRange;
        reference.m_fleeAngle = FleeAngle;
        reference.m_fleeInterval = FleeInterval;
    }

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

    public void Set(ref DropRef r)
    {
        r.m_prefab =  PrefabName;
        r.m_amountMin = AmountMin;
        r.m_amountMax = AmountMax;
        r.m_chance =  Chance;
        r.m_onePerPlayer = OnePerPlayer;
        r.m_levelMultiplier = LevelMultiplier;
        r.m_dontScale =  DoNotScale;
    }

    public DropRef ToRef()
    {
        var r = new DropRef();
        r.m_prefab =  PrefabName;
        r.m_amountMin = AmountMin;
        r.m_amountMax = AmountMax;
        r.m_chance =  Chance;
        r.m_onePerPlayer = OnePerPlayer;
        r.m_levelMultiplier = LevelMultiplier;
        r.m_dontScale =  DoNotScale;
        return r;
    }
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

    public void Set(ref TameableRef? r)
    {
        if (TamingTime == 0) return;
        if (r == null)
        {
            r = new TameableRef();
        }

        r.m_fedDuration = FedDuration;
        r.m_tamingTime = TamingTime;
        r.m_startsTamed = StartTamed;
        r.m_commandable = Commandable;
        r.m_unsummonDistance = UnsummonDistance;
        r.m_unsummonOnOwnerLogoutSeconds = UnsummonOnOwnerLogoutSeconds;
        r.m_levelUpOwnerSkill = Enum.TryParse(LevelUpOwnerSkill, true, out Skills.SkillType t) ? t : null;
        r.m_levelUpFactor = LevelUpFactor;
        r.m_dropSaddleOnDeath = DropSaddleOnDeath;
        r.m_randomStartingName = RandomStartingName;
    }
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

    public void Set(ref ProcreationRef? r)
    {
        if (r == null) return;
        r.m_updateInterval =  UpdateInterval;
        r.m_totalCheckRange = TotalCheckRange;
        r.m_maxCreatures = MaxCreatures;
        r.m_partnerCheckRange = PartnerCheckRange;
        r.m_pregnancyChance = PregnancyChance;
        r.m_pregnancyDuration = PregnancyDuration;
        r.m_requiredLovePoints = RequiredLovePoints;
        r.m_offspring = Offspring;
        r.m_spawnOffset = SpawnOffset;
        r.m_minOffspringLevel =  MinOffspringLevel;
    }
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

    public void Set(ref NPCTalkRef? r)
    {
        if (r == null) return;
        r.m_name = Name;
        r.m_maxRange = MaxRange;
        r.m_greetRange = GreetRange;
        r.m_byeRange = ByeRange;
        r.m_offset = Offset;
        r.m_minTalkInterval = MinTalkInterval;
        r.m_hideDialogDelay =  HideDialogueDelay;
        r.m_randomTalkInterval = RandomTalkInterval;
        r.m_randomTalkChance = RandomTalkChance;
        r.m_randomTalk =  RandomTalk;
        r.m_randomTalkInFactionBase = RandomTalkInFactionBase;
        r.m_randomGreets = RandomGreets;
        r.m_randomGoodbye = RandomGoodbye;
        r.m_privateAreaAlarm = PrivateAreaAlarm;
        r.m_aggravated =  Aggravated;
    }
}

[Serializable]
public class GrowUpData
{
    public float GrowTime;
    public bool InheritTame;
    public string GrownPrefab = "";
    public List<AltGrownData> AltGrownPrefabs = new();

    public void Set(ref GrowUpRef? r)
    {
        if (r == null) return;
        r.m_growTime = GrowTime;
        r.m_inheritTame =  InheritTame;
        r.m_grownPrefab = GrownPrefab;
        if (AltGrownPrefabs.Count != 0)
        {
            r.m_altGrownPrefabs = AltGrownPrefabs.Select(x => x.ToRef()).ToList();
        }
    }
}

[Serializable]
public class AltGrownData
{
    public string GrownPrefab = "";
    public float Weight;

    public GrowUpRef.GrownEntry ToRef()
    {
        var r = new  GrowUpRef.GrownEntry();
        r.m_prefab =  GrownPrefab;
        r.m_weight = Weight;
        return r;
    }
}

[Serializable]
public class LevelEffectData
{
    public float Scale;
    public float Hue;
    public float Saturation;
    public float Value;
    public bool SetEmissiveColor;
    public VisualMethods.ColorData EmissiveColor = new();
    public string EnableObject = "";

    public LevelSetupRef ToRef()
    {
        var r = new LevelSetupRef();
        r.m_scale = Scale;
        r.m_hue = Hue;
        r.m_saturation = Saturation;
        r.m_value = Value;
        r.m_setEmissiveColor = SetEmissiveColor;
        r.m_enableObject =  EnableObject;
        r.m_emissiveColor = EmissiveColor.ToHex();
        return r;
    }
}