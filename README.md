## ![](https://cdn.discordapp.com/attachments/965315944966668288/965346898066157638/title.png)

A content mod from SpikestripModding. Contains new items, enemies, elites, and more. To discuss the mod or share feedback/bug reports, find us here: https://discord.gg/me7P53smzk

Every piece of content can be turned on or off in the `Spikestrip 2.0.cfg` config file. Everything is enabled by default.

## ![](https://cdn.discordapp.com/attachments/965315944966668288/1060709310532948068/updateditems.png)

<details>
<summary>Items</summary>

### Common Items

| Item | Description |
| ----------- | ----------- |
| Pewter Bracelet | Boost your next 'Critical Strike' with each hit. |
| Groove's Alarm | Gain a bonus according to the day of the week. |
| (Your profile name)'s Wool | Gain armor for every nearby enemy. |

### Uncommon Items

| Item | Description |
| ----------- | ----------- |
| Arsonist's Satchel | Gain random 'Gasoline' items at the start of each stage. |
| Bloody Surgical Mask | Gain a stacking chance to bleed enemies during combat. Taking damage removes all stacks and puts the item on cooldown. |
| Smouldering Comet | Recruit a friendly Blue Lemurian that drops Lunar Coins on kills. |
| Sunrise Smoothie | Heal on skill use. |

### Legendary Items

| Item | Description |
| ----------- | ----------- |
| Blood Orange | Damage-over-time effects activate your 'On Hit' items. High damage hits also inflict damage-over-time. |
| Infinity Codex | Deal extra damage to enemies in your Monster Log Database. |
| Perma-Spike Factory | Produce near-permanent spike piles for massive damage. |
| Sun Blade | The first attack ignites enemies for massive damage. Afterwards, hits also ignite. |

### Lunar Items

| Item | Description |
| ----------- | ----------- |
| Imperfect Construct | Recharge shield by attacking enemies... BUT replace a portion of your health with shield that cannot recharge on its own. |
| Box of Agony | Reduce incoming damage... BUT your health is hidden from view. |
| Cosmic Bulwark | Gain random items on the next stage... BUT the environment is rendered hostile and barren. |

### Void Items

| Item | Description |
| ----------- | ----------- |
| Abyssal Cartridge | Prevent a small amount of debuffs before breaking. Each debuff prevented has a chance to bless ALL of your stats. Corrupts all Medkits. |

### Gasolines

Granted at random by the Arsonist's Satchel at the start of each stage.

| Item | Description |
| ----------- | ----------- |
| Chilled CO2 | Chance on killing an enemy to freeze other nearby enemies. |
| Coagulated Mixture | Killing an enemy slows down other nearby enemies. |
| Essential Oils | Killing an enemy regenerates nearby allies. |
| Pressurized Fluid | Chance on killing an enemy to pull together other nearby enemies. |

### Equipment

| Equipment | Cooldown | Description |
| ----------- | ----------- | ----------- |
| Quartz Rabbit | 100s | Double your items for a short time. |
| Courier Sphere | 30s | Unleash the Courier Sphere on all monsters in view. |
| Impulse Frost Shield | 30s | Interrupts an enemy as it readies an attack, freezing it and causing it to take max HP damage. |

</details>

## ![](https://cdn.discordapp.com/attachments/965315944966668288/1060710775611723796/updatedthreats.png)

<details>
<summary>Monsters, Elites and Interactables</summary>

### Monsters

| Monster | Description |
| ----------- | ----------- |
| Sigma Construct | A sturdy construct that opens up to fire a sweeping beam. Spawns from stage 4 onwards. |
| Lively Pot | A pot on wheels that leaves a trail of tar behind it. |

### Elites

| Elite | Description |
| ----------- | ----------- |
| Veiled | Invisible and fast for 20 seconds after spawning, and cloak again after 10 seconds out of combat. Two hits knock them out of cloak. |
| Aragonite | Create explosions on their attackers, buff nearby allies, and call down lightning strikes when they die. |
| Plated | Protected by plating, and their hits weaken your attacks. |
| Warped | Their hits warp your gravity. |

### Interactables

| Interactable | Description |
| ----------- | ----------- |
| Lunar Vault | Spend time charging the vault to open it and claim a Lunar Coin. |
| Cloaked Shrine | Offer to the shrine for a hidden reward, which can be a Void or Lunar item. |
| Shrine of Tribulation | A shrine of chance with a single attempt and a very low chance of success, which pays out a legendary item. |

</details>

## ![](https://cdn.discordapp.com/attachments/965315944966668288/1060709310843330652/updatedskills.png)

<details>
<summary>Skills and Artifacts</summary>

### Skills

