# Reactive Status Bars

A BepInEx plugin for Dungeon Settlers that dynamically changes health bar colors when health drops below specific thresholds, alongside a flashing alert for critical health states.

<img width="752" height="423" alt="dungeonsettlers-healthbar" src="https://github.com/user-attachments/assets/a12e1d9c-13af-4c06-ba0b-685cf54d1e31" />

## Requirements

* [BepInEx 6 Bleeding Edge, IL2CPP build](https://builds.bepinex.dev/projects/bepinex_be)

## Installation

1. Download `ReactiveStatusBars.zip` from this repo's [Releases](../../releases) page and extract it into your game folder.
2. Launch the game once. A config file will be generated at `BepInEx/config/ReactiveStatusBars.cfg`.

## Configuration

Edit `BepInEx/config/ReactiveStatusBars.cfg` to adjust the settings.

### Thresholds

* **`Low`** (Default `0.50`): Health ratio (0 to 1) below which the Low warning color applies.
* **`Critical`** (Default `0.25`): Health ratio (0 to 1) below which the Critical warning color applies.

### Colors

*Note: Colors are formatted as `R,G,B,A` float values between 0.0 and 1.0.*

* **`Low.Color`** (Default `1, 0.5, 0` - Orange): Color used at the Low health threshold.
* **`Low.BlendStrength`** (Default `0.5`): 0 keeps the original bar color, 1 fully replaces it with the Low color.
* **`Critical.Color`** (Default `1, 0, 0` - Red): Color used at the Critical health threshold.
* **`Critical.BlendStrength`** (Default `0.8`): 0 keeps the original bar color, 1 fully replaces it with the Critical color.

### Flash Effect

Controls the pulsing effect triggered when a status bar enters the Critical threshold.

* **`Enabled`** (Default `true`): Whether bars briefly flash when entering Critical health.
* **`FriendlyOnly`** (Default `true`): If true, only friendly units flash. Enemy severity is still shown via solid color changes.
* **`DurationSeconds`** (Default `1.5`): How long a triggered flash lasts (between 0.1 and 10).
* **`SpeedHz`** (Default `4.0`): Flashes per second while active (between 0.5 and 15).
* **`Intensity`** (Default `0.6`): 0 is invisible, 1 flashes fully to the target FlashColor.
* **`Color`** (Default `1, 1, 1, 1` - White): The specific color the bar flashes toward.
