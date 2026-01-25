# MonsterDB 

MonsterDB allows you to inspect, modify, and extend Valheim prefabs through structured YML files.
All supported components and behaviors are defined and parsed based on file headers and base types.

### Header

All main files start with

```yml
Prefab: string
IsCloned: bool
ClonedFrom: string
Type: BaseType
```

These fields determine how MonsterDB interprets and processes the rest of the file.

The fields `GameVersion` and `ModVersion` are optional and do not affect runtime behavior.
They exist solely to help track compatibility with Valheim or MonsterDB updates for long-term maintainability.

### How-to modify a creature

1. Use the `write` command to export a creature’s YML files.
   This includes:
    - The main creature file
    - Item files (if the creature has attacks)
    - A ragdoll file (if one exists)

2. Modify the exported YML files as needed.
   Many fields are omitted when they match default values (for example, `0`).

3. Some components can be added or removed.
   When removing fields:
    - Be cautious, as removing a field may remove an entire component.
    - If you want to keep a removable component unchanged, replace its contents with `{}`.

4. Refer to the other files in the `README` folder for details on which fields and components can be safely added or removed.

5. For fields under the main containers:
    - Removing a field causes MonsterDB to ignore it during updates.
    - This allows you to delete any fields you do not intend to modify.

Examples can be found in the GitHub Wiki folder.

### How-to clone a creature

1. Use the `clone` command to export a cloned creature’s YML file.
2. Modify the cloned file to suit your needs.

### How-to

- Any item can be converted into an egg by adding the `EggGrow` component.
  The item will hatch into the specified prefab.

- Any item can be converted into a fish by adding the `Fish` component.
  This allows the item to swim and behave like a fish in water.

## Special MonsterDB behaviours

### Tameable Creatures

MonsterDB allows any creature to be made tameable, including animals such as Deer or Hare.
This is normally impossible because they use `AnimalAI`. MonsterDB adds the `AnimalTameable`
component to override this behavior and allow food searching and consumption.

### Rideable Creatures

MonsterDB simplifies making creatures rideable.
Instead of requiring a saddle setup, the player is attached directly to the creature’s root or `attachParent` field
You should adjust `attachOffset` to ensure correct visual alignment and use command: `monssterdb export_bones` to figure out which bone to attach to under `attachParent`

### Player Cloning

When cloning `Player`, all player-specific components are removed.
The prefab is converted to use the custom MonsterDB `Human` component, which inherits from `Humanoid`.

## PNG

Any `.png` files placed in the `Import` folder are automatically registered by MonsterDB.

Once registered, you can reference a PNG by its filename **without** the `.png` extension.

### Notes

#### Visuals

When modifying the visuals on a creature, you will need to do the same to it's ragdoll, if they have one, if you wish the ragdoll to look appropriate. Ragdoll scale is not always the same as the creature scale.
(e.g. Neck scale: 1, 1, 1 while Neck_ragdoll scale: 0.3, 0.3, 0.3, so if you want to half their scale, Neck: 0.5, 0.5, 0.5 while Neck_ragdoll: 0.15, 0.15, 0.15)

You can use the command `write_visual` or `clone_visual` to export a prefab YML with just the Visual data, to create custom FX prefabs. Useful when modifying a creature appearance, to also target their effect prefabs.

```yml
ItemData:
  icons:
  - myCustomPNGNameWithoutExtension
Visuals:
  renderers:
  - prefab: Cube.001
    parent: Visual
    index: 1
    materials:
    - name: MountainGolem_mat
      mainTexture: myCustomPNGNameWithoutExtension
```

### Audio

Any audio files placed in the `Import` folder are automatically registered by MonsterDB.

- Audio files are **not synced** with the server.
- Distribute them manually to all players.

Use the audio filename without its extension as the `prefab` value.

```yml
hitEffects:
  effectPrefabs:
  - prefab: myAudioFileNameWithoutExtension 
```

### Commands

all commands begin with: `monsterdb`

use `help` to see this list in-game

- `monsterdb export_main_tex [prefabName]`: export creature main texture
- `monsterdb search_tex [query]`: search cached textures
- `monsterdb export_tex [textureName]`: export texture as png
- `monsterdb write [prefabName]`: write creature YML file to `Save` Folder
- `monsterdb write_all`: write all creature YML to `Save` Folder
- `monsterdb mod [prefabName]`: read creature YML file and update from `Modified` folder
- `monsterdb revert [prefabName]`: revert creature to factory settings
- `monsterdb clone [prefabName][newName]`: clone creature, must be a character
- `monsterdb write_egg [prefabName]`: write egg YML file to `Save` Folder
- `monsterdb write_all_egg`: write all egg YML to `Save` Folder
- `monsterdb mod_egg [prefabName]`: read egg YML from `Modified` Folder and update
- `monsterdb revert_egg [prefabName]`: revert egg item to factory settings
- `monsterdb clone_egg [prefabName][newName]`: clone egg item, must be an item
- `monsterdb write_item [prefabName]`: export item YML
- `mosnterdb mod_item [fileName]`: read item YML file and update
- `monsterdb clone_item [prefabName][newName]`: clone item and export YML file
- `monsterdb write_fish [prefabName]`: export fish YML file
- `monsterdb mod_fish [fileName]`: read fish YML file and update
- `monsterdb clone_fish [prefabName][newName]`: clone fish and export YML file
- `monsterdb export_sprite [spriteName]`: export sprite texture as PNG
- `monsterdb search_sprite [query]`: search cached sprites
- `monsterdb write_visual [prefabName]`: export prefab visual YML
- `monsterdb clone_visual [prefabName][newName]`: clone prefab and export visual YML
- `monsterdb export_bones [prefabName]`: export bone structure as YML
- `monsterdb create_item [prefabName][newName]`: advanced command to try to convert any prefab into an item, does not work with creatures
- `monsterdb snapshot [prefabName]`: snapshots a prefab into an icon and exports as PNG

### Examples

Visit: https://github.com/RustyMods/MonsterDB/tree/main/MonsterDB/Wiki/Reference/Import