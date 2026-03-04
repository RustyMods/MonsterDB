# Item

```yml
ItemData: ItemData                  # See ItemData.md
Visuals: Visuals                    # See Visuals.md
Recipe: Recipe                      # See below
Conversions: List<ItemConversion>   # See below
```

Fish & Egg inherit from Item, meaning they have access to the above fields.

### Recipe

```yml
item: string                          # Prefab ID
amount: integer                       
enabled: boolean
qualityResultAmountMultiplier: float
listSortWeight: integer
craftingStation: string
repairStation: string
minStationLevel: integer
requireOnlyOneIngredient: boolean
resources: List<Requirement>          # See below
```

### Requirement

```yml
resItem: string                         # Item Prefab ID
amount: integer                         
extraAmountOnlyOneIngredient: integer
amountPerLevel: integer                 
recover: boolean
```

### ItemConversion

```yml
prefab: string                          # Target conversion prefab (i.e fermenter)
type: ConversionType                    # See below
from: string                            # Item Prefab ID
to: string                              # Item Prefab ID
producedItems: integer                  # Amount produced (fermenter)
cookTime: float                         # Time to cook (cooking station)
```

### ConversionType

```yml
Fermenter                                # Fields: from, to, producedItems
CookingStation                           # Fields: from, to, cookTime
Smelter                                  # Fields: from, to
```