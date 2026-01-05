# Visuals

Visuals targets different parts of the prefab. 
The top level transform which controls the scale, and the internal body which holds references to the materials.
`PlayerVisual` inherits from `Visual` with extra fields.

```yml
Visuals:
  scale: Scale                      # See below
  levelSetups: List<LevelSetup>     # See below
  materials: List<Material>         # See below
```

### Scale
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
emissiveColor: string         # Hex code
enableObject: string          # Child Transform Name
```

### Material
```yml
name: string                  # name of material, do not change or will not be able to target specific material
shader: string                
color: string                 # Hex code, does not work with Custom/Player shader
hue: float                    # Only works with Custom/Creature shader
saturation: float             # Only works with Custom/Creature shader
value: float                  # Only works with Custom/Creature shader
emissionColor: string         # Hex code
mainTexture: string           # Texture Name
```

### PlayerVisual

`PlayerVisual` inherits from `Visual` with extra fields:

```yml
modelIndex: List<int>           # 0 = male, 1 = female
beards: List<string>            # Prefab IDs
hairs: List<string>             # Prefab IDs
hairColors: List<string>        # Hex codes
skinColors: List<string>        # Hex codes
```