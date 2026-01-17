# Fish
```yml
name: string
swimRange: float
minDepth: float
maxDepth: float
speed: float
acceleration: float
turnRate: float
wpDurationMin: float
wpDurationMax: float
avoidSpeedScale: float
avoidRange: float
height: float

hookForce: float
staminaUse: float
escapeStaminaUse: float
escapeMin: float
escapeMax: float
escapeWaitMin: float
escapeWaitMax: float
escapeMaxPerLevel: float
baseHookChance: float
pickupItemStackSize: int
blockChangeDurationMin: float
blockChangeDurationMax: float
collisionFleeTimeout: float

baits: BaitSetting[]
extraDrops: DropTable

jumpSpeed: float
jumpHeight: float
jumpForwardStrength: float
jumpHeightLand: float
jumpChance: float
jumpOnLandchance: float
jumpOnLandDecay: float
maxJumpDepthOffset: float
jumpOnLandRotation: float
waveJumpMultiplier: float
jumpMaxLevel: float
jumpEffects: EffectList
```

### BaitSetting
```yml
bait: string
chance: float
```

### DropTable
```yml
drops: DropData[]
dropMin: int
dropMax: int
dropChance: float
oneOfEach: bool
```

### DropData
```yml
item: string
stackMin: int
stackMax: int
weight: float
dontScale: bool
```
