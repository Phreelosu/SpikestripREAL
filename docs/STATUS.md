# Spikestrip 2.0 revival: status

Goal: one `Spikestrip2_0.dll`, built from the ILSpy'd sources of the 4 original assemblies, running on the current game (Seekers of the Storm + Alloyed Collective), with R2API in place of Groove's homegrown content framework.

## Layout

- `Spikestrip2_0/Main.cs`: the only real plugin. It keeps Groove's GUID `com.groovesalad.GrooveSaladSpikestripContent`. An empty `PlasmaCompatPlugin` keeps `com.plasmacore.PlasmaCoreSpikestripContent`, because ZetAspects and NemesisSpikestrip check for both GUIDs and look up types by their original namespaces.
- `Spikestrip2_0/Core/` (namespace `Spikestrip2_0.Core`): thin Aetherium-style content bases (`ContentBase<T>`, `ItemBase<T>`, `EquipmentBase<T>`, `EliteBase<T>`, `ArtifactBase<T>`, `InteractableBase<T>`, `MonsterBase<T>`) that register straight through R2API. Also:
  - `SpikestripContent`: bundles, config, the Spikestrip expansion, and the content loader.
  - `SpikestripUtil`: misc helpers.
  - `DirectorUtil`: DirectorAPI spawns.
  - `OverlayMaterials` and `BuffBehaviours`: the only two hooks R2API has no module for.
