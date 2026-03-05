# Main files

MonsterDB supports 13 Base Types, defined by the Type field in each file.
This field determines how MonsterDB parses and processes the file and must not be changed.

### BaseType
```
Humanoid
Character
Human
Egg
Item
Fish
Projectile
Ragdoll
SpawnAbility
Visual
CreatureSpawner
SpawnArea
SpawnData
All
```

Each Base Type corresponds to a specific prefab structure and set of required components.

### Humanoid

A Humanoid is a creature that uses the Humanoid and MonsterAI components.
Unlike Character, Humanoids support item-based attacks, allowing weapons and equipment to define combat behavior.

In vanilla Valheim, only Humanoids can be tamed. MonsterDB extends this limitation by adding custom components that allow any creature type to be tameable.

### Character

Character is a creature with components `Character` and `AnimalAI`

### Human

Human is a creature with components `Human` which is a custom MonsterDB component that inherits from `Humanoid` and `MonsterAI`

### Egg

Egg is an item with components `EggGrow` and `ItemDrop`
It represents a growable entity that eventually spawns a creature.

### Item

Item is an item with component `ItemDrop`

### Fish

Fish is an item with components `Fish` and `ItemDrop`

### Projectile

Projectile is a prefab with component `Projectile` or `Aoe` or `TriggerSpawnAbility`

### Ragdoll

Ragdoll is a prefab with component `Ragdoll`

### SpawnAbility

SpawnAbility is a prefab with component `SpawnAbility`

### Visual

Visual is a prefab renderer, particle system and light data. Use this if you wish to modify just the visual of a prefab. Useful for targeting effect prefabs.

### CreatureSpawner

Creature spawner is a prefab with component `CreatureSpawner` and optionally `RandomSpawn`, (i.e `Spawner_Bat` `Spawner_Charred`).
These prefabs typically do not have a mesh thus are invisible.

### SpawnArea

Spawn area is a prefab with component `SpawnArea`. These prefabs are similar to creature spawner, but do have a mesh. (i.e: `BonePileSpawner`)

### SpawnData

Spawn data contains fields to manipulate SpawnSystem spawn data. SpawnData is exported along side new cloned creatures to keep relevant edits within the same file,
but you can have it exist in its own file by using `SpawnData` type.

### All

All includes all BaseType in a single aggregate file. This is used to easily share many MonsterDB files as a single file.


## Example

```yml
GameVersion: 0.221.4
ModVersion: 0.2.0
Type: Human
Prefab: Human             # prefab ID
ClonedFrom: Player       
IsCloned: true
```

For automatic handling of these fields, use MonsterDB commands to clone, else, you can directly modify `Prefab`, `ClonedFrom` and `IsCloned` if you want to clone an object.