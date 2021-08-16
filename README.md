# Brigadier

This is a port of [the original Brigadier](https://github.com/Mojang/brigadier) to C#, and a game implementation of it for Muck.

Currently, the main part of it works (core brigadier library, the bits that aren't related to Muck). There are even tests. All vanilla commands are fully functional as well, in the implementation for Muck.

# Building

If you wanna contribute, you need to have an environment where you can actually test your changes before submitting a pull request.

0) Install dependencies to your game.
    - Install the .NET 4.x mono and unity assemblies to your game in ``libs\``.
        - You can obtain these manually by building a unity 2019.4.19f1 project and copying the contents of ``Data\Managed\`` (except for ``Assembly-CSharp.dll``, of course).
        - This also consists of the ``IndexRange``, ``Microsoft.Bcl.AsyncInterfaces`` and ``System.Threading.Tasks.Extensions`` NuGet packages. Only ``IndexRange`` should actually be required for Brigadier.
        - These are required for indices and ranges (C# 8), and for compiled expressions (``Dawn.ValueString`` uses this to speed up calls to parse methods)
        - You also need to add ``libs\`` to the mono search path in your doorstop config if you plan on actually running the game after building this mod.
    - Install [gg1kommands base](https://muck.thunderstore.io/package/GoldenGuy1000/gg1kommands_base/) to your game. This is a soft dependency, but is required during compilation of course.
    - Install [OffroadPackets](https://muck.thunderstore.io/package/Terrain/OffroadPackets/) to your game. This is required for server-side commands.
        - You can also clone [the source code](https://github.com/Terrain2/OffroadPackets) and then build it. The steps are very much the same as brigadier. (``$ git clone https://github.com/Terrain2/OffroadPackets.git``)
1) Install .NET 6
    - Yes, it's in preview. I don't care, this project uses features from C# 10.
2) Clone this repository (``$ git clone https://github.com/Terrain2/BrigadierMuck.git``)
3) Clone [BetterChat](https://github.com/Terrain2/BetterChat) next to it (``$ git clone https://github.com/Terrain2/BetterChat.git``)
    - This is because brigadier implements everything from BetterChat, so it needs a compile-time reference to define an incompatibility.
    - You don't need to update its ``GameDir.targets`` to build Brigadier, but you should probably do it anyways.
    - Do not install it to your game. It is not compatible with Brigadier.
4) Update ``GameDir.targets`` to your reflect game's install directory.
    - Do not commit this change. It does not need to change in the repository, only on your machine.
    - Sub-projects (``CompatibilityPatcher`` and ``Tests``) reference this same file. You don't have to update this in multiple locations.
5) Run ``dotnet build``
    - The plugin should build fine. If it builds, but spams that ``IndexRange`` is missing at runtime in the console, it means you did not add the .NET 4.x to doorstop's mono search path.
## Testing

To run tests on the core brigadier library, just go to the ``Tests\`` directory, and run ``dotnet test``. Brigadier should be rebuilt in debug mode with the ``TEST`` compile symbol. If it ends up throwing exceptions about ``UnityEngine`` missing, then the ``TEST`` compile symbol was not set. It should've been from the project reference in ``BrigadierTests.csproj``. If this happens, a workaround is to manually set the default configuration in ``Brigadier.csproj`` to ``Test``.

# FAQ (for those who have worked with the original Brigadier)

If you've worked with the java version of Brigadier, you may have some questions about API changes:

## Where did &lt;S&gt; go?

When i tried registering command trees originally, even though i clearly had a ``CommandDispatcher<S>`` with a set generic argument, that could not be inferred through to ``Literal`` and the such methods. Therefore, i removed it. Maybe in the future .NET will be better at inferring type arguments, or maybe i just did something wrong. Either way, it still works for Muck, and that's fine by me.

# (FAQ) Making commands

## Where is ``ArgumentBuilder.executes``? What is ``ExecutesRaw``?

This is a game-specific implemenation detail. The core brigadier here only has ``ExecutesRaw``, but muck-specific extension methods ``ExecutesLocal`` and ``ExecutesServer`` exist. There is only one command tree, and if a command is parsed to a node that executes on the server (``ExecutesServer``), the raw input is sent to the server to be re-parsed and ran there instead. Likewise, ``ExecutesLocal`` will execute locally, and if it was called from a server-side command, an error is shown to the user because of mismatching command trees.

## Where is ``LiteralArgumentBuilder.literal``? Where is ``IntegerArgumentType.integer``?

Such convenience methods are located under a single import: ``using static Brigadier.StaticUtils;``. ``Literal`` and ``Argument`` are there, as well as the static ``Dispatcher`` which is where you register commands. ``StaticUtils.Arguments`` is now available with just ``Arguments``, and under it are methods that are (generally) named the exact same as the type of argument they represent. Therefore it's not a good idea to be ``using static Brigadier.StaticUtils.Arguments;``, but if you wanna shadow existing types, then go right ahead.

# (FAQ) Making custom argument types

## What is ``StringScanner`` and what happened to ``StringReader``?

``StringReader`` already exists in .NET and does not do what brigadier's ``StringReader`` did. ``StringScanner`` is the name brigadier uses for the equivalent class.

## Where is ``StringScanner.ReadInt()``?

``StringScanner`` does not have such a method or an equivalent built in. .NET has a lot of numeric types, and just because i think all of them should be supported, i use a library called [Dawn.ValueString](https://www.nuget.org/packages/Dawn.ValueString) to parse these numbers generically. (it looks for a static ``Parse`` method) The numeric types under ``StaticUtils.Arguments`` all return a ``MinMaxRegexArgument``, which as the name implies, takes a regex and has a min/max value for its parsed type. ``StringScanner.MatchParse`` is the relevant method on ``StringScanner``, and it will match a regex and return the match contents parsed culture-invariantly with ``Dawn.ValueString``.

## What happened to ``Message`` and ``CommandExceptionType``?

I felt they are unnecessary. Just use strings. Localization can be done where you throw the exception, like in the core .NET libraries.

# FAQ (Questions you may have even though you didn't use the original brigadier)

## What is ``StringScanner.ParseStack`` and ``StringScanner.MakeException``?

A common pattern i noticed in just the codebase of brigadier is assigning a local variable the current cursor, and then testing something, and resetting the cursor to that variable before throwing an exception. See [IntegerArgumentType.java](https://github.com/Mojang/brigadier/blob/master/src/main/java/com/mojang/brigadier/arguments/IntegerArgumentType.java#L48-L61).

Because this is such a common occurence, i simplified it with a ``ParseStack`` which is just a stack of cursor positions. When you call ``StringScanner.MakeException``, the ``ParseStack`` is emptied, and a ``CommandSyntaxException`` is created from the scanner with the current cursor position (and optionally, an offset from it), and the cursor is reset to the bottom of the ``ParseStack``. Make sure to always pop your cursor from the ``ParseStack`` before you return a value though, because that isn't implicit. You can also just not use the ``ParseStack``, but if you're writing reusable code, you should make sure to clear the parse stack before you throw an exception.

## What is a ``FinalizingArgument``? Where is ``StringArgumentType.getString``?

A pattern i also noticed in the original brigadier is that an argument type may need to use the command sender to actually produce a final result (i.e. relative coordinates). Minecraft's actual code isn't available freely on the internet, so the best i can do is link to [a relevant line in a project that modifies /tp](https://github.com/RegnorMC/dimenager/blob/1e738d7ecf3ba6eccf399ad6694a14371d36e03b/src/main/java/net/regnormc/dimenager/mixin/TeleportCommandMixin.java#L90) to show you how that is done.

I think that is unnecessary, and when calling ``CommandContext.Argument<T>``, it will accept either an exact type match of ``T``, or a ``FinalizingArgument<T>``, calling ``FinalizingArgument<T>.Finalize`` before returning to the actual command. This makes for nicer command code imo where you don't have to think as much about how each argument exactly works, and same goes for the pattern of a static method like ``StringArgumentType.getString``. That just doesn't exist in my port, and you should instead call ``CommandContext.Argument<string>``.

If you do need to get the actual intermediate result, you can also call ``CommandContext.Argument<Y>(string) where Y : FinalizingArgument<T>``, which will return the intermediate value. You probably wanna constrain that more than just "finalizing argument" which doesn't contain any useful info, perhaps something like ``ctx.Argument<Vector3Argument.FinalizingVector3>("pos")`` in the real world.

If you're making an argument type that will be commonly used like that (messing with the intermediate result and then converting to a final result in the command), you can also just not use a ``FinalizingArgument``.

## Why are there 3 parameters to ``SuggestionsBuilder.Suggest``?

The original brigadier had two different suggestion types: a string, and an ``int``. I thought "Huh, what the fuck?" until i figured out that it's so ``9`` can appear above ``10`` in a suggestions list when sorted. Its original sorting is by string/string or int/int, and if they mismatch, by their string. This is mostly fine (how often do you have mixed type suggestions?), except the fact that there's no well defined sort order.

Keeping with the theme of supporting every .NET numeric type equally, i allow you to call this method generically with any ``T where T : IComparable<T>, IEquatable<T>``, and that just made it even worse, because now you can suggest multiple numeric types. The sorting method that the original brigadier uses for mixed arguments will throw an exception in .NET because it isn't transitive. As a fallback method, it will sort by type and then value of the type.

Okay, all good so far, the first parameter is the sorting key, and it is stringified to actual displayed suggestions. When implementing it for Muck, i realized i wanted multiple kinds of prefixes for numbers (``@`` for steam IDs, ``#`` for item/powerup/mob/player IDs), and i didn't feel like creating a new class was necessary. As such, the second parameter is optional, and contains the displayed suggestion to the player, defaulting to the sorting key as a string.

The third parameter to that method is the tooltip, which has not been implemented yet and is currently just being ignored. When mouse support is implemented, it will appear when you hover your mouse over the argument.

# What is not implemented yet? (TODO)

## Mouse selection & Suggestion tooltips

The actual brigadier suggestions API does already have support for providing a tooltip. That tooltip is not displayed anywhere though, because there is no support for picking suggestions with your mouse as shown in this gif:

![](https://raw.githubusercontent.com/JorelAli/CommandAPI/master/docssrc/src/images/warps.gif)

## API Documentation

A lot of the code is not documented. It would be nice if it was.

## More argument types

There should be more argument types that apply to Muck or Unity in general.

## (Maybe?) Unpatch and convert non-Brigadier commands based on config

Currently, there is partial support for gg1kommands, with autocomplete for commands and a correct number of arguments. A spigot project called CommandAPI adds support for brigadier command trees (spigot does not expose brigadier directly, it still uses the bukkit command api), and it has a really cool feature where it can convert plugins using bukkit's command API to its own with brigadier support. Muck mods that just patch ``ChatBox.ChatCommand`` do not expose enough information to do the [whole plugin automatic conversion](https://commandapi.jorel.dev/6.2.0/conversionforowners.html#example---converting-all-commands-from-essentialsx) that CommandAPI does (that's basically what the gg1kommands interop is), but it also supports [specifying command structure manually](https://commandapi.jorel.dev/6.2.0/conversionforownerssingleargs.html#example---converting-essentialsxs-speed-command), which may be possible to grab from a config file and register in brigadier. Exact syntax of arguments vary heavily between preexisting command plugins though. Especially things such as players, where brigadier supports a player name, a ``#`` then lobby ID, or an ``@`` then a steam ID. How can that be converted to a compatible argument for the original plugin we are converting? Should it only suggest what the originally plugin supports exactly? If so, how could you specify that? And for numbers, what about culture? Some plugins don't think about it and vary between systems, and some are consistent.