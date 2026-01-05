# Egg

Egg type is a special case, where Eggs are Items that have the `EggGrow` component. 
MonsterDB allows to convert any items into eggs by adding the component.

```yml
GameVersion: 0.221.4
ModVersion: 0.2.0
Type: Egg
Prefab: string
ClonedFrom: string
IsCloned: boolean
EggGrow:
  growTime: float
  grownPrefab: string               # Prefab ID
  tamed: boolean                    # Starts tamed, if tameable
  updateInterval: float
  requireNearbyFire: boolean
  requireUnderRoof: boolean
  requireCoverPercentige: float
  hatchEffect: EffectList
  growingObject: string             # Child Transform Name
  notGrowingObject: string          # Child Transform Name
Visuals:
  scale:
    x: 1
    y: 1
    z: 1
  materials:
    - name: string
      shader: string
      color: string                 # Hex code
      hue: float
      saturation: float
      value: float
      emissionColor: string         # Hex code
      mainTexture: string           # Texture Name
name: string                        # Item display name
description: string                 # Item description
```