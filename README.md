# Overview

- Mark all the resources you and your teammates have found.Displays the name, capacity, distance, and exclusive icon of the resource on your screen.

- Package resources can be displayed at a long distance, other items can only be displayed at a close distance. But when someone marks something with the Middle Mouse Button, all the surrounding resources can be displayed at a very long distance.

- You can set the display name of resources in `Localia.ResourceHelper-Plus.cfg`.


# For Devs

You can add extar icons(.png) for ResourceHelper in `/Your MTFO Rundown Folder/Custom/ExtraResourceHelper/`

It's a simple way to configure by "file name".

The file name consists of 9 parameters, and separated by ",":

	For Example：MyCustomRundown,Gold Bulkhead Key,146,GOLD_KEY,0,40,1.0,1.0,0.0.png

	Parameters： rundown_name,tag_name,item_id,show_name,ammo_divisor,fade_distance,R,G,B
	


#### 1.rundown_name
- The name of your rundown.
- Players will not see it in the game.
- Be used to generate the config in `Localia.ResourceHelper-Plus.cfg`
- Do not use "ResourceHelper" as rundown_name, which will mess up the sorting of CFG.

#### 2.tag_name
- The internal name of your custom item.
- Players will not see it in the game.
- Be used to generate the config in `Localia.ResourceHelper-Plus.cfg`.

#### 3.item_id
- The item's persistentID in ItemDataBlock.
- Cannot use duplicate ID. (Including the items already provided by ResHelper)

#### 4.show_name
- The name displayed on the icon in the game.
- On the first time it is loaded, this name will be saved to cfg as the default name.

#### 5.ammo_divisor(0~100)
- Divisor of item capacity.
- Determines the quantity calculation displayed after the display name like ""x 2".
- Resource packs are usually set to 20 and other consumables to 1.
- When it is 0, it means that the item has no stacking quantity ( such as flashlight ).
- If the value is wrong, it will show a incorrect quantity on icon.

#### 6.fade_distance(1~60)
- Maximum distance the icon can display.
- Resource packs are usually set to 40 and other consumables to 10.

#### 7~9.RGB(0.0~1.0)
- Color of the icon.
- Values between 0.0~1.0 , not 0~255.


*You can access and refer to the original PNG files of ResHelper, which are the best templates*.



# Credits

Thanks Flowaria for the code to loads the additional icon (from ExtraEnemyCustomization).


# Note

Contact [Localia#5666 on the modding server](https://discord.gg/FRdArrB5w8) if you want to give feedback or have found any issue.


============ Change Log ============

v2.0.3
 - Fixed file path.

v2.0.2
 - Fixed an issue that could not load properly in some countries.

v2.0.0
 - Rewrite most of the code, support ExtarResourceHelper.
 - Added a settable function of auto fade icon out when ADS.
 - Cfg is renamed as `Localia.ResourceHelper-Plus.cfg`.

v1.8.8
 - Fixed file path.

v1.8.6
 - Re-complied for bepinex2.0.
 - Remove dependency on "PingEverything".

v1.8.5
 - Add support for red/orange glowsticks.
 - Add a fade-in effect.
 
v1.8.1
 - Support for checkpoint.
 - Optimized code.

v1.6.5
 - Fixed bug in ammo reading, now compatible with mod "Stacks".

v1.6.0
 - Improve compatibility with other mods.
 
v1.5.1
 - Optimized code and improved performance.

v1.5.0
 - Optimized zoom display of long-distance resources.
 - Optimized the experience of marking resources with the middle mouse button, and can be displayed at a longer distance after marking.

v1.1.0
 - Fix the problem of invalidation after changing the level.
 
v1.0.0
 - Initial Release
