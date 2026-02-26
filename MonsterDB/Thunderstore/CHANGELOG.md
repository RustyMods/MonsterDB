## 0.2.7
- tweaked emission color to multiply color by alpha to control intensity
- added Custom/Blob Shader material properties to modify
- `monsterdb export Texture [prefab]` now exports normal map, flow texture map, and emission map
- improved commands feedback
- added config toggle `full file` to control output of YML files. if on, will write entire YML file, instead of omitting default or null values
- added `Recipe` field to Item, Egg, Fish Types to modify/add recipes
- added `Conversions` field to Item, Egg, Fish Types to add conversions to `Fermenter`, `CookingStation`, `Smelter`
- added PNG file watcher to load new images while in-game

## 0.2.6
- new command to export SpawnData
- feature to modify vanilla spawn data

## 0.2.5
- overhauled commands, instead of individual commands for each type, now you can do `monsterdb export Character Greydwarf` | `monsterdb export Item SwordIron` instead of `monsterdb write Greydwarf` | `monsterdb write_item SwordIron`
- improved terminal output when commands are ran

## 0.2.4
- cleaned up serialization
- added support for scale to be set as a float instead of (x, y, z): e.g. `scale: 1.5` which will convert to: `scale: { x: 1.5, y: 1.5, z: 1.5 }` or `scale: { x: 1.5 }` which will convert to: `scale: x: 1.5, y: 1, z: 1`
- fixed humanoid item sets not updating

## 0.2.3
- improved custom saddle
- new field for saddle: `attachParent` use this field to target specific creature bone to attach to
- new command `monsterdb export_bones [prefabName]`: writes creature child hierarchy to learn structure of creature, to use to target specific child for saddle
- improved saddle control for custom saddled creatures
- new command: `monsterdb snapshot [prefabName]`: generate icon for prefab
- new feature, turn any prefab into an item (risky), e.g: SeekerEgg into an item, to use to create an egg for seeker; command: `monsterdb create_item [prefabName][newName]`
- improved egg growth percentage hover text
- fixed grow up hover text error when creature is not loaded
- created custom controller for volture to enable walking, landing, takeoff (originally not enabled in vanilla volture)
- volture custom controller added automatically if volture modified

## 0.2.2
- added last priority to startup to load after WackyDB
- fixed item scaling to always scale based off original scale to fix continuously multiplying scale off new value, when updating multiple times during same session
- added support to edit and add raids
- added procreation hover text (configurable on/off), default (off)
- added grow up hover text (configurable on/off), default (off)

## 0.2.1
- fixed scaling items
- changed color fields to use RGBA() instead of hex, hex still works as color value

## 0.2.0
- complete overhaul
- file structure overhaul
- single file editing
- removed support for conversion from RRR
- added support to modify or turn any item into a fish
- added support to modify or turn any item into a egg
- added support to localize text