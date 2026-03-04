<div align="center">

<br/>

<h1 align="center">MonsterDB</h1>

<h3 align="center">Manipulate, clone, and customize any creature or item in Valheim — including those added by other mods.</h3>

<br/>

<table align="center">
  <tr>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/frostling.png?raw=true" alt="Frostling" width="380" height="380" style="object-fit:cover"/></td>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/volture.png?raw=true" alt="Volture" width="380" height="380" style="object-fit:cover"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/deer.png?raw=true" alt="Deer" width="380" height="380" style="object-fit:cover"/></td>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/seeker.png?raw=true" alt="Seeker" width="380" height="380" style="object-fit:cover"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/obsidian-golem.png?raw=true" alt="Obsidian Golem" width="380" height="380" style="object-fit:cover"/></td>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/seeker-egg.png?raw=true" alt="Seeker Egg" width="380" height="380" style="object-fit:cover"/></td>
  </tr>
  <tr>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/skeleton.png?raw=true" alt="Bronze Skeleton" width="380" height="380" style="object-fit:cover"/></td>
    <td><img src="https://github.com/RustyMods/MonsterDB/blob/main/MonsterDB/Screenshots/bear.png?raw=true" alt="Bear" width="380" height="380" style="object-fit:cover"/></td>
  </tr>
</table>

<br/>

<table><tr><td width="900">
<br/>

MonsterDB is a Valheim plugin that gives you full control over creatures and items in the game. Modify stats, visuals, and behaviors — or clone any prefab to create entirely new variants. MonsterDB works with any creature or item, including those added by other mods.

<br/>
</td></tr></table>

<br/>

<h1 align="center">Features</h1>

<table><tr><td width="900">
<br/>

### Modify & Clone
- Modify or clone **creatures, items, egg items, and fish items**
- Modify **creature visuals** — texture, color, and scale
- Export **textures as PNG**

### Creature Customization
- Make any creature **tameable**
- Define **custom factions** and **custom spawn data**
- Add effects, attacks, and audio to make creatures more engaging

### Item Customization
- Turn any item into an **egg** or a **fish**

<br/>
</td></tr></table>

<br/>

<h1 align="center">Use Cases</h1>

<table><tr><td width="900">
<br/>

- Create **variants of any creature** with custom textures or audio
- Make creatures more engaging by adding **effects or attacks**
- Modify specific aspects of creatures for balance or gameplay purposes
- Build entirely new creature ecosystems using **clones and spawn data**

<br/>
</td></tr></table>

<br/>

<h1 align="center">Examples</h1>

<table><tr><td width="900">
<br/>

Visit the [MonsterDB GitHub](https://github.com/RustyMods/MonsterDB) for curated examples.

- Make `Neck` tameable and procreate `NeckEgg` which hatches into `Neck_hatchling` and grows up into a `Neck`
- Make `Player` into a `Human` NPC

<br/>
</td></tr></table>

<br/>

<h1 align="center">Sync</h1>

<table><tr><td width="900">
<br/>

MonsterDB includes full server synchronization support so all clients stay consistent with the server's configuration.

| Step | Description |
|:---|:---|
| **Game Load** | Plugin starts and reads all files from the `Import` folder |
| **Server** | Loads imported files, creates clones, and applies all updates |
| **Client** | Waits for files from the server, then creates clones and applies updates |
| **FileWatcher** | If enabled, any changes made on the server are automatically sent to all connected clients |

<br/>
</td></tr></table>

<br/>

<h1 align="center"> RRR </h1>

<table><tr><td width="900">
<br/>

To convert `RRR.json` files into `MDB.yml` files, install https://thunderstore.io/c/valheim/p/ValheimModding/JsonDotNET/ 
to gain access to command: `monsterdb rrr` which will look for files within folder `RRR` at `BepInEx/config/MonsterDB/RRR`

RRR Converter was designed to parse both `RRR.json` and `DropThat.cfg` to coalesce into `MDB.yml`

<h3> Notes </h3>

- Not all RRR values are transfered over, i.e (`cTintColor`, `cTintItems`, `cBodySmoke`)
- Recommended to review exported files and clean up fields ( fields that do not change any values )

List of fields not copied:
```
- dtAttackDamageOverride
- fAttackDamageTotalOverride
- sAttackProjectileOverride
- cTintColor
- cTintItems
- cBodySmoke
- bAttackPlayerObjectsWhenAlerted
- fHairValueMin
- fHairValueMax
- bCanTame
- bAlwaysTame
- bCanProcreateWhenTame
- bFxNoTame
- bSfxNoAlert
- bSfxNoIdle
- bSfxNoHit
- bSfxNoDeath
```

<br/>
</td></tr></table>

</div>
