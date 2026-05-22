# Miniscript

## Design

Here is an AI list of things the language should be able to do: 

```
check conditions ✔

change NPC property values ✔
jump to nodes ✔
play sounds ✔
give/drop items ✔
end conversation ✔

set flags (??)
call known gameplay actions (??)
```




## Special Variables

`$TARGET` - The target NPC or player character.



## Example 1

```asm
push $TARGET
push "comfort"
push  -0.1
echo $TARGET
call "adjust_npc_attribute"
```

## Example 2

``asm
set $counter 0

Conversation
    DialogeNode1
    - DialogNode1.1
    - DialogNode1.2 (Disrespectful)

    - DialogNode1.1.1
    - DialogNode1.1.2
    - DialogNode1.1.3 

    - DialogNode1.2.1
    - DialogNode1.2.2
