# ItemData

ItemData defines how an item behaves in combat and gameplay, including damage values, animations, attack logic, AI behavior, and effects.
Valheim uses ItemData for both player items and creature attacks, which means changes can have wide-reaching consequences if shared prefabs are modified.

⚠️ Important:
Be careful when modifying items directly. Many items are:

- Referenced by player equipment
- Shared across multiple creatures

To avoid unintended side effects, MonsterDB automatically clones ItemData when cloning creatures, 
ensuring that creature attacks remain independent from the original source items.

### Typical Use Cases

Common reasons to modify or define ItemData include:

- Adjusting damage values for a specific creature attack
- Adding unique mechanics (e.g. spawning prefabs on hit)
- Applying status effects
- Defining attack-specific AI behavior (range, interval, conditions)

Advanced users can leverage these fields to precisely control how and when creatures use specific attacks.

```yml
prefab: string              # Prefab ID
name: string                # Display name
description: string
maxStackSize: int
maxQuality: int
scaleByQuality: float
weight: float
scaleWeightByQuality: float
value: int                  # Trader value
teleportable: bool
icons: string[]             # List of icon names
toolTier: int               # Can damage rocks/trees of same or lower tool tier
damages: Damages            # See Damages section below
attackForce: float          # Pushback force
backstabBonus: float        # Damage multiplier on backstab
dodgeable: true
blockable: true
tamedOnly: false

# Status effects
attackStatusEffect: string        # StatusEffect ID
attackStatusEffectChance: float   # (0.0 – 1.0)

# Spawning behavior
spawnOnHit: string                # Prefab ID
spawnOnHitTerrain: string         # Prefab ID

# Attack definition
attack: Attack                    # See Attack section below

# AI Attack Behavior
# Controls how the AI evaluates and uses this attack:
aiAttackRange: float
aiAttackRangeMin: float
aiAttackInterval: float
aiAttackMaxAngle: float
aiInvertAngleCheck: boolean
aiWhenFlying: boolean
aiWhenFlyingAltitudeMin: float
aiWhenFlyingAltitudeMax: float
aiWhenWalking: boolean
aiWhenSwiming: boolean
aiPrioritized: boolean
aiInDungeonOnly: boolean
aiInMistOnly: boolean
aiMaxHealthPercentage: float      # (0.0 - 1.0)
aiMinHealthPercentage: float
aiTargetType: TargetType          # See below

# Various effects
hitEffect: EffectList
hitTerrainEffect: EffectList
blockEffect: EffectList
startEffect: EffectList
holdStartEffect: EffectList
triggerEffect: EffectList
trailStartEffect: EffectList
```

### TargetType 
```yml
Enemy
FriendHurt
Friend
```

### Damages
```yml
damage:     float     # true damage, independent of damage modifiers
blunt:      float
slash:      float
pierce:     float
chop:       float
pickaxe:    float
fire:       float
frost:      float
lightning:  float
poison:     float
spirit:     float
```

### Attack

Defines the mechanics, animation, and logic of the attack itself.
```yml
attackType: AttackType              # See below

# Animation
attackAnimation: string             # Animation ID (trigger)
chargeAnimationBool: string         # Animation ID (boolean)
attackRandomAnimations: int
attackChainLevels: int
loopingAttack: boolean

# Projectile
consumeItem: boolean

# Hit:
hitTerrain: boolean
attackHealthReturnHit: float        # Health healed on hit
attackKillsSelf: boolean
speedFactor: float
speedFactorRotation: float
attackStartNoise: float
attackHitNoise: float
damageMultiplier: float
damageMultiplierPerMissingHP: float
damageMultiplierByTotalHealthMissing: float
staminaReturnPerMissingHP: float
forceMultiplier: float
staggerMultiplier: float
recoilPushback: float
selfDamage: int
attackOriginJoint: string           # specify which bone the attack starts from

# Raycast
attackRange: float
attackHeight: float
attackHeightChar1: float
attackHeightChar2: float
attackOffset: float

# Special
spawnOnTrigger: string              # prefab ID
toggleFlying: boolean               # On trigger, toggle flying, tupically used by Seekers
attach: boolean                     # On hit, attach to target, typically used by Ticks
cantUseInDungeon: boolean

# Reload
requiresReload: boolean
reloadAnimation: string             # animation ID
reloadTime: float
drawAnimationState: string

attackAngle: float
attackRayWidth: float               # Used in conjunction with attackHeightChar1
attackRayWidthCharExtra: float      # Used in conjunction with attackHeightChar2
maxYAngle: float
lowerDamagePerHit: boolean

hitPointtype: HitPointType          # See below
hitThroughWalls: boolean
multiHit: boolean
pickaxeSpecial: boolean          
lastChainDamageMultiplier: float

resetChainIfHit: DestructibleType   # See below
spawnOnHit: string                  # prefab ID
spawnOnHitChance: float             # 0.0 - 1.0
attackProjectile: string            # prefab ID
projectileRef: Projectile           # see below
projectileVel: float
projectileVelMin: float
randomVelocity: boolean
projectileAccuracy: float
projectileAccuracyMin: float
circularProjectileLaunch: boolean
distributeProjectilesAroundCircle: boolean
useCharacterFacing: boolean
useCharacterFacingYAim: boolean
launchAngle: float
projectiles: int
projectileBursts: int
burstInterval: float
destroyPreviousProjectile: boolean
perBurstResourceUsage: boolean

# Effects
hitEffect: EffectList
hitTerrainEffect: EffectList
startEffect: EffectList
triggerEffect: EffectList
trailStartEffect: EffectList
burstEffect: EffectList
```

### AttackType

AttackType defines attack behaviour on triggered

```yml
Horizontal            # Melee
Vertical              # Melee
Projectile            # Ranged
None                  # Non Attack
Area                  # AOE
TriggerProjectile
```

### HitPointType

HitPointType defines which collider gets chosen on attack triggered

```yml
Closest
Average
First
```

### DestructibleType
```yml
None
Default
Tree
Character
Everything
```

### Projectile
```yml
prefab: string    # prefab ID
# enum ProjectileType
# None, Arrow, Magic, Bomb, Physical, Bolt, Missile,
# Spear, Tar, Fire, Lava, Posion, Smoke, Frost, Catapult
# AOE, Harpoon, Nature, Lightning, Summon
type: Arrow
damage: Damages       # see above
aoe: float            # If over 0, will trigger AOE damage, by this range
dodgeable: boolean
blockable: boolean
attackForce: float    # Pushback force
backstabBonus: float
statusEffect: string  # StatusEffect ID
healthReturn: float
ttl: float
hitNoise: float

# Effects
hitEffects: EffectList
hitWaterEffects: EffectList

# Spawn On Hit
spawnOnTtl: false
spawnOnHit: string        # prefab ID
spawnOnHitChance: float   # (0.0 - 1.0) if over 1.0 will always spawn
spawnCount: int
randomSpawnOnHit: string list
randomSpawnOnHitCount: 1
randomSpawnSkipLava: false
staticHitOnly: false
groundHitOnly: false
spawnOffset:
  x: 0
  y: 0
  z: 0
spawnRandomRotation: boolean
spawnFacingRotation: boolean
spawnOnHitEffects: EffectList
spawnProjectileNewVelocity: false
spawnProjectileMinVel: float
spawnProjectileMaxVel: float
spawnProjectileRandomDir: float
spawnProjectileHemisphereDir: boolean
projectilesInheritHitData: boolean
onlySpawnedProjectilesDealDamage: boolean
divideDamageBetweenProjectiles: boolean
```