| Survivor | Skill | Description |
| ----------- | ----------- | ----------- |
| Commando | 「Weave」 | Entangling. Rapidly shoot an enemy for 50% damage. |
| Huntress | 「Network」 | Enter the Void, draining your health over time. Attacks integrate enemies. |
| Artificer | 「Choke」 | Crushing. Create a Binding Maelstrom that damages enemies, then mark enemies to be pulled into it. |
| REX | Petal Storm | Quickly launch piercing petals at the cost of health. |
| Loader | 「Conduit」 | Charge a beam that damages enemies, then finish with a destructive blast. |
| Acrid | 「Deeprot」 | Attacks that apply Poison apply stacking Deeprot instead, slowing enemies. At 5 stacks, the enemy is afflicted with Soulrot. |
| Captain | EMP Charge | Shocking. Aim a grenade that applies heavy force to ALL characters. |
| Railgunner | 「Torpor」 | Dormant. Awaken slumbering energy, transforming your weapon into a piercing 8x200% damage plasma shotgun. |

### Artifacts

| Artifact | Description |
| ----------- | ----------- |
| Spread | Projectiles are split into weaker fragments. |
| Wrath | Calls down fury from the sky. |

</details>

## Credits

* Main Team - Groove_Salad, PlasmaCore, Heyimnoop, literallyeurope
* Additional Models - Dotflare, 2cute2game, Gummies, SkeletorChampion
* Lore Assistance - swuff★, Dexterity
* Old Elite Ramp Implementation - Nebby
* Early Testing - literallyeurope, SteamStream, swuff★
 & more
* Some Concepts From - Xanderpitz
* Made Possible Through - Hopoo the Game

## Changelog
**1.2.2**
* Fixed Lunar Vaults flooding stages that have no chests, like Hidden Realm: Soulbound Threshold. Spikestrip's interactables were being added to every stage, and on stages without a matching category a new category was created that held only Spikestrip's interactable. They are now only added to stages that already have that kind of interactable.
* Fixed the Blue Lemurian from Smouldering Comet being invisible, T-posing and not blue
* All required R2API modules are now listed as dependencies
* The README now lists all of the mod's content

**1.2.1**
* No more embedded resources
* Re-added Arsonist's Satchel as its working fine

**1.2.0**
* Rewrote the mod from the ground up
* Singular dll
* Fixed Sunblade on hit error
* Fixed up Torpor
* Fixed Conduit
* Fixed Choke
* Got rid of Groove's fake msu and swapped everything to R2API instead

**1.1.6**
* Fixed Sigma Construct sometimes having its hitbox disabled on clients
* Buffed Sigma Construct damage
* Nerfed Sigma Construct health

**1.1.5**
* Fixed Torpor losing damage per bullet
* Reduced Torpor damage to 8x200
* Fixed Torpor to work with XQR Targeting from Free Item Friday

**1.1.4**
* Fixed clients sometimes crashing due to infinite loop in Veiled elite code
* Updated Choke description to match new functionality
* Fxed void bolts sometimes colliding with the player who cast them
* Fixed choke sometimes restoring its stock after void bolts are cast

**1.1.3**
* Choke rework
* Torpor proc coeficcient: .9 => 1.0
* Torpor bullet count: 6 => 8
* Torpor reload window randomization reduced
* Fixed static variable in Sigma Beam cuasing weird timing effects when multiple are active at once
* Sigma Constructs now forced into the open state when attacking
* Cloaked elites now re-cloak when out of combat for 10s
* Cloaked elites can be knocked out of cloak by hitting them twice
* Cloaked elites no longer hide their projectiles and tracers
* Aragonite elites now create exlossions on their attackers instead of the other way around
* Tweaked Aragonite death explossion spawns
* Aragonite buff ward size is now changes based on how much it has been hit
* Added team-based area indicators for aragonite range indicators
* Fixed Weave bouncing to certain enemies multiple times

### 1.1.2

* Added a new primary for Commando as his void skill
* Reduced volume of Aragonite elite spawn SFX
* Increased chance of cloaked shrine dropping void or Lunar items and reduced change of dropping white items
* Removed Elder Lemurian SFX from Sunblade
* Fixed visual bug when hitting teammates while having Sunblade
* Sunblade now has a 7s cooldown before it can be activated again
* Halved damage of Sunblade burn when upgraded by Ignition Tank
* Removed Deeprot effect while Soulrot is active to help with performance
* Slightly increased Sigma Construct damage 

### 1.1.1

* Probably fixed Torpor reload bug

### 1.1.0

* All Elites
  * Elite ramps are now added through EliteAPI (should no longer show up as blazing elites)
* Sunrise Smoothie
  * Fixed smoothie activating when scoping in as Railgunner
