# Visuals

Visuals targets different parts of the prefab. 
The top level transform which controls the scale, and the internal body which holds references to the materials.
`PlayerVisual` inherits from `Visual` with extra fields.

```yml
Visuals:
  scale: Scale                          # See below
  levelSetups: List<LevelSetup>         # See below
  renderers: List<Renderer>             # See below
  lights: List<Light>                   # See below
  particleSystems: List<ParticleSystem> # See below
  ### Extra fields for humans
  modelIndex: List<int>                 # 0 = male, 1 = female
  beards: List<string>                  # Prefab IDs
  hairs: List<string>                   # Prefab IDs
  hairColors: List<string>              # Hex codes or RGBA()
  skinColors: List<string>              # Hex codes or RGBA()
```

### Scale

Scale accepts two data forms: single or vector, meaning you can either do:
```yml
scale: 1.3
```
or 
```yml
scale:
  x: 1.3
  y: 1.3
  z: 1.3
```

#### Vector
```yml
x: float
y: float
z: float
```

### LevelSetup
```yml
scale: float                  # Scale multiplier
hue: float                    # Only works with Custom/Creature shader
saturation: float             # Only works with Custom/Creature shader
value: float                  # Only works with Custom/Creature shader
setEmissiveColor: boolean
emissiveColor: string         # Hex code or RGBA
enableObject: string          # Child Transform Name
```

`(prefab, parent, index)` are coupled together to create a unique identifiers
this is used to specifically search for the correct child to update.
MDB does this because many children can share the same name within the hierarchy of the prefab

### Renderer

```yml
prefab: string                # Target renderer name
parent: string                # Target renderer parent name
index: integer                # Target renderer sibling index
active: boolean               # if prefab is visible
enabled: boolean              # if renderer is enabled
### both active and enabled basically do the same thing, but LevelSetup can change active, so to permanently disable, use enabled: false
materials: List<Material>     # See below
```

### Light

```yml
prefab: string                # Target light name
parent: string                # Target light parent name
index: integer                # Target light sibling index
active: boolean               
color: string                 # Hex code or RGBA()
type: LightType               # See below
intensity: float
range: float
```

#### LightType
```yml
Spot
Directional
Point
Rectangle
Disc
Pyramid
Box
Tube
```

### Particle System

```yml
prefab: string                          # Target particle system name
parent: string                          # Target particle system parent name
index: integer                          # Target particle system sibliing index
startColor: MinMaxGradient              # See below
customData: CustomDataModule            # See below
colorOverLifetime: MinMaxGradient       # See below
```

#### MinMaxGradient

```yml
mode: ParticleSystemGradientMode        # See below
color: string                           # Hex code or RGBA()
colorMin: string                        # Hex code or RGBA()
colorMax: string                        # Hex code or RGBA()
gradient: Gradient                      # See below
gradientMin: Gradient                   # See below
gradientMax: Gradient                   # See below
```

#### ParticleSystemGradientMode (enum)

`mode` defines how the MinMaxGradient chooses colors, if you choose `Color` then you only need to edit the `colorMax` field,
while if you choose `TwoColor`, then you only need to edit `colorMin` and `colorMax`.

```yml
Color                         # field: colorMax                 
Gradient                      # field: gradient
TwoColor                      # field: colorMin, colorMax
TwoGradients                  # field: gradientMin, gradientMax
RandomColor                   # field: gradientMax
```

#### Gradient

```yml
mode: GradientMode                  # See below
alphaKeys: GradientAlphaKey         # See below
colorKeys: GradientColorKey         # See below
colorSpace: ColorSpace              # See below
```

#### GradientMode (enum)
```yml
Blend
Fixed
PerceptualBlend
```

#### GradientAlphaKey

Gradient alpha keys control the transparency over time. i.e, if you want a smooth transition from opaque to clear, you simply
need to add 2 keys: 
```yml
alpha: 1
time: 0
```
to
```yml
alpha: 0
time: 1
```

```yml
alpha: float              # transparency
time: float               # time stamp
```
#### GradientColorKey

Gradient color key controls the color over time, i.e, if you want a smooth transition from orange to red, you simply need to add 2 keys:
```yml
color: RGBA(1, 0.5, 0, 1)
time: 0
```
to
```yml
color: RGBA(1, 0, 0, 1)
time: 1
```

```yml
color: string            # Hex code or RGBA()
time: float              # time stamp
```

#### CustomDataModule

Some particle effects use CustomData module to control color rather than the main module

```yml
custom1: MinMaxGradient         # See above
custom2: MinMaxGradient         # See above
```

### Material
```yml
name: string                  # name of material, do not change or will not be able to target specific material
shader: string                
color: string                 # Hex code or RGBA(), does not work with Custom/Player shader
hue: float                    # Only works with Custom/Creature shader
saturation: float             # Only works with Custom/Creature shader
value: float                  # Only works with Custom/Creature shader
emissionColor: string         # Hex code
mainTexture: string           # Texture Name
emissionTexture: string       # Texture Name
tintColor: string             # Hex code or RGBA()
materialOverride: string      # Special MDB field, if it has value, when updating, will first search for matching material, and use that instead
glossiness: float             
metallic: float
bumpScale: float
bumpTexture: string           # Texture Name
flowSpeedAll: float           # Custom/Blob shader property
flowSpeedY: float             # Custom/Blob shader property
flowTexture: string           # Custom/Blob shader property, Texture Name
flowColor: string             # Custom/Blob shader property, Hex code or RGBA()
edgeColor: string             # Custom/Blob shader property, Hex code or RGBA()
emissiveColor: string         # Custom/Blob shader property, Hex code or RGBA()
```