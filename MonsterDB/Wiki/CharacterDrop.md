# CharacterDrop

CharacterDrop defines creature loot.

Note: 
- If `Drops` is removed completely from the file, the plugin will remove the component from the creature
- If `Drops` is added to file, the plugin will add the component to the creature

```yml
Drops:
  drops: List<Drop>       # See below
  dropsEnabled: bool
```

### Drop
```yml
prefab: string            # Prefab ID
amountMin: int
amountMax: int
chance: float             # (0.0 - 1.0)
onePerPlayer: bool
levelMultiplier: bool
dontScale: bool
```