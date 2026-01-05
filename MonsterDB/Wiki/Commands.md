# Commands

all commands begin with: `monsterdb`

use `help` to see this list in-game

- `monsteredb export_main_tex [prefabName]`: export creature main texture
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
- `monsterdb setup_spawn [prefabName]`: write new spawn data YML and load into spawn system

```yaml
export_main_tex:  [prefabName] export creature main texture                               # only admin: False
export_tex:       [textureName] export texture as png                                     # only admin: False
write:            [prefabName] save creature reference to Save folder                     # only admin: False
write_all:        save all creatures to Save folder                                       # only admin: False
mod:              [prefabName] read creature file from Modified folder and update         # only admin: True
revert:           [prefabName] revert creature to factory settings                        # only admin: True
clone:            [prefabName][newName] must be a character                               # only admin: True
write_egg:        [prefabName] save egg reference to Save folder                          # only admin: False
write_all_egg:    save all egg references to Save folder                                  # only admin: False
mod_egg:          [prefabName] read egg reference from Modified folder                    # only admin: True
revert_egg:       [prefabName] revert egg to factory settings                             # only admin: True
clone_egg:        [prefabName][newName] must be an item                                   # only admin: True
setup_spawn:      [prefabName] write new spawn data file and load into spawn system       # only admin: False
```