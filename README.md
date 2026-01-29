# MonsterDB
Plugin allows users to manipulate variables of monsters in the game and clone them

### v0.2.0 Overhaul

If you are migrating from 0.1.5 to 0.2.0, use v0.1.5 to export your modifications using `monsterdb export [prefabName]`,
then when using v0.2.0, add those files into the folder `Legacy` and in-game, use the command `monsterdb convert_all`,
this will iterate through your files and convert v0.1.5 files into v0.2.0 format.

v.0.2.0 is a complete re-write of the plugin to allow for easier future maintainability, and easier to use file structure.

- most data can be found in a single file to streamline file reading
- fields can be deleted to make files a lot easier to manipulate
- removed fields are ignored by plugin, and will subsequently not update that particular field
- this way, users can target specifically what they are interested in changing

v0.2.0 introduces new features

- modify eggs (e.g `ChickenEgg`, `AsksvinEgg`) and add egg component to any item
- modify fishes (e.g `Fish1`) and add fish component to any item
- custom factions can be defined using `Faction.yml` file
- localization files can be added in the `Localization` folder (e.g `English.yml`, `French.yml`), to allow to use `$enemy_mycreature` as the key in name fields

v0.2.0 updates
- simplified tameable component working with non-MonsterAI creatures (e.g `Deer`)
- simplified folder structure, any exported files are saved in `Export` folder, and any files to be imported should be added to `Import` folder, you can organize your files in sub-directories.
- spawn data is now built-in creature file

v0.2.0 notes

- if you are creating advanced creature attacks, you will still need to use `monsterdb clone_item [prefabName]` and define the fields there

### Features

- modify creatures, items, egg items, fish items
- clone creature, items, egg items, fish items
- make any creature tameable
- make any item into an egg
- make any item into a fish
- define custom factions
- define custom spawn data
- modify creature visuals (e.g. texture, color, scale)
- export textures as png

MonsterDB should be able to modify/clone any creatures, including added creatures from other mods

### Use cases

- create variants of any creatures with custom textures or audio
- make creatures more engaging by adding effects or attacks
- modify specific aspects of creatures

### Examples

Visit MonsterDB Github for curated examples

- Make `Neck` tameable and procreate `NeckEgg` which hatches into `Neck_hatchling` and grows up into a `Neck`
- Make `Player` into a `Human` NPC

### Server sync

1. Game load, plugin starts, reads all files from `Import` folder
2. ZNet starts (Multiplayer)
    3. if server: loads all imported files
        4. creates clones
        5. updates all
    6. if connecting to server: waits for files from server
        7. creates clones
        8. updates all
9. if `FileWatcher` enabled, any changes made on server, will automatically be sent to all clients
    10. update specific prefab from file

<div class="container"> 
   <img src="https://i.imgur.com/yDQlQQP.png" alt="Screenshot 1" width="600"/>
</div>
<div class="container"> 
   <img src="https://i.imgur.com/f7GPjiY.png" alt="Screenshot 2" width="600"/>
</div>
<div class="container"> 
   <img src="https://i.imgur.com/R2mU5il.png" alt="Screenshot 3" width="600"/>
</div>
<div class="container"> 
   <img src="https://i.imgur.com/JHPNKfu.png" alt="Screenshot 4" width="600"/>
</div>
<div class="container"> 
   <img src="https://i.imgur.com/wjeJBbg.png" alt="Screenshot 5" width="600"/>
</div>
<div class="container"> 
   <img src="https://i.imgur.com/j5CBR33.png" alt="Screenshot 6" width="600"/>
</div>
<div class="container"> 
   <img src="https://i.imgur.com/1D3NPLO.png" alt="Screenshot 7" width="600"/>
</div>

<style>
   .container {
      text-align: center;
   }
</style>

