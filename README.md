# Express Bus Services
Unlock the peak efficiency of buses:

1. Buses skip stops whenever possible
2. Buses do not unbunch (except when at first stop of line)
3. Niche: can choose to allow buses self-adjust their service levels to better meet commuting demand
4. Niche: can choose to allow minibuses (ie, buses where capacity <= 20) to depart faster when pax boarding+alighting <= 5
5. Special extension: trams can be configured to not unbunch at non-terminus stops (Light Rail mode and True Tram mode)

# Mod Status

[img]https://i.imgur.com/O3ujMsj.png[/img]

- Compatible with Transport Line Manager (implemented compatibility; TLM can work more completely)
- ~~Compatible with Improved Public Transport 2 (implemented compatibility; IPT2 would override things otherwise)~~
- Mods available for this mod (yes, mod-ception):
  - Express Bus Services (IPT2 Plug-In) https://github.com/Vectorial1024/ExpressBusServices_IPT2
  - Express Bus Services (TLM Plug-In) https://github.com/Vectorial1024/ExpressBusServices_TLM
- Special thanks to Klyte45 from TLM for letting me implement their "Express Bus" mode code on this side

Because this mod is very small and simple, I am open-sourcing this to everyone out there.

## Types of Transportation Covered

These transportation types are currently covered by this mod:

- Buses; as a result of game mechanics, the following transportation types are also covered:
  - Evacuation buses
  - Tourism buses
- Trolleybuses

Note: Intercity buses were previously affected by this mod, which does not make much sense and might have caused some other problems (https://github.com/Vectorial1024/ExpressBusServices/issues/15#issuecomment-1187672211), and so they are now free and are not affected by this mod.

These transportation types are covered by this mod, but contains a different set of settings:

- Trams

These transportation types will NEVER be covered by this mod because it will not make sense:

- Cruisers
- Airplanes
- Cable cars
- Ferries

These transpotation types will not be covered by this mod because they will become unrealistic:

- Metros
- Trains
- Blimps
- Monorails
- Helicopters

## Modes of Stop-Skipping

- Prudential (Legacy): buses etc still stop at stops, but will depart immediately when the stop-skipping criteria is met
- Aggressive: buses etc will try to predict whether it needs to stop at non-terminus stops, and will literally skip the stop if the prediction passed. Not available for trams.
- Experimental: a stronger version of Aggressive Mode. WIP.

### Trams-only: Tram Mode

- Disabled: trams behave according to "default behavior" (eg if you have TLM, it uses TLM behavior)
- Light Rail: trams stop fully at stops, but do not unbunch at non-terminus stops
- True Tram: equivalent to Prudential mode as described above

## Extra Stop-Skipping Options: Service Level Self-Balancing

When activated, buses will have a chance to immediately transfer themselves to the most in-demand section of the bus line, resuming from the relevant first-stop terminus, thereby skipping multiple stops at once (refer to https://github.com/Vectorial1024/ExpressBusServices/issues/12 for more info); this will be helpful when commuting patterns are asymmetric.

When attempting to transfer over a long distance (currently set to 1 city tile), buses will send themselves back to depot, and let (another) depot spawn a replacement bus. This reduces the time that the bus "arrives" at the intended transfer target, and speeds up the transfer.

## Extra Niche Options: Minibus Mode

Minibuses are defined as buses where capacity is `<= 20`.

When activated, minibuses will pay attention to the number of passengers boarding and alighting at the bus stop: if that number is `<= 5`, then the minibuses may wait less and depart earlier than usual, simulating the Hong Kong minibus situation.

## Extra QOL Behavior: Improved Unbunching

Buses will now unbunch based on the number of vehicles on the line. This results in generally higher effectiveness of high-frequency buses, while also maintaining reasonable spacing for low-frequency buses. (Note: due to tech limitations, there is a max amount of time that buses can wait at termini, and I cannot influence that; so low-frequency buses cannot be too low-frequency if you are aiming for an even distribution of buses in the line.) 

# Motivation

This mod aims to salvage vanilla CSL buses from being borderline unusable to being competitive against metro and tram lines.

By skipping stops and removing most of the unbunching, buses can now become an efficient alternative to metro and tram lines when e.g. the streets and curves are too tight for metro stations, or when conversion to tram lines are not possible due to street layouts.

Combined with mods such as Transport Line Manager and Improved Public Transport 2, it should be much more likely to earn profits per bus line. (Upkeep of depots should be covered by some other, stronger income source.)

# Technical Information

This mod uses Harmony to modify ("patch") the CSL code to achieve the effect.

The integrated compatibility to IPT2 is achieved by utilizing Harmony's ability to detect other mods in runtime and selectively apply patches. See the source code for more details.

The ability for this mod to determine whether a certain vehicle has loaded all loadable CIMs is powered by Harmony's "reverse-patching" feature available since Harmony v2. See the source code for a working example of reverse-patching.

Expanding on my answer on #1, the general procedure of this mod is something like this:

0. General event occurs: bus arrives at bus stop
1. Bus AI tries to unload passengers
2. I count how many is unloaded
3. Bus AI tries to load passengers
4. I count how many is loaded
5. Enter loop:
6. If non-first bus stop:
   1. If unloaded + loaded = 0 then depart immediately, exit loop
   2. If some CIMs boarded/alighted & waited enough time at stop & all boarding CIMs boarded then depart immediately, exit loop
   3. Else wait for next loop
999. Return control to game

No data is written to the save-game because instant-departures are, well, instant. It is very unlikely that the arrival state persists at the moment a save occurs. (Well, unbunching right after loading a save game shouldn't be too severe.) Moreover, I can determine whether the bus is at the first bus stop of the line just from vanilla CSL data structures. There is no need to save any data.
