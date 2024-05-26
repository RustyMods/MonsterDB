using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace MonsterDB.DataBase;

[Serializable][CanBeNull]
public class MonsterData
{
    public bool isClone = false;
    public string OriginalMonster = "";
    public List<string> Information = new()
        { "Each category must be enabled to modify values", "Categories may have sub-categories to enable" };

    public CharacterData Character = new();
    public MonsterAIData MonsterAI = new();
    public CharacterDropData CharacterDrop = new();
    public ProcreationData Procreation = new();
    public TameData Tameable = new();
    public List<MaterialData> Materials = new();
}

[Serializable]
[CanBeNull]
public class Scale
{
    public float x;
    public float y;
    public float z;
}
[Serializable]
[CanBeNull]
public class MaterialData
{
    public bool Enabled;
    public List<string> ShaderInformation = new() { "Shader is informational only" };
    public string Shader = "";
    public string MaterialName = "";
    public string MainTexture = "";
    public ColorData MainColor = new();
    public float Hue;
    public float Saturation;
    public float Value;
    public float AlphaCutoff;
    public float Smoothness;
    public bool UseGlossMap;
    public string GlossTexture = "";
    public float Metallic;
    public float MetalGloss;
    public ColorData MetalColor = new();
    public string EmissionTexture = "";
    public ColorData EmissionColor = new();
    public float BumpStrength;
    public string BumpTexture = "";
    public bool TwoSidedNormals;
    public bool UseStyles;
    public float Style;
    public string StyleTexture = "";
    public bool AddRain;
}

[Serializable]
public class ColorData
{
    public float Red;
    public float Green;
    public float Blue;
    public float Alpha;
}

[Serializable][CanBeNull]
public class DamageModifiersData
{
    public string Blunt = "Normal";
    public string Slash = "Normal";
    public string Pierce = "Normal";
    public string Chop = "Normal";
    public string Pickaxe = "Normal";
    public string Fire = "Normal";
    public string Frost = "Normal";
    public string Lightning = "Normal";
    public string Poison = "Normal";
    public string Spirit = "Normal";
}

[Serializable][CanBeNull]
public class CharacterData
{
    public bool Enabled = false;

    public List<string> PrefabNameInformation =
        new() { "PrefabName must not be changed, it is the internal ID for the monster" };
    public string PrefabName = null!;

    public List<string> ScaleInformation = new()
    {
        "Sub-category: Scale", "Height of collider can be seen by looking at the position", "of the monster name plate"
    };
    public bool ChangeScale = false;
    public Scale Scale = new Scale();
    public float ColliderHeight;
    public Scale ColliderCenter = new();
    public string Name = null!;
    public string Group = "";
    public string Faction = "";
    public bool Boss;
    public bool DoNotHideBossHUD;
    public string BossEvent = "";
    public string DefeatSetGlobalKey = "";
    // Movement & Physics
    public List<string> MovementInformation = new() { "Sub-category: Movement" };
    public bool ChangeMovement = false;
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
    public string GroundTilt = "None";
    public float GroundTiltSpeed;
    public bool Flying;
    public float JumpStaminaUsage;
    public bool DisableWhileSleeping;
    // Effects
    public List<string> EffectsInformation = new() { "Sub-category: Effects", "Effects not found will be ignored" };
    public bool ChangeEffects = false;
    public List<EffectPrefab> HitEffects = new();
    public List<EffectPrefab> CriticalHitEffects = new();
    public List<EffectPrefab> BackStabHitEffects = new();
    public List<EffectPrefab> DeathEffects = new();
    public List<EffectPrefab> WaterEffects = new();
    public List<EffectPrefab> TarEffects = new();
    public List<EffectPrefab> SlideEffects = new();
    public List<EffectPrefab> JumpEffects = new();
    public List<EffectPrefab> FlyingContinuousEffects = new();
    public List<EffectPrefab> PickUpEffects = new();
    public List<EffectPrefab> DropEffects = new();
    public List<EffectPrefab> ConsumeItemEffects = new();
    public List<EffectPrefab> EquipEffects = new();
    public List<EffectPrefab> PerfectBlockEffect = new();
    // Health & Damage
    public List<string> HealthDamageInformation = new()
    {
        "Sub-category: Health & Damage", "Available damage modifiers:",
        "VeryWeak", "Weak", "Normal", "Resistant", "VeryResistant", "Immune", "Ignore"
    };
    public bool ChangeHealthDamage = false;
    public bool TolerateWater;
    public bool TolerateFire;
    public bool TolerateSmoke;
    public bool TolerateTar;
    public float Health;
    public DamageModifiersData DamageModifiers = new();
    public bool StaggerWhenBlocked;
    public float StaggerDamageFactor;
    // Default Items
    public List<string> ItemsInformation = new()
    {
        "Sub-category: Items", "If item not found then it will be ignored",
        "Attack Animation are unique to each creature"
    };
    public bool ChangeItems = false;
    public List<CreatureItem> DefaultItems = new();
    public List<CreatureItem> RandomWeapons = new();
    public List<CreatureItem> RandomArmors = new();
    public List<CreatureItem> RandomShields = new();
    public List<RandomSet> RandomSets = new();
    public string UnarmedWeapon = "";
    public string BeardItem = "";
    public string HairItem = "";
    public bool FemaleModel;

}

[Serializable]
[CanBeNull]
public class CreatureItem
{
    public string Name = null!;
    public string AttackAnimation = "";
    public string AttackOrigin = "";
    public bool HitTerrain;
    public bool HitFriendly;
    public float AttackRange;
    public float AttackRangeMinimum;
    public float AttackInterval;
    public float AttackMaxAngle;
    public AttackDamages AttackDamages = new();
    public int ToolTier;
    public float AttackForce;
    public bool Dodgeable;
    public bool Blockable;
    public string SpawnOnHit = "";
    public string SpawnOnHitTerrain = "";
    public string AttackStatusEffect = "";
}

public class AttackDamages
{
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
}

[Serializable][CanBeNull]
public class EffectPrefab
{
    public string Prefab = null!;
    public bool Enabled = true;
    public int Variant = -1;
    public bool Attach;
    public bool Follow;
    public bool InheritParentRotation;
    public bool InheritParentScale;
    public bool RandomRotation;
    public bool MultiplyParentVisualScale;
    public bool Scale;
}

[Serializable][CanBeNull]
public class RandomSet
{
    public string Name = null!;
    public List<CreatureItem> Items = new();
}

[Serializable][CanBeNull]
public class MonsterAIData
{
    public bool Enabled = false;
    public float ViewRange;
    public float ViewAngle;
    public float HearRange;
    public bool MistVision;
    public List<string> EffectsInformation = new() { "Sub-category: Effects", "Effects not found will be ignored" };
    public bool ChangeEffects = false;
    public List<EffectPrefab> AlertedEffects = new();
    public List<EffectPrefab> IdleSound = new();
    public List<EffectPrefab> WakeUpEffects = new();
    public float IdleSoundInterval;
    public float IdleSoundChance;
    public string PathAgentType = null!;
    public float MoveMinimumAngle;
    public bool smoothMovement;
    public bool SerpentMovement;
    public float SerpentTurnRadius;
    public float JumpInterval;
    // Random Circle
    public float RandomCircleInterval;
    // Random Movement
    public float RandomMoveInterval;
    public float RandomMoveRange;
    // Fly Behavior
    public bool RandomFly;
    public float ChanceToTakeOff;
    public float ChanceToLand;
    public float GroundDuration;
    public float AirDuration;
    public float MaxLandAltitude;
    public float TakeOffTime;
    public float FlyAltitudeMinimum;
    public float FlyAltitudeMaximum;
    public float FlyAbsoluteMinimumAltitude;
    // Other
    public bool AvoidFire;
    public bool AfraidOfFire;
    public bool AvoidWater;
    public bool Aggravatable;
    public bool PassiveAggressive;
    public string SpawnMessage = "";
    public string DeathMessage = "";
    public string AlertedMessage = "";
    public float TimeToSafe;
    // Monster AI
    public float AlertRange;
    public bool FleeIfHurtWhenTargetCannotBeReached;
    public bool FleeIfNotAlerted;
    public float FleeIfLowHealth;
    public bool CirculateWhileCharging;
    public bool CirculateWhileChargingFlying;
    public bool EnableHuntPlayer;
    public bool AttackPlayerObjects;
    public int PrivateAreaTriggerThreshold;
    public float InterceptTimeMaximum;
    public float InterceptTimeMinimum;
    public float MaximumChaseDistance;
    public float MinimumAttackInterval;
    // Circle Target
    public float CircleTargetInterval;
    public float CircleTargetDuration;
    public float CircleTargetDistance;
    // Sleep
    public bool Sleeping;
    public float WakeUpRange;
    public bool NoiseWakeUp;
    public float MaximumNoiseWakeUpRange;
    public float WakeUpDelayMinimum;
    public float WakeUpDelayMaximum;
    // Other
    public bool AvoidLand;
    // Consume Items
    public List<string> ConsumeInformation = new() { "Sub-category: Consume Items" };
    public bool ChangeConsume = false;
    public List<string> ConsumeItems = new();
    public float ConsumeRange;
    public float ConsumeSearchRange;
    public float ConsumeSearchInterval;
}

[Serializable][CanBeNull]
public class CharacterDropData
{
    public bool Enabled = false;
    public List<DropData> Drops = new();
}

[Serializable][CanBeNull]
public class DropData
{
    public string Prefab = null!;
    public int AmountMinimum;
    public int AmountMaximum;
    public float Chance;
    public bool OnePerPlayer;
    public bool LevelMultiplier;
    public bool DoNotScale;
}

[Serializable][CanBeNull]
public class ProcreationData
{
    public bool Enabled;
    public float UpdateInterval;
    public float TotalCheckRange;
    public int MaxCreatures;
    public float PartnerCheckRange;
    public float PregnancyChance;
    public float PregnancyDuration;
    public int RequiredLovePoints;
    public List<string> OffSpringInformation = new() { "Sub-category: Offspring" };
    public bool ChangeOffSpring = false;
    public string OffSpring = null!;
    public int MinimumOffspringLevel;
    public float SpawnOffset;
    public List<string> EffectsInformation = new() { "Sub-category: Effects", "Effects not found will be ignored" };
    public bool ChangeEffects = false;
    public List<EffectPrefab> BirthEffects = new();
    public List<EffectPrefab> LoveEffects = new();
}

[Serializable][CanBeNull]
public class TameData
{
    public bool Enabled;
    public bool MakeTameable = false;
    public float FedDuration;
    public float TamingTime;
    public bool StartsTamed;
    public bool Commandable;
    public float UnSummonDistance;
    public float UnSummonOnOwnerLogoutSeconds;
    public string LevelUpOwnerSkill = "None";
    public float LevelUpFactor;
    public string SaddleItem = "";
    public bool DropSaddleOnDeath;
    public float DropItemVelocity;
    public List<string> EffectsInformation = new() { "Sub-category: Effects" };
    public bool ChangeEffects = false;
    public List<EffectPrefab> TamedEffects = new();
    public List<EffectPrefab> SootheEffects = new();
    public List<EffectPrefab> PetEffects = new();
    public List<EffectPrefab> UnSummonEffects = new();
    public List<string> RandomStartingName = new();
}