- `Groove/`, `Plasma/`, `Noop/`: content sources, with the **original namespaces kept** (Plasma's bundle binds by namespace and class name, and ZetAspects reflects into content classes). `Groove/GrooveSaladSpikestripContent` now only holds the alt-skill templates and `LockSkillFromExpansion`.
- `Unfinished/`: content that was hard-disabled or cut in the original release (Death/Radiant/Yeah elites, Nullifier Corpse, Void Microbot and leeches, Lantern, Trailer Mode, and the item icon renderer). None of it has its own icons (the elites borrow Warped's). Arsonist's Satchel and its 4 Gasolines had full assets and were ported out of here into `Groove/`. It's excluded from the build with `<Compile Remove>`, like Aetherium does.
- `Assets/`: asset bundles and `Spikestrip2.bnk` (Wwise 2024.1, v150), all embedded. `plasmacorespikestripcontentpack.bundle` is **retargeted** to `Spikestrip2_0.dll` and `R2API.ArtifactCode.dll` (see `tools/BundleTool`).

## Done

### Session 1
- Merged the 4 dlls into 1, with the bundles embedded. The code compiles against current GameLibs, MMHOOK, and R2API, and the hooks are ported to SotS/AC signatures. Details are in git history; highlights:
  - `ItemIcon.SetItemIndex` now hooks the `SetItemIndexArgs` overload, and `SkillIcon.Update` is now `LateUpdate`.
  - `ScreenDamage` now hooks `Update`, `AddDot` hooks the ProcChainMask overload, and `BodyCatalog.Init` became `availability.CallWhenAvailable`.
  - `Row.buttons` is replaced by the `buttonContainerTransform` children.
- AC inventory: `itemStacks` is gone. Calls use `GetItemCountEffective`, `GiveItemPermanent`, and `RemoveItemPermanent`.
- Fixed: Quick Rabbit's Cookie (MysticsItems) exclusion was wiped on load. Removed `NemesisCommand` (hard-disabled, and its hook target is gone).
- "Noop's Wool" is now "<local profile name>'s Wool".

### Session 2
- **Groove's framework is gone.** Deleted:
  - `SpikestripContentBase` and its 27 static content lists, plus `ContentPacks`.
  - `SpikestripEliteRamps` (an IL patch on CharacterModel), `SpikestripDamageColors`, and `SpikestripExpansionDirectorHelper` (which hand-patched every DCCS).
  - `SpikestripContentRunBehaviour`, Groove's `Base` loader, `NoobBase`, and all of GrooveUnsharedUtils.
- What replaces each piece:
  - Content lists became `ContentAddition.*`, and elites became `EliteAPI` + `CustomElite` with the ramp.
  - Tiers resolve lazily through `EliteAPI.GetEliteTierEnumerable`, so UnsharedUtils' tier repair is no longer needed.
  - Damage colors use `ColorsAPI`, temp VFX use `TempVisualEffectAPI`, and monster/interactable spawns use `DirectorAPI`.
  - Lobby toggling still works: spawn prefabs carry a vanilla `ExpansionRequirementComponent`, and elite equipment has `requiredExpansion`.
- GrooveUnsharedUtils folded directly into the content it patched:
  - **Bloody Mask:** the old hook ran `orig` twice; it's now a `RecalculateStatsAPI` bleed bonus.
  - **Imperfect Construct:** the health→shield IL patch replaces the compounding `baseHealthAdd` version.
  - **Blessing:** +3% to all stats, plus regen and armor scaling. The old version compounded armor and crit.
  - **Abyssal Cartridge:** the counter lives on the master, so it no longer resets each stage. The texts are corrected.
  - **Lunar Vault** opens in 90s, **Torpor** gets the literallyeurope icon, and **Potmobile** gets the Dry Basin spawn, smaller scale, walk loop sfx, and 1 dps trail.
- **Fixed:** honor-tier elite variants never got their color ramp (only the main EliteDef did). Elite defs now have names.
- **Quartz Rabbit uses AC temporary items:**
  - Activating grants the doubled stacks with `GiveItemTemp`. After 8s, `RemoveItemTemp` takes back exactly what was granted, so temp items the player already had are kept.
  - The tracker sits on the master's inventory, so it still expires if the body dies.
  - Minions that copy items mid-buff have the doubled stacks removed (via the `AddItemsFrom` hook).
  - The custom "doubled item" UI background is gone, because the game draws temp items natively.
- Soundbank rebuilt by the user (v150, contains `Play_AragoniteElite_Spawn`) and embedded.
- Fixed tooling damage: PowerShell 5.1 had mangled the 「」/… strings in 8 files (it reads BOM-less files as cp1250). They're restored from the decompile. Always read with `-Encoding UTF8` / `ReadAllText(path, UTF8)`.

## Not yet tested in game

Nothing has been launched yet. Highest-risk areas:
- Plasma bundle components, after the assembly retarget.
- `DirectorAPI` stage names for Simulacrum (`it*`) and custom stages.
- The `TempVisualEffectAPI` prefabs.
- Imperfect Construct's IL hook. It logs `Imperfect Construct: RecalculateStats IL hook failed` if the IL no longer matches.

## Next steps

1. **Launch test.** Collect `LogOutput.log` and fix what breaks.
2. Remaining obsolete-API backlog (build with `-p:NoWarn=1701%3B1702` to see it):
   - `CharacterBody.ItemBehavior` should move to `BaseItemBodyBehavior`. Careful: ZetAspects reflects `ItemBehavior.body` on the elite buff behaviours.
   - `GenericPickupController.pickupIndex` should be `pickupState`, and `CreatePickupDroplet(PickupIndex)` should use the `UniquePickup` overload.
   - `PurchaseInteraction.onPurchase` should be `onPurchaseDetailedServer`, and `onCharacterDeathGlobal` should be `onCharacterDeathServer`.
   - `StatHookEventArgs.cooldown*` fields have moved under `allSkills`. Asset `*Sprite`/`*Prefab` fields have become `*Reference`.
3. `manifest.json`, README, and CHANGELOG still have template content. They need the Thunderstore dependencies: BepInExPack, HookGenPatcher, and the R2API submodules (ArtifactCode, Colors, CommandHelper, ContentManagement, Core, Deployable, Director, Dot, Elites, Items, Language, Networking, Orb, Prefab, RecalculateStats, Sound, TempVisualEffect, Unlockable).


yes the summary to help my progress was made by ai, cry about it