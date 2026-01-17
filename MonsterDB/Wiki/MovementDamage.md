# MovementDamage

`MovementDamage` can be added/removed from creature.

```yml
runDamageObject: string       # Prefab ID, do not change
areaOfEffect: AOE
```

### AOE

Only damage is relevant to MovementDamage, but perhaps advanced users can utilize other fields

```yml
name: string
useAttackSettings: true
damage: Damage
scaleDamageByDistance: boolean
dodgeable: boolean
blockable: boolean
toolTier: int
attackForce: float
backstabBonus: float
statusEffect: string
statusEffectIfBoss: string
attackForceForward: float
spawnOnHitTerrain: string
hitTerrainOnlyOnce: boolean
spawnOnGroundType: GroundMaterial     # See below
groundLavaValue: float
hitNoise: float
placeOnGround: boolean
randomRotation: boolean
maxTargetsFromCenter: int
multiSpawnMin: int
multiSpawnMax: int
multiSpawnDistanceMin: float
multiSpawnDistanceMax: float
multiSpawnScaleMin: float
multiSpawnScaleMax: float
multiSpawnSpringDelayMax: float
chainStartChance: float
chainStartChanceFalloff: float
chainChancePerTarget: float
chainObj: string
chainStartDelay: float
chainMinTargets: int
chainMaxTargets: int
chainEffects: EffectList
chainDelay: float
chainChance: float
damageSelf: boolean
hitOwner: boolean
hitParent: boolean
hitSame: boolean
hitFriendly: boolean
hitEnemy: boolean
hitCharacters: boolean
hitProps: boolean
hitTerrain: boolean
launchCharacters: boolean
launchForceUpFactor: float
useTriggers: boolean
triggerEnterOnly: float
radius: float
activationDelay: float
ttl: float
ttlMax: float
hitAfterTtl: float
hitInterval: float
hitOnEnable: boolean
attachToCaster: boolean
hitEffects: EffectList
initiateEffect: EffectList
```

### GroundMaterial

```yml
None
Default
Water
Stone
Wood
Snow
Mud
Grass
GenericGround
Metal
Tar
Ashlands
Lava
Everything
```