# Express Bus Services
Unlock the peak efficiency of buses:

1. Buses skips stops whenever possible
2. Buses do not unbunch (except when at first stop of line)

# Mod Status

- Compatible with Transport Line Manager (natural compatibility)
- Compatible with Improved Public Transport 2 (implemented compatibility; IPT2 would override things otherwise)

Because this mod is very small and simple, I am open-sourcing this to everyone out there.

# Motivation

This mod aims to salvage vanilla CSL buses from being borderline unusable to being competitive against metro and tram lines.

By skipping stops and removing most of the unbunching, buses can now become an efficient alternative to metro and tram lines when e.g. the streets and curves are too tight for metro stations, or when conversion to tram lines are not possible due to street layouts.

Combined with mods such as Transport Line Manager and Improved Public Transport 2, it should be much more likely to earn profits per bus line. (Upkeep of depots should be covered by some other, stronger income source.)

# Technical Information

This mod uses Harmony to modify ("patch") the CSL code to achieve the effect.

The integrated compatibility to IPT2 is achieved by utilizing Harmony's ability to detect other mods in runtime and selectively apply patches. See the source code for more details.

As pointed out in #1, the general procedure of this mod is something like this:

0. General event occurs: bus arrives at bus stop
1. Bus AI tries to unload passengers
2. I count how many is unloaded
3. Bus AI tries to load passengers
4. I count how many is loaded
5. Enter loop:
6. If non-first bus stop & loaded + unloaded = 0 then depart immediately, else (other waiting conditions)

No data is written to the save-game because instant-departures are, well, instant. It is very unlikely that the arrival state persists at the moment a save occurs. (Well, unbunching right after loading a save game shouldn't be too severe.) Moreover, I can determine whether the bus is at the first bus stop of the line just from vanilla CSL data structures. There is no need to save any data.
