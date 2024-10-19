# SpinCore

Library and utility mod for Spin Rhythm XD.

## Features

- Custom panels and nested mod settings pages
- Custom UI components on tabs and pages
- Custom UI components on modal dialogs
- Translations
- Custom chart triggers
- A collection of utility functions

## How to use

In your mod, add a reference to the mod (SpinCore.dll) and add a dependency to SpinCore to your mod like so:
```diff
+using SpinCore;
 ...

 [BepInPlugin("srxd.johndoe.mycoolplugin", "My Cool Plugin", "0.1.0"]
+[BepInDependency(SpinCorePlugin.Guid, SpinCorePlugin.Version)]
 internal class MyCoolPlugin : BaseUnityPlugin
 ...
```
This line will make sure the mod loads **only if** SpinCore is loaded with the given version string.

If your mod has optional SpinCore support, you will need to manually enter the guid (?) and declare your plugin a soft dependency.

From there on, you can start using SpinCore in your mod. If you need a reference on how to use the mod, check out this repo's [TestMod](SpinCore.TestMod).

## License

This mod is licensed under the [MIT License](LICENSE).
