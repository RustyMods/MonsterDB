# MonsterDB
Plugin allows users to manipulate variables of monsters in the game and clone them

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

