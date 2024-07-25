# SpinCore

Library and utility mod for Spin Rhythm XD.

## Features

- Custom panels and nested mod settings pages
- Custom UI components on tabs and pages
- Custom UI components on modal dialogs
- Translations
- Custom chart triggers

## How to use

In your mod, add a reference to the mod (SpinCore.dll), and add the following line in your plugin definition:
```csharp
[BepInDependency("srxd.raoul1808.spincore", BepInDependency.DependencyFlags.HardDependency)]
```

If your mod has optional SpinCore support, change the definition above to a `SoftDependency`.

From there on, you can start using SpinCore in your mod. If you need a reference on how to use the mod, check out this repo's [TestMod](SpinCore.TestMod).

## License

This mod is licensed under the [MIT License](LICENSE).
