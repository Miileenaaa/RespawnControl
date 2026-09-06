# RespawnControl (UCH Mod)

**RespawnControl** is a BepInEx mod for *Ultimate Chicken Horse* that aims to let the user have the most control as possible over their respawns, and remove the most annoying aspects

## Requirements

- [BepInEx 5.x](https://github.com/BepInEx/BepInEx)
- The game **Ultimate Chicken Horse** (shocker)

## Why use this over any other respawn mod?

The biggest problems with the current respawn mods is that they make you respawn fast in every gamemode, like party, which is unideal as it has made players accidently end a round early due to it, and they also keep bringing an annoying scoreboard popup when you die in challenge mode, this mod fixes all of that and does even more

# Features

## Challenge Mode

### Instant Respawn Key

The instant respawn key (configured in the config file - by default is B) lets you instantly respawn in challenge mode upon pressing it

This being a standalone key is important, as it could make levels where you need to hold B for a short time otherwise impossible

### Instant Retry on Death

The instant retry on death is a setting which you can turn on in the config file (disabled by default), with it, it allows you to instantly respawn when you die (no scoreboard popup!)

### "No Auto Retry after" & "No Accidental Instant Respawn"

These 2 features lets you prevent the mod from respawning you too quick, or if you accidently press the instant respawn key when you're in a really good run

## Freeplay Mode

### Freeplay Respawn Key

The freeplay respawn key (disabled by default) lets you respawn at anytime in freeplay

### Freeplay Respawn Time

The freeplay respawn time (0.5s by default) lets you change how fast you respawn in freeplay, without affecting other gamemodes like party

## How to Use

1. Install BepInEx into your UCH directory
2. Place the built `RespawnControl.dll` into: `Ultimate Chicken Horse/BepInEx/plugins/`
3. Boot up your game, check the `com.Milena.RespawnControl.cfg` file in `Ultimate Chicken Horse/BepInEx/config/` to verify settings
4. Enjoy!

## Special Thanks

Special thanks to Ossie, he helped me to install the necessary software for modding, and this mod is built ontop of his mod `BuildUnlimiter` (which is built ontop of `DanceForce` lmao)

Also shoutout to tls for making the `challenge_retry` mod, while I didn't take any code from their mod, our mods are simillar in the challenge respawning area, but they did make their's first, and inspired me to make this whole mod aswell