* Abyssal Cartridge
  * Fixed Blessings giving only 0.3% crit instead of the intended 3%
* Sigma Construct
  * Added support for spawning on Forgotten Haven (untested)
  * Added support for spawning on Simulacrum Sky Meadows (untested)
* Lively Pot
  * Added support for spawning on Dry Basin (untested)

</br>*Some of Spikestrip's content has been marked as legacy, and is no longer enabled by default.* </br>
</br>
"I had fun working on Spikestrip, but it has been stressful to maintain for many reasons.. I don't plan on supporting my portion of Spikestrip's content in the future, so it has been marked as legacy and will be disabled for new users. If you want to re-enable it, check the Spikestrip 2.0 Legacy config, but do so at your own risk!"  - Groove

### 1.0.9

* Noop's Wool
	* Fixed an issue where the armor would overwrite other mods
* Imperfect Construct
	* Replaced the icon of construct

### 1.0.8

* Choke
	* Fixed particle effects not ending properly
* Sigma Construct
	* Fixed hurtboxes being disabled

### 1.0.7

* Deeprot
	* Now slows move and attack speed by 10% per stacking
	* Now lasts longer: 7s => 12s
	* Stacks to apply Soulrot: 6 => 5
* Choke
	* Fixed bad override priority causing all stacks to be consumed at once

### 1.0.6

* Rewrite to no longer depend on proper file structure. Sorry for all the updates!

### 1.0.5

* Attempting to fix load issues caused by file structure

### 1.0.4

* Bloody Surgical Mask
  * Fixed cooldown buff preventing the player's stats from recalculating, often preventing the player from getting the sprinting speed boost
* Cloaked Shrine
  * Fixed max spawns per stage being set to 0
* Sigma Construct
  * Fixed warning spam on clients
  * Fixed eviscerate not properly targeting their hurtboxes
  * Fixed weakpoints being non-functional
  * Fixed hurtboxes sometimes disappearing for clients
  * Added support for spawning on Fogbound Lagoon
* Imperfect Construct
  * Fixed health to shield conversion rarely causing deaths/jank
* Abyssal Cartridge
  * Fixed cleansed debuffs count resetting between stages
  * Blessing stat up 5% -> 3%
  * Fixed Blessings affecting some stats in unexpected ways
* Veiled Elites
  * Invisibility on hit -> invisibility and speed for 20 seconds after spawn
* Aragonite Elites
  * Lightning strike projectiles now use team area indicators
  * Lightning strikes on death spawn in staggered waves
  * Lightning strike on death count 15 -> 24
  * Increased lightning strike on death spawn radius
  * Fixed spawning pre-loop with Aetherium
* Lunar Vault
  * Opening time 120s -> 90s
* Torpor
  * New skill icon courtesy of literallyeurope
* Potmobile
  * Increased size
  * Trail dps 150% -> 100%
  * Reduced intensity of sfx
* Removed Spikestrip 2.0

### 1.0.3
* Fixed Readme

### 1.0.2
* New Item: Bloody Surgical mask
* New Skill: Deeprot
* New Elites: Veiled 
* Removed Spikestrip 1.0 from the industrial sabotage mission type 
* Fixed Various Bugs

### 1.0.1
* New Item: Abyssal Cartridge
* New item: Pewter Bracelet
* Sigma Construct
  * No longer spawns before stage 4
  * Beam Range increased by 5m
  * Beam fire rate doubled
  * Beam properly networked
  * Improved AI
  * Can be properly targeted by auto-aim abilities
* Lively Pot
  * Can be properly stunned, shocked, and frozen
  * Fixed various issues with death, notably corpses persisting when they clearly shouldn’t
  * Tar trail duration 12s -> 9s
  * Slightly improved tar trail visuals
  * Reduced Intensity of sfx
* Sun Blade
  * Burn damage reduced
  * Description updated to include stacking
  * No longer stacks damage on the initial hit
* Quartz Rabbit
  * New logbook courtesy of Dexterity
* Impulse Frost Shield
  * Fixed latency compensation
* Infinity Codex
  * Slightly fancier vfx
* Box of Agony
  * Now properly provides armor
* Smouldering Comet
  * Blue Lemurian is now 100% more blue!
* Lunar Vault
  * No longer spawns when sacrifice artifact is enabled
* Torpor
  * Fixed dormant applying to other railgunner secondaries
  * Reduced randomization of the reload bar by 0.1s
* Deeprot
  * New icon by Plasma
  * Soulrot duration increased
* Conduit
  * Beam dps reduced 600% -> 500%
* Removed Item: Spikestrip

### 1.0.0
* Initial Release
  * Removed Spikestrip 1.0
