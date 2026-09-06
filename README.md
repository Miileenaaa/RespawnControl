# InstantRespawn (UCH Mod)

**InstantRespawn** is a tiny BepInEx mod for *Ultimate Chicken Horse* that allows you to instantly respawn in challenge mode, by either pressing a button or enabling `Instant retry on death`

## Requirements

- [BepInEx 5.x](https://github.com/BepInEx/BepInEx)
- The game **Ultimate Chicken Horse** (shocker)

## How to Use

1. Install BepInEx into your UCH directory
2. Place the built `InstantRespawn.dll` into: `Ultimate Chicken Horse/BepInEx/plugins/`
3. Boot up your game, check the `com.Milena.instantrestart.cfg` file in `Ultimate Chicken Horse/BepInEx/config/` to verify settings
4. Enjoy!

## Features

### Instant Respawn Key

The instant respawn key (configured in the config file - by default is B) lets you instantly respawn in challenge mode upon pressing it, this being a standalone key is important, as it could make levels where you need to hold B for a short time otherwise impossible

### Instant Retry on Death

The instant retry on death is a setting which you can turn on in the config file (disabled by default), with it, it allows you to instantly respawn when you die, you can also configure it to only respawn after a certain amount of time (if set to 0.2 or lower, it may be impossible to get a post mortem clear!)

Special thanks to Ossie, he helped me to install the necessary software for modding, and this mod is built ontop of his mod `BuildUnlimiter` (which is built ontop of `DanceForce` lmao)
Also shoutout to tls for making the `challenge_retry` mod, while I didn't take any code from their mod, our mods are really similar but they did make their's first, and inspired me to make this mod aswell
