# MonsterDB
Plugin allows users to manipulate variables of monsters in the game and clone them

## Commands
```
monsterdb write [prefabName]                    - Writes monster data to disk
monsterdb update [prefabName]                   - Forice updates monster using registered data
monsterdb clone [prefabName] [cloneName]        - Clones prefab and registers data
monsterdb reset [prefabName]                    - Resets prefab to orignal data
monsterdb reload                                - Updates all registered data
monsterdb write_item [prefabName]               - If prefab is an item (monster attack data), writes data to disk to use as reference
monsterdb clone_item [prefabName] [cloneName]   - clones prefab and saves to items folder, copy it to your creatures item folders
monsterdb write_spawn [prefabName]              - Writes spawn data - location dependant, go to area where creature spawns
```
## How To
In-game, use the commands to write data to disk to start manipulating monsters

Plugin will watch the files and update the monsters upon any changes

You will find list of data organized into folders
```
Effects        // Specific files are set in this folder for you to manipulate
Items          // Humanoid creatures only - folders are organized in this
Visual         // Manipulate the materials and scale
```
## Effects Folder
Files generated are based on the data in the game, if you make creature tameable or talker,
you will need to add the missing YML files
```
Example:
Tameable component looks for:
- LoveEffects.yml
- PetEffects.yml
- SoothEffects.yml
- UnsummonEffecst.yml
```
These files contains list of effects. You can add or remove items in this list.
## How to make creature tameable
You will to add two files to make creature tameable:
```
- MonsterAI.yml
- Tameable.yml
```
Copy the data from the AnimalAI.yml file into the MonsterAI.yml, without removing the extra lines
in the MonsterAI.yml file. (line 39+):

MonsterAI is AnimalAI plus extra data:
```
AlertRange: 6
FleeIfHurtWhenTargetCannotBeReached: true
FleeUnreachableSinceAttack: 30
FleeUnreachableSinceHurt: 20
FleeIfNotAlerted: true
FleeIfLowHealth: 0
FleeTimeSinceHurt: 0
FleeInLava: true
CirculateWhileCharging: true
CirculateWhileChargingFlying: false
EnableHuntPlayer: false
AttackPlayerObjects: true
PrivateAreaTriggerThreshold: 4
InterceptTimeMax: 1
InterceptTimeMin: 0
MaxChaseDistance: 0
MinAttackInterval: 0
CircleTargetInterval: 0
CircleTargetDuration: 5
CircleTargetDistance: 10
Sleeping: false
WakeupRange: 5
NoiseWakeup: false
MaxNoiseWakeupRange: 50
WakeupDelayMin: 0
WakeupDelayMax: 0
AvoidLand: false
ConsumeItems:
- Carrot
- Turnip
- Onion
- Mushroom
- Raspberry
- Blueberries
ConsumeRange: 1
ConsumeSearchRange: 10
ConsumeSearchInterval: 10
```
## How to make creature talk
Add NPCTalk.yml into the creature folder (only works with MonsterAI creatures - monsters that attack)

If want NPC Talk effects, you will need to add these files in the Effects folder:
```
- RandomTalkFX.yml
- RandomGreetFX.yml
- RandomGoodbyeFX.yml
```
## Textures
Place png files in the CustomTextures directory

Plugin will load them during boot-up and you can use the file name in the Visual/Materials files

You will need to share your textures with your friends if you want them.

## Materials
```
ShaderType: Standard // Shader Type - Informational only
Name: Material Name  // Name of material - Used as a key to a know which material to change // Do not change
_Color:              // Manipulate the color of the material
  r: 0.8529412       // red
  g: 0.8529412       // green
  b: 0.8529412       // blue
  a: 1               // alpha (Opacity)
_MainTex: ''         // Change the texture using the name of texture, will search in custom textures and in-game textures
_EmissionColor:      // Manipulate the emission of the material (glow)
  r: 0               // red
  g: 0               // green
  b: 0               // blue
  a: 1               // alpha
```

## How to change attacks
Best practice is to save item data using commands, and use them as reference when manipulating
ItemData.yml
```
Name: Dverger_melee               // ID - You can change this to use a different prefab, although items are quite specific
AnimationState: OneHanded         // State the creature holds the item
SpawnOnHit: ''                    // PrefabName
SpawnOnHitTerrain: ''             // PrefabName
AttackStatusEffect: ''            // StatusEffectName - plugin will search the ObjectDB StatusEffects library for a match
AttackStatusEffectChance: 1       // Value 0 - 1
AttackType: Horizontal            // Type
AttackAnimation: attack_poke      // This field is important and unique to the creature - it is the key to the attack animation
AttackOriginJoint: ''             // Use Visual.txt to know the names of available joints
SpawnOnTrigger: ''                // PrefabName
Projectile: ''                    // PrefabName
```
## How to make creatures spawn
You will find a directory named SpawnData

In this folder, the plugin generates an Example.yml file.

Copy and Paste this file, rename it to something that is not Example.yml
```
m_name: ''                    // Name of the SpawnData - Does not do anything
m_enabled: false              // Easily enable / disable spawn file
m_devDisabled: false          // Not sure what this does
m_prefab:                     // PrefabName added here
m_biome:                      // Biome added here
m_biomeArea: Everything       // Edge, Median, Everything
m_requiredGlobalKey: ''       // Add key as conditionals
m_requiredEnvironments: []    // List of environment names
m_foldout: false              // Not sure what this does
```
The command 'write_spawn' attempts to find spawn data matching the creature name, and saves it into a reference folder.
These 'reference' files will not modify existing spawn data if added to main folder. Rather, it will add.

These are to be used as references to develop your own spawn files.
## Import / Export
```
monsterdb import - imports all creature data files in the Import directory
monsterdb export [prefabName] - exports to a single file creature data to share
```
You can use this file structure to easily share MonsterDB files with your friends or even use this file structure
to create/edit creatures.
## Special considerations
Only Tameable.yml, NPCTalk.yml can be added into creature folders and actually add the components.
