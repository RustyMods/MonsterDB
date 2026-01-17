# EffectList

EffectList defines how visual and gameplay effects are spawned into the world when triggered 
(for example: on hit, death, alert, or special abilities).

Each entry describes what prefab to spawn and how it should behave relative to the creature or object that triggered it.

⚠️ Important:
Effect prefabs must contain a ZNetView to be properly networked and disposed.
Prefabs without a ZNetView may leak or behave incorrectly in multiplayer.

```yml
effectPrefabs: List<EffectData>
```

### EffectData
```yml
prefab: string                      # Prefab ID to spawn
variant: int                        # Model variant index (e.g. player models: 0 = male, 1 = female)
attach: boolean                     # Attach effect to the creature
follow: boolean                     # Follow creature movement
inheritParentRotation: boolean      # Inherit parent rotation
inheritParentScale: boolean         # Inherit parent scale
multiplyParentVisualScale: boolean  # Multiply by the creature's visual scale
randomRotation: boolean             # Apply a random rotation when spawned
scale: boolean                      # Scale with creature scale
childTransform: string              # Child transform name to attach to (e.g. "Head", "LeftHand")
```