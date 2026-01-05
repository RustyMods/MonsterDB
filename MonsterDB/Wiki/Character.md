# Character

`Character` is the base class that defines all creatures in Valheim.
Both `Humanoid` and `Player` inherit from Character, meaning they include all of the fields below in addition to their own extensions.

```yml
Character:
    # NOTE: Player kill statistics use `name` as the key when recording counts
    name: string                # Display name
    group: string               # Same group overrides faction hostility
    
    faction: Faction (enum)     # See below, Creatures of the same faction are friendly
    
    # Boss configuration
    boss: boolean
    dontHideBossHud: boolean
    bossEvent: string           # Environment override while boss is active
    defeatSetGlobalKey: string  # World modifier set on defeat
    
    # AI & targeting
    aiSkipTarget: boolean       # If true, ignored as a target by other creatures
    
    # Movement
    crouchSpeed: float          # Speed while crouched
    walkSpeed: float            # Speed while walking
    speed: float
    turnSpeed: float
    runSpeed: float
    runTurnSpeed: float
    acceleration: float
    
    # Jumping & air control
    jumpForce: float
    jumpForceForward: float
    jumpForceTiredFactor: float # Jump force multiplier
    airControl: float
    jumpStaminaUsage: float
    
    # Flying
    flying: float
    flySlowSpeed: float
    flyFastSpeed: float
    flyTurnSpeed: float
    
    # Swimming
    canSwim: float
    swimDepth: float
    swimSpeed: float
    swimTurnSpeed: float
    swimAcceleration: float
    
    # Ground interaction
    groundTilt: GroundTilt (enum)  # See below
    groundTiltSpeed: float
    
    # Physics & scaling
    disableWhileSleeping: boolean  # Disable gravity & kinematics while sleeping
    useAltStatusEffectScaling: boolean
    # If true, targeting collider radius ignores local scale
    
    # Environmental tolerances
    tolerateWater: boolean
    tolerateFire: boolean
    tolerateSmoke: boolean
    tolerateTar: boolean
    
    # Effects
    hitEffects: EffectList
    critHitEffects: EffectList
    backstabHitEffects: EffectList
    deathEffects: EffectList
    waterEffects: EffectList
    tarEffects: EffectList
    slideEffects: EffectList
    jumpEffects: EffectList
    flyingContinuousEffect: EffectList
    pheromoneLoveEffect: EffectList
    
    # Health & regeneration
    health: float
    regenAllHPTime: float       # Time (seconds) to fully regenerate health
    
    damageModifiers: DamageModifiers    # See below
      
    # Combat reactions
    staggerWhenBlocked: boolean
    staggerDamageFactor: float  # Damage multiplier while staggered
    
    # Player interaction
    enemyAdrenalineMultiplier: float
    # Multiplies adrenaline gained by players when hitting this creature
```

### DamageModifiers

```yml
# DamageType:             # DamageModifier:
blunt:                      Normal
slash:                      Resistant
pierce:                     Weak
chop:                       Immune
pickaxe:                    Ignore
fire:                       VeryResistant
frost:                      VeryWeak
lightning:                  SlightlyResistant
poison:                     SlightlyWeak
spirit:
```

### Faction (enum)
```
Players
AnimalsVeg
Undead
Demon
MountainMonsters
SeaMonsters,
PlainsMonsters
Boss
MistlandsMonsters
Dverger
PlayerSpawned
TrainingDummy
```

### GroundTilt (enum)
Movement behavior on tilted ground
```
None
Pitch
Full
PitchRaycast
FullRaycast
Flying
```

## Humanoid

`Humanoid` inherits from `Character`, so all `Character fields` are also part of `Humanoid`.
In addition, it includes the following fields for controlling attacks, equipment, and effects:

```yml
# Lists of attacks and equipment configurations

# Default attacks linked to items. Modify this to control which attacks the Humanoid can perform.
# Each attack references a specific item. The string list can contain multiple instances of the same attack.
# NOTE: Modifying ItemData affects all items with the same prefab name.
defaultItems: List<string>

# Randomized equipment choices
randomWeapon: List<string>
randomArmor: List<string>
randomShield: List<string>
randomSets: List<ItemSet>         # Alternative attack setups
randomItems: List<RandomItem>

# Effects applied in various situations
consumeItemEffects: EffectList
equipEffects: EffectList
perfectBlockEffect: EffectList

# Direct list of attack ItemData
# NOTE: Player clones should not use this list. They use player items (e.g., SwordIron) as attacks.
# Changing this will affect player items globally.
attacks: List<ItemData>
```

### ItemSet
```yml
name: string
items: List<string>
```

### RandomItem
```yml
prefab: string
chance: float
```