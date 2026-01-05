# BaseAI

`BaseAI` is the base class that defines all creature AI in Valheim.
Both `AnimalAI` and `MonsterAI` inherit from BaseAI, meaning they include all of the fields below in addition to their own extensions.

```yml
# Detection
viewRange: float
viewAngle: float
hearRange: float
mistVision: bool                      # If true, they can detect through Mistlands mist

alertedEffects: EffectListRef
idleSound: EffectListRef
idleSoundInterval: float
idleSoundChance: float

# Movement
patrol: bool                            # If true, will not wander
pathAgentType: Pathfinding.AgentType
moveMinAngle: float
smoothMovement: bool

# Special movement
serpentMovement: bool
serpentTurnRadius: float

# Random movement
jumpInterval: float
randomCircleInterval: float
randomMoveInterval: float
randomMoveRange: float

# Flying
randomFly: bool
chanceToTakeoff: float
chanceToLand: float
groundDuration: float
airDuration: float
maxLandAltitude: float
takeoffTime: float
flyAltitudeMin: float
flyAltitudeMax: float
flyAbsMinAltitude: float

# Fleeing
avoidFire: bool
afraidOfFire: bool
avoidWater: bool
avoidLava: bool
skipLavaTargets: bool
avoidLavaFlee: bool
fleeRange: float
fleeAngle: float
fleeInterval: float

# Aggressiveness
aggravatable: bool
passiveAggresive: bool

# Misc
spawnMessage: string
deathMessage: string
alertedMessage: string
```

### AnimalAI

`Character` uses AnimalAI, as opposed to Humanoid that use MonsterAI.

```yml
timeToSafe: float     # Time until they begin alert cooldown
```

### MonsterAI

`Humanoid` uses MonsterAI, as opposed to Character that use AnimalAI. MonsterAI inherits from `BaseAI`, thus will have all of BaseAI fields including below:

```yml
# Flee
alertRange: float
fleeIfHurtWhenTargetCantBeReached: bool
fleeUnreachableSinceAttacking: float
fleeUnreachableSinceHurt: float
fleeIfNotAlerted: bool
fleeIfLowHealth: float
fleeTimeSinceHurt: float
fleeInLava: bool
fleePheromoneMin: float
fleePheromoneMax: float

circulateWhileCharging: bool
circulateWhileChargingFlying: bool
enableHuntPlayer: bool                      # This value is dynamic and can change during scene, modify start value
attackPlayerObjects: bool                   # If true, will attack built objects
privateAreaTriggerTreshold: int
interceptTimeMax: float
interceptTimeMin: float
maxChaseDistance: float
minAttackInterval: float
circleTargetInterval: float
circleTargetDuration: float
circleTargetDistance: float
sleeping: bool                              # If true, will start asleep when spawned, if Animator is setup to do so
wakeupRange: float
noiseWakeup: bool
maxNoiseWakeupRange: float
wakeupEffects: EffectListRef
sleepEffects: EffectListRef
wakeUpDelayMin: float
wakeUpDelayMax: float
fallAsleepDistance: float
avoidLand: bool
consumeItems: List<string>                  # Prefab IDs of items it can consume, to use in conjuction with Tameable
consumeRange: float
consumeSearchRange: float
consumeSearchInterval: float
```