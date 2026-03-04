# Raids

Raid files should exist under the `Raids` folder at: `BepInEx/config/MonsterDB/Raids`

```yml
name: string                              # Required identifier
enabled: boolean                          
random: boolean                           
duration: float 
nearBaseOnly: boolean
pauseIfNoPlayerInArea: boolean
eventRange: float
standaloneInterval: float                 # If over 0, will override event system interval check
standaloneChance: float                   # If over 0, will override event system chance check
spawnerDelay: float
biome: Biome                              # See below
requiredGlobalKeys: List<string>
altRequiredKnownItems: List<string>
altRequiredPlayerKeysAny: List<string>
altRequiredPlayerKeysAll: List<string>
notRequiredGlobalKeys: List<string>
altRequiredNotKnownItems: List<string>
altNotRequiredPlayerKeys: List<string>
startMessage: string
endMessage: string
forceMusic: string
forceEnvironment: string
spawn: List<SpawnData>                      # See SpawnData.md
```

#### Biome

```yml
None
Meadows
Swamp
Mountain
BlackForest
Plains
AshLands
DeepNorth
Ocean
Mistlands
All
```