# VisEquipment

Visual Equipment controls how items are attached to characters. Not all characters have VisEquipment component.
Edit ItemData fields to control which `joint` an item should attach to. Use VisEquipment fields to edit the joints.

```yml
leftHand: Transform               # See below
rightHand: Transform
helmet: Transform
backShield: Transform
backMelee: Transform
backTwohandedMelee: Transform
backBow: Transform
backTool: Transform
backAtgeir: Transform
```

## Use case

Edit joint of a character to properly have items displayed, (i.e Skeleton helmet joint needs adjusting to have helmet look correct)

### Transform

`(prefab, parent, index)` are used to create a unique identifier for child, since many children can share the same name.

```yml
name: string
parent: string
index: integer
position: Vector
rotation: Vector
scale: Vector
```

#### Vector
```yml
x: float
y: float
z: float
```

Use the command: `monsterdb Export bones [ID]` to get a layout of the creature hierarchy so you can target specific joints. 