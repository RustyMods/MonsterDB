# Factions

MonsterDB allows to define custom factions. Be careful when modifying a character faction field. Any values that isn't part of the default Valheim factions, will automatically be generated as a custom faction.

You will need to define the faction properties in `Faction.yml` in MonsterDB top directory (e.g `BepInEx/config/MonsterDB/Faction.yml`)

### Example
```yml
- MyFaction:
  name: MyFaction
  targetTamed: true
  targetTameables: true
  allies:
  - ForestMonsters
  - Players
- MyOtherFaction:
  name: MyOtherFaction
  targetTamed: false
  targetTameables: true
  allies:
  - MountainMonsters
  - PlainsMonsters
```

Unlike default factions, MonsterDB factions allow to include many factions as `allies`

### Default Factions

```
Players
AnimalsVeg
ForestMonsters
Undead
Demon
MountainMonsters
SeaMonsters
Boss
MistlandsMonsters
Dverger
PlayerSpawned
TrainningDummy
```