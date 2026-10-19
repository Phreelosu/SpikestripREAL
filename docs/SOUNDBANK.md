# Rebuilding the Spikestrip soundbank

The original `Spikestrip2.bnk.sound` was built with Wwise 2019.2 (bank version 135). The game now runs Wwise 2024.1 (bank version 150), which won't load it.

The old bank is small. It has **2 events**, and only one of them is used:

| Event | Played by | Source audio | Voice volume | Bus |
|---|---|---|---|---|
| `Play_AragoniteElite_Spawn` | `RagingElite.cs` (Aragonite elite spawn) | `Wwise/source/Play_AragoniteElite_Spawn.wem` | **-9 dB** | Master Audio Bus |
| *(unknown, id 657931668)* | nothing in code or bundles | `Wwise/source/unused_event_657931668.wem` | -7 dB | Master Audio Bus |

The unused event is most likely the pre-1.1.2 version of the spawn sound (the changelog says its volume was reduced). It can be skipped.

## Steps (Wwise 2024.1)

1. Convert `Play_AragoniteElite_Spawn.wem` to `.wav`, for example with vgmstream: `vgmstream-cli -o spawn.wav Play_AragoniteElite_Spawn.wem`.
2. In a Wwise 2024.1 project, import the `.wav` as a Sound SFX and set **Voice Volume to -9**. Leave it on the Master Audio Bus.
3. Create the event **`Play_AragoniteElite_Spawn`** (exact name, since the ID is the FNV hash of the name) with a single *Play* action targeting that sound.
4. Make a soundbank named **`Spikestrip2`** containing that event, and generate it for Windows.
5. Copy the generated `Spikestrip2.bnk` to `Spikestrip2_0/Assets/Spikestrip2.bnk`. It's embedded automatically, and `Main.LoadSoundBank` registers it through R2API's SoundAPI. Don't ship `Init.bnk`, the game provides its own.

To check a bank's version: bytes 8-11 of the file (little-endian u32) should read `150`.

## Tooling

`tools/BundleTool bnk <bank> <outDir> [names.txt]` dumps the `.wem` files and event IDs from any bank, and names the events it can match from a candidate list.
