# Tameable

`Tameable` can be added/removed from creature

```yml
fedDuration: float
tamingTime: float
startsTamed: boolean
tamedEffect: EffectList
sootheEffect: EffectList
petEffect: EffectList
commandable: boolean
unsummonDistance: float
unsummonOnOwnerLogoutSeconds: float
unSummonEffect: EffectList
levelUpOwnerSkill: Skill
levelUpFactor: float
saddleItem: string
dropSaddleOnDeath: boolean
dropSaddleOffset: Vector3
dropItemVel: float
randomStartingName: List<string>
tamingSpeedMultiplierRange: float
tamingBoostMultiplier: float
nameBeforeText: boolean
tameText: string
```

### Procreation

`Procreation` can be added/removed from creature

```yml
updateInterval: float
totalCheckRange: float
maxCreatures: int
partnerCheckRange: float
pregnancyChance: float
pregnancyDuration: float
requiredLovePoints: int
offspring: string
minOffspringLevel: int
spawnOffset: float
spawnOffsetMax: float
spawnRandomDirection: boolean
seperatePartner: string
noPartnerOffspring: string
birthEffects: EffectList
loveEffects: EffectList
```

### GrowUp

`GrowUp` can be added/removed from creature
```yml
growTime: float
inheritTame: boolean
grownPrefab: string
altGrownPrefabs: List<string>
```