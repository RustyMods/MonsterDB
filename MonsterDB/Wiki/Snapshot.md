Creating Item Icons with `monsterdb snapshot`

MonsterDB includes a snapshot system that allows you to capture any prefab and convert it into a `.png` image. This image can then be edited in your preferred image manipulation software and re-imported into MonsterDB, where it will automatically be converted into a usable in-game Sprite.

This workflow is especially useful when working with prefabs that do not already have suitable icons.

## Snapshotting a Prefab

To generate a snapshot image:
1. Open the in-game console.
2. Run the following command: `monsterdb snapshot` followed by the prefab ID.

MonsterDB will render the prefab and export it as a `.png` file.

Optionally you can define light intensity and camera rotation when snapshotting
```
monsterdb snapshot SeekerEgg 0.7 0 45 0
```

## Editing and Importing the icon
Once the snapshot is generated:
1. Edit the `.png` file
2. Place the edited `.png` into your MonsterDB Import folder.

On the next load, MonsterDB will automatically convert the image into a `Sprite` and use the name of the `.png` file as the name of the `Sprite`

3. Use the name to target the Sprite in the icon field

## Example: Creating a SeekerEgg Item icon

In this example, we wanted to make Seeker's tameable and produce eggs that can hatch into SeekerBrood that grows up into a new Seeker. We found that there already exists an in-game asset that would fit our egg item needs perfectly, but it was not an item. So we cloned the `SeekerEgg` prefab using `create_item` command.

Because no existing item icon closely matched the `SeekerEgg`, we used
```
monsterdb snapshot SeekerEgg
```
to generate a custom icon:

1. Snapshot the `SeekerEgg` prefab.
2. Edit the resulting `.png`
3. Rename the `.png` file as `SeekerEgg_icon.png`
3. Import the edited image back into MonsterDB
4. Add `SeekerEgg_icon` in icon field in the ItemData

With the icon in place, the new `SeekerLarva` item can now:
1. Be picked up by the player
2. Display a proper inventory icon

![alt](https://i.imgur.com/jTkLoK9.png)

## `SeekerLarva.yml` file
```yml
GameVersion: 0.221.4
ModVersion: 0.2.2
Type: Egg
Prefab: SeekerLarva
ClonedFrom: SeekerEgg
IsCloned: true
ItemData:
  name: $item_seekeregg
  description: $item_seekeregg_description
  icons:
  - SeekerEgg_icon
  maxStackSize: 20
EggGrow:
  growTime: 1800
  grownPrefab: SeekerBrood
  tamed: true
  updateInterval: 5
  requireNearbyFire: true
  requireUnderRoof: true
  requireCoverPercentige: 0.7
  hatchEffect:
    effectPrefabs:
    - prefab: fx_egg_splash
  notGrowingObject: Not Growing
```

