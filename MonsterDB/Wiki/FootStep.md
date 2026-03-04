# FootStep

Footstep controls the visual and sound effects triggered when a creature takes a step.

```yml
footlessFootsteps: boolean
footlessTriggerDistance: float
footstepCullDistance: float
effects: List<StepEffect>         # See below
```

### StepEffect

```yml
name: string
motionType: MotionType            # See below
material: GroundMaterial          # See below
effectPrefabs: List<string>
```

#### MotionType (enum)
```
Jog
Run
Sneak
Climbing
Swimming
Land
Walk
```

#### GroundMaterial (enum)
```
None
Default
Water
Stone
Wood
Snow
Mud
Grass
GenericGround
Metal
Tar
Ashlands
Lava
Everything
```

### Note
Effects added to StepEffect will be converted to a `Non-ZNetView` version, if it has a `ZNetView` component.
