# Monster Database

Enables deep configurations to monsters loaded into game and cloning.

## Console Commands
Can only be used locally. Create your monster tweaks / clones locally, then transfer completed YML file to your server.
```
save [prefabName]
    Writes to disk monster data
    
save attack [prefabName]
    Writes to disk monster attack data
    
save textures
    save to file all the names of textures in available resources
    
clone [prefabName] [customName]
    clones creature and writes to disk clone data
    
update [prefabName]
    updates creature with latest data
    
search texture [filter]
    prints a list of textures where filter is contained in name
    
search attack [filter]
    prints a list of attacks where filter is contained in name
    
reset [prefabName]
    Tries to resets monster to default settings
```
### Spawn Data
MonsterDB creates an example file for spawn data.
You can create as many files as you wish within the folder

### Server Sync
If playing multiplayer, server sends the Monster Files, Spawn Files and Texture Files to clients

If client is connected to server, the server files are used instead of the local files

There is about a 100MB size limit so you can also share the textures through mod packs.
I would recommend to share texture files this way to avoid 100MB limit

### File Watch
If files are changed, it will automatically update during run-time, as well as send new data to clients

### Human Player Animation Keys
MonsterDB creates a base human named: Human01

You can use player items for this monster
```
- swing_axe 0 1 2
- axe_secondary
- swing_pickaxe
- swing_longsword 0 1 2
- sword_secondary
- mace_secondary
- swing_sledge
- swing_hammer
- swing_hoe
- bow_fire
- bow_aim
- atgeir_attack 0 1 2
- atgeir_secondary
- battleaxe_attack 0 1 2
- battleaxe_secondary
- spear_throw
- spear_poke
- unarmed_attack 0 1
- unarmed_kick
- throw_bomb
- knife_stab 0 1 2
- knife_secondary
- crossbow_fire
- dual_knives 0 1 2
- dual_knives_secondary
- staff_fireball 0 1
- staff_rapidfire
- staff_shield
- staff_summon
- greatsword 0 1 2
- greatsword_secondary
```
### Monster Data YML
Each section has an enable keyword to optimize to optimize for performance
If you make changes then disable section, it will not revert the changes
```yaml
## ENABLED must be set to 'true' to make changes
ENABLED: false 
## Insert the prefab name of the original monster you are cloning from
OriginalMonster: ''
## isClone is true, then plugin will search for original monster, if it exists, clone it with data below
isClone: false
## PrefabName is the internal ID, only change if you are creating a clone
PrefabName: GoblinKing
## Change the scale of the monster, collider inputs allow for fine tuning
CHANGE_SCALE: false
Scale:
  x: 1
  y: 1
  z: 1
ColliderHeight: 8
ColliderCenter:
  x: 0
  y: 4
  z: 0
## Humanoid or Character component values
DisplayName: $enemy_goblinking
Group: ''
Faction: Boss
Boss: true
DoNotHideBossHUD: false
BossEvent: boss_goblinking
DefeatSetGlobalKey: defeated_goblinking
## Modify the movement behavior
CHANGE_MOVEMENT: false
CrouchSpeed: 2
WalkSpeed: 5
Speed: 2
TurnSpeed: 50
RunSpeed: 4
RunTurnSpeed: 50
FlySlowSpeed: 5
FlyFastSpeed: 12
FlyTurnSpeed: 12
Acceleration: 0.2
JumpForce: 10
JumpForceForward: 0
JumpForceTiredFactor: 0.7
AirControl: 0.1
CanSwim: true
SwimDepth: 7.79
SwimSpeed: 1.5
SwimTurnSpeed: 100
SwimAcceleration: 0.05
GroundTilt: FullRaycast
GroundTiltSpeed: 50
Flying: false
JumpStaminaUsage: 10
DisableWhileSleeping: false
## Modify the visual and sound effects
CHANGE_CHARACTER_EFFECTS: false
HitEffects:
- Prefab: fx_goblinking_hit
  Enabled: true
  Variant: -1
  Attach: false
  Follow: false
  InheritParentRotation: false
  InheritParentScale: false
  RandomRotation: false
  MultiplyParentVisualScale: false
  Scale: false
CriticalHitEffects:
- Prefab: fx_crit
  Enabled: true
  Variant: -1
  Attach: false
  Follow: false
  InheritParentRotation: false
  InheritParentScale: false
  RandomRotation: false
  MultiplyParentVisualScale: false
  Scale: false
BackStabHitEffects:
- Prefab: fx_backstab
  Enabled: true
  Variant: -1
  Attach: false
  Follow: false
  InheritParentRotation: false
  InheritParentScale: false
  RandomRotation: false
  MultiplyParentVisualScale: false
  Scale: false
DeathEffects:
- Prefab: fx_goblinking_death
  Enabled: true
  Variant: -1
  Attach: false
  Follow: false
  InheritParentRotation: false
  InheritParentScale: false
  RandomRotation: false
  MultiplyParentVisualScale: false
  Scale: false
- Prefab: GoblinKing_ragdoll
  Enabled: true
  Variant: -1
  Attach: false
  Follow: false
  InheritParentRotation: false
  InheritParentScale: false
  RandomRotation: false
  MultiplyParentVisualScale: false
  Scale: false
WaterEffects:
- Prefab: vfx_water_surface
  Enabled: true
  Variant: -1
  Attach: true
  Follow: false
  InheritParentRotation: false
  InheritParentScale: false
  RandomRotation: false
  MultiplyParentVisualScale: false
  Scale: false
## Brackets indicate a list of entries, like above
TarEffects: []
SlideEffects: []
JumpEffects: []
FlyingContinuousEffects: []
PickUpEffects: []
DropEffects: []
ConsumeItemEffects: []
EquipEffects: []
PerfectBlockEffect: []
## Modify health and damages
CHANGE_HEALTH_DAMAGES: false
TolerateWater: true
TolerateFire: false
TolerateSmoke: false
TolerateTar: false
Health: 10000
## Immune | Ignore | VeryResistant | Resistant | Normal | Weak | VeryWeak
DamageModifiers:
  Blunt: Normal
  Slash: Normal
  Pierce: VeryResistant
  Chop: Ignore
  Pickaxe: Ignore
  Fire: Resistant
  Frost: Normal
  Lightning: Normal
  Poison: Immune
  Spirit: Normal
StaggerWhenBlocked: false
StaggerDamageFactor: 0
## Modify Attacks, they are defined by ItemDrop component
## You can set enable_item to false to easily remove entries without deleting data
## You can either change the Name of item to try to clone attack from another monster
## Or change the values
CHANGE_ATTACKS: false
DefaultItems:
- ENABLE_ITEM: true
  Name: GoblinKing_Beam
  ## Attack Animation cannot be empty 
  ## It is related to the unique triggers each monster has
  AttackAnimation: beam
  ## Most attacks do not require an attack origin, can be left empty
  ## It will cause errors if you input an origin that does not exist
  ## This is related to the armature of the creature
  AttackOrigin: BeamRoot
  CHANGE_ITEM_DATA: false
  HitTerrain: true
  HitFriendly: false
  AttackRange: 40
  AttackRangeMinimum: 10
  AttackInterval: 15
  AttackMaxAngle: 20
  AttackDamages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 50
    Pickaxe: 50
    Fire: 40
    Frost: 0
    Lightning: 20
    Poison: 0
    Spirit: 0
  ToolTier: 3
  ## Attack force relates to the amount of push back you receive
  AttackForce: 10
  Dodgeable: true
  Blockable: true
  SpawnOnHit: ''
  SpawnOnHitTerrain: ''
  AttackStatusEffect: ''
  HitThroughWalls: false
  CHANGE_ITEM_EFFECTS: false
  ## Follow the same syntax as the effect lists above
  HitEffects: []
  HitTerrainEffects: []
  StartEffects: []
  TriggerEffects: []
  TrailStartEffects: []
  BurstEffects: []
  CHANGE_PROJECTILE: false
  ## You can swap out projectiles from other creatures
  Projectile_PrefabName: projectile_beam
  ## Make sure you give it a unique ID
  ## You can keep the same ID for different attacks if they are the same
  Projectile_Clone_ID: ''
  CHANGE_PROJECTILE_DATA: false
  ## If all your cloned projectiles share the same ID, then if data is true,
  ## Then it will only record the last changes
  Projectile_Damages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 0
    Pickaxe: 0
    Fire: 0
    Frost: 0
    Lightning: 0
    Poison: 0
    Spirit: 0
  Projectile_AOE: 0
  Projectile_Dodgeable: true
  Projectile_Blockable: true
  Projectile_AttackForce: 40
  ## Add the Status Effect Name ID
  Projectile_StatusEffect: ''
  Projectile_CanHitWater: true
  ## Add the PrefabName of the object you want to spawn on hit
  Projectile_SpawnOnHit: ''
  ## Chance is a value between 0 and 1
  Projectile_SpawnChance: 1
  ## Random Spawn on hit is a list of PrefabNames
  Projectile_RandomSpawnOnHit: []
  Projectile_StaticHitOnly: false
  Projectile_GroundHitOnly: false
  PROJECTILE_CHANGE_EFFECTS: false
  Projectile_HitEffects:
  - Prefab: fx_goblinking_beam_hit
    Enabled: true
    Variant: -1
    Attach: false
    Follow: false
    InheritParentRotation: false
    InheritParentScale: false
    RandomRotation: false
    MultiplyParentVisualScale: false
    Scale: false
  Projectile_HitWaterEffects: []
  Projectile_SpawnOnHitEffects: []
- ENABLE_ITEM: true
  Name: GoblinKing_Meteors
  AttackAnimation: cast1
  AttackOrigin: ''
  CHANGE_ITEM_DATA: false
  HitTerrain: true
  HitFriendly: false
  AttackRange: 30
  AttackRangeMinimum: 0
  AttackInterval: 25
  AttackMaxAngle: 20
  AttackDamages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 0
    Pickaxe: 0
    Fire: 0
    Frost: 0
    Lightning: 0
    Poison: 0
    Spirit: 0
  ToolTier: 0
  AttackForce: 0
  Dodgeable: false
  Blockable: false
  SpawnOnHit: ''
  SpawnOnHitTerrain: ''
  AttackStatusEffect: ''
  HitThroughWalls: false
  CHANGE_ITEM_EFFECTS: false
  HitEffects: []
  HitTerrainEffects: []
  StartEffects: []
  TriggerEffects: []
  TrailStartEffects: []
  BurstEffects: []
  CHANGE_PROJECTILE: false
  Projectile_PrefabName: ''
  Projectile_Clone_ID: ''
  CHANGE_PROJECTILE_DATA: false
  Projectile_Damages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 0
    Pickaxe: 0
    Fire: 0
    Frost: 0
    Lightning: 0
    Poison: 0
    Spirit: 0
  Projectile_AOE: 0
  Projectile_Dodgeable: false
  Projectile_Blockable: false
  Projectile_AttackForce: 0
  Projectile_StatusEffect: ''
  Projectile_CanHitWater: false
  Projectile_SpawnOnHit: ''
  Projectile_SpawnChance: 0
  Projectile_RandomSpawnOnHit: []
  Projectile_StaticHitOnly: false
  Projectile_GroundHitOnly: false
  PROJECTILE_CHANGE_EFFECTS: false
  Projectile_HitEffects: []
  Projectile_HitWaterEffects: []
  Projectile_SpawnOnHitEffects: []
- ENABLE_ITEM: true
  Name: GoblinKing_Nova
  AttackAnimation: nova
  AttackOrigin: ''
  CHANGE_ITEM_DATA: false
  HitTerrain: true
  HitFriendly: false
  AttackRange: 10
  AttackRangeMinimum: 0
  AttackInterval: 20
  AttackMaxAngle: 90
  AttackDamages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 100
    Pickaxe: 100
    Fire: 65
    Frost: 0
    Lightning: 65
    Poison: 0
    Spirit: 0
  ToolTier: 2
  AttackForce: 100
  Dodgeable: true
  Blockable: true
  SpawnOnHit: ''
  SpawnOnHitTerrain: ''
  AttackStatusEffect: ''
  HitThroughWalls: true
  CHANGE_ITEM_EFFECTS: false
  HitEffects: []
  HitTerrainEffects: []
  StartEffects: []
  TriggerEffects: []
  TrailStartEffects: []
  BurstEffects: []
  CHANGE_PROJECTILE: false
  Projectile_PrefabName: ''
  Projectile_Clone_ID: ''
  CHANGE_PROJECTILE_DATA: false
  Projectile_Damages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 0
    Pickaxe: 0
    Fire: 0
    Frost: 0
    Lightning: 0
    Poison: 0
    Spirit: 0
  Projectile_AOE: 0
  Projectile_Dodgeable: false
  Projectile_Blockable: false
  Projectile_AttackForce: 0
  Projectile_StatusEffect: ''
  Projectile_CanHitWater: false
  Projectile_SpawnOnHit: ''
  Projectile_SpawnChance: 0
  Projectile_RandomSpawnOnHit: []
  Projectile_StaticHitOnly: false
  Projectile_GroundHitOnly: false
  PROJECTILE_CHANGE_EFFECTS: false
  Projectile_HitEffects: []
  Projectile_HitWaterEffects: []
  Projectile_SpawnOnHitEffects: []
- ENABLE_ITEM: true
  Name: GoblinKing_Taunt
  AttackAnimation: taunt
  AttackOrigin: ''
  CHANGE_ITEM_DATA: false
  HitTerrain: true
  HitFriendly: false
  AttackRange: 50
  AttackRangeMinimum: 10
  AttackInterval: 60
  AttackMaxAngle: 5
  AttackDamages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 0
    Pickaxe: 0
    Fire: 0
    Frost: 0
    Lightning: 0
    Poison: 0
    Spirit: 0
  ToolTier: 0
  AttackForce: 30
  Dodgeable: true
  Blockable: true
  SpawnOnHit: ''
  SpawnOnHitTerrain: ''
  AttackStatusEffect: ''
  HitThroughWalls: false
  CHANGE_ITEM_EFFECTS: false
  HitEffects: []
  HitTerrainEffects: []
  StartEffects: []
  TriggerEffects: []
  TrailStartEffects: []
  BurstEffects: []
  CHANGE_PROJECTILE: false
  Projectile_PrefabName: ''
  Projectile_Clone_ID: ''
  CHANGE_PROJECTILE_DATA: false
  Projectile_Damages:
    Damage: 0
    Blunt: 0
    Slash: 0
    Pierce: 0
    Chop: 0
    Pickaxe: 0
    Fire: 0
    Frost: 0
    Lightning: 0
    Poison: 0
    Spirit: 0
  Projectile_AOE: 0
  Projectile_Dodgeable: false
  Projectile_Blockable: false
  Projectile_AttackForce: 0
  Projectile_StatusEffect: ''
  Projectile_CanHitWater: false
  Projectile_SpawnOnHit: ''
  Projectile_SpawnChance: 0
  Projectile_RandomSpawnOnHit: []
  Projectile_StaticHitOnly: false
  Projectile_GroundHitOnly: false
  PROJECTILE_CHANGE_EFFECTS: false
  Projectile_HitEffects: []
  Projectile_HitWaterEffects: []
  Projectile_SpawnOnHitEffects: []
RandomWeapons: []
RandomArmors: []
RandomShields: []
RandomSets:
  ## Name you random set
  - Name: Leather
    Items:
    ## Insert Attack Items like above
    - ENABLE_ITEM: true
      Name: ArmorLeatherChest
      AttackAnimation: ''
      AttackOrigin: ''
      CHANGE_ITEM_DATA: false
      HitTerrain: true
      HitFriendly: false
      AttackRange: 2
      AttackRangeMinimum: 0
      AttackInterval: 2
      AttackMaxAngle: 5
      AttackDamages:
        Damage: 0
        Blunt: 0
        Slash: 0
        Pierce: 0
        Chop: 0
        Pickaxe: 0
        Fire: 0
        Frost: 0
        Lightning: 0
        Poison: 0
        Spirit: 0
      ToolTier: 0
      AttackForce: 10
      Dodgeable: false
      Blockable: false
      SpawnOnHit: ''
      SpawnOnHitTerrain: ''
      AttackStatusEffect: ''
      HitThroughWalls: false
      CHANGE_ITEM_EFFECTS: false
      HitEffects: []
      HitTerrainEffects: []
      StartEffects: []
      TriggerEffects: []
      TrailStartEffects: []
      BurstEffects: []
      CHANGE_PROJECTILE: false
      Projectile_PrefabName: ''
      Projectile_Clone_ID: ''
      CHANGE_PROJECTILE_DATA: false
      Projectile_Damages:
        Damage: 0
        Blunt: 0
        Slash: 0
        Pierce: 0
        Chop: 0
        Pickaxe: 0
        Fire: 0
        Frost: 0
        Lightning: 0
        Poison: 0
        Spirit: 0
      Projectile_AOE: 0
      Projectile_Dodgeable: false
      Projectile_Blockable: false
      Projectile_AttackForce: 0
      Projectile_StatusEffect: ''
      Projectile_CanHitWater: false
      Projectile_SpawnOnHit: ''
      Projectile_SpawnChance: 0
      Projectile_RandomSpawnOnHit: []
      Projectile_StaticHitOnly: false
      Projectile_GroundHitOnly: false
      PROJECTILE_CHANGE_EFFECTS: false
      Projectile_HitEffects: []
      Projectile_HitWaterEffects: []
      Projectile_SpawnOnHitEffects: []
UnarmedWeapon: ''
## Beard0 | Beard1 | Beard2 ...
BeardItem: ''
## Hair0 | Hair1 | Hair2 ...
HairItem: ''
## For Player models, you can set female or male
FemaleModel: false
CHANGE_AI: false
ViewRange: 30
ViewAngle: 90
HearRange: 9999
MistVision: false
CHANGE_MONSTER_EFFECTS: false
AlertedEffects: []
IdleSound: []
WakeUpEffects: []
IdleSoundInterval: 5
IdleSoundChance: 0.25
PathAgentType: HugeSize
MoveMinimumAngle: 90
smoothMovement: true
SerpentMovement: false
SerpentTurnRadius: 20
JumpInterval: 0
RandomCircleInterval: 2
RandomMoveInterval: 15
RandomMoveRange: 10
RandomFly: false
ChanceToTakeOff: 1
ChanceToLand: 1
GroundDuration: 10
AirDuration: 10
MaxLandAltitude: 5
TakeOffTime: 5
FlyAltitudeMinimum: 3
FlyAltitudeMaximum: 10
FlyAbsoluteMinimumAltitude: 32
AvoidFire: false
AfraidOfFire: false
AvoidWater: true
Aggravatable: false
PassiveAggressive: false
## Set or remove messages
SpawnMessage: $enemy_boss_goblinking_spawnmessage
DeathMessage: $enemy_boss_goblinking_deathmessage
AlertedMessage: ''
TimeToSafe: 0
AlertRange: 20
FleeIfHurtWhenTargetCannotBeReached: false
FleeIfNotAlerted: false
FleeIfLowHealth: 0
CirculateWhileCharging: false
CirculateWhileChargingFlying: false
EnableHuntPlayer: true
AttackPlayerObjects: true
PrivateAreaTriggerThreshold: 4
InterceptTimeMaximum: 0
InterceptTimeMinimum: 0
MaximumChaseDistance: 0
MinimumAttackInterval: 0
CircleTargetInterval: 0
CircleTargetDuration: 10
CircleTargetDistance: 10
Sleeping: true
WakeUpRange: 5
NoiseWakeUp: false
MaximumNoiseWakeUpRange: 50
WakeUpDelayMinimum: 0
WakeUpDelayMaximum: 0
AvoidLand: false
CHANGE_MONSTER_CONSUME: false
ConsumeItems: []
ConsumeRange: 2
ConsumeSearchRange: 5
ConsumeSearchInterval: 10
## Modify Monster Drops
CHANGE_DROP_TABLE: false
Drops:
- Prefab: TrophyGoblinKing
  AmountMinimum: 1
  AmountMaximum: 1
  Chance: 1
  OnePerPlayer: false
  LevelMultiplier: false
  DoNotScale: true
- Prefab: YagluthDrop
  AmountMinimum: 3
  AmountMaximum: 3
  Chance: 1
  OnePerPlayer: false
  LevelMultiplier: false
  DoNotScale: true
CHANGE_PROCREATION: false
UpdateInterval: 0
TotalCheckRange: 0
MaxCreatures: 0
PartnerCheckRange: 0
PregnancyChance: 0
PregnancyDuration: 0
RequiredLovePoints: 0
ChangeOffSpring: false
## Offspring prefab name
OffSpring: 
MinimumOffspringLevel: 0
SpawnOffset: 0
ChangeEffects: false
BirthEffects: []
LoveEffects: []
CHANGE_TAMEABLE: false
## Make Tameable set to true if Monster is not tameable by default
MAKE_TAMEABLE: false
FedDuration: 0
TamingTime: 0
StartsTamed: false
Commandable: false
UnSummonDistance: 0
UnSummonOnOwnerLogoutSeconds: 0
LevelUpOwnerSkill: None
LevelUpFactor: 0
## Only usable if monster is rideable by default
SaddleItem: ''
DropSaddleOnDeath: false
DropItemVelocity: 0
CHANGE_TAME_EFFECTS: false
TamedEffects: []
SootheEffects: []
PetEffects: []
UnSummonEffects: []
RandomStartingName: []
## Modify material of monster
Materials:
- Enabled: false
  Shader: Custom/Creature
  MaterialName: GoblinKing_mat
  ## You can register custom textures and set it here, or try to input textures from the game
  MainTexture: GoblinKing_d
  MainColor:
    Red: 1
    Green: 1
    Blue: 1
    Alpha: 1
  Hue: 0
  Saturation: 0
  Value: 0
  AlphaCutoff: 0.112
  Smoothness: 0.07
  UseGlossMap: false
  GlossTexture: GoblinKing_m
  Metallic: 1
  MetalGloss: 0.746
  MetalColor:
    Red: 1.548
    Green: 1.548
    Blue: 1.548
    Alpha: 1
  EmissionTexture: ''
  EmissionColor:
    Red: 0
    Green: 0
    Blue: 0
    Alpha: 1
  BumpStrength: 1
  BumpTexture: GoblinKing_n
  TwoSidedNormals: true
  UseStyles: false
  Style: 0
  StyleTexture: ''
  AddRain: false
- Enabled: false
  Shader: Custom/Creature
  MaterialName: GoblinKing_mat
  MainTexture: GoblinKing_d
  MainColor:
    Red: 1
    Green: 1
    Blue: 1
    Alpha: 1
  Hue: 0
  Saturation: 0
  Value: 0
  AlphaCutoff: 0.112
  Smoothness: 0.07
  UseGlossMap: false
  GlossTexture: GoblinKing_m
  Metallic: 1
  MetalGloss: 0.746
  MetalColor:
    Red: 1.548
    Green: 1.548
    Blue: 1.548
    Alpha: 1
  EmissionTexture: ''
  EmissionColor:
    Red: 0
    Green: 0
    Blue: 0
    Alpha: 1
  BumpStrength: 1
  BumpTexture: GoblinKing_n
  TwoSidedNormals: true
  UseStyles: false
  Style: 0
  StyleTexture: ''
  AddRain: false
```
### Directories
- Data
- Monsters
- SpawnData
- Textures
Data folder is for information files

You can use the command save attack [prefabName] to get file structure for monster attacks
You can use the command search attack [filter] to look for available attack items to clone

### Spawn File YML
```yaml
## m_name is unique ID for spawn file
m_name: ''
m_enabled: true
m_devDisabled: false
m_prefab: 
## Meadows | BlackForest | Swamp | Mountains | Plains | Mistlands | DeepNorth | AshLands | Ocean | All
m_biome: 
## Edge | Median | Everything
m_biomeArea: Everything
m_maxSpawned: 1
m_spawnInterval: 4
m_spawnChance: 100
m_spawnDistance: 10
m_spawnRadiusMin: 0
m_spawnRadiusMax: 0
m_requiredGlobalKey: ''
## List of names of environments
m_requiredEnvironments: []
m_groupSizeMin: 1
m_groupSizeMax: 1
m_groupRadius: 3
m_spawnAtNight: true
m_spawnAtDay: true
m_minAltitude: -1000
m_maxAltitude: 1000
m_minTilt: 0
m_maxTilt: 35
m_inForest: true
m_outsideForest: true
m_inLava: false
m_outsideLava: true
m_canSpawnCloseToPlayer: false
m_insidePlayerBase: false
m_minOceanDepth: 0
m_maxOceanDepth: 0
m_huntPlayer: false
m_groundOffset: 0.5
m_groundOffsetRandom: 0
m_maxLevel: 1
m_minLevel: 1
m_levelUpMinCenterDistance: 0
m_overrideLevelupChance: -1
m_foldout: false
```