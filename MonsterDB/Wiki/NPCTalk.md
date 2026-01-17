# NPCTalk

`NPCTalk` can be added/removed from creature.

NPCTalk defines random text bubbles a creature will display when conditions are met.

```yml
name: string                            
maxRange: float
greetRange: float
byeRange: float
offset: float                           # Height text from head
minTalkInterval: float
hideDialogDelay: float
randomTalkInterval: float
randomTalkChance: float
randomTalk: List<string>
randomTalkInFactionBase: List<string>
randomGreets: List<string>
randomGoodbye: List<string>
privateAreaAlarm: List<string>
aggravated: List<string>
randomTalkFX: EffectList
randomGreetFX: EffectList
randomGoodbyeFX: EffectList
```