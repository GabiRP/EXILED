# Frequently Asked Questions

## What is EXILED?
EXILED - short for "EXtended In-runtime Library for External Development" is a plugin framework for SCP: Secret Laboratory compatible with MP2. It is not a direct replacement for SMOD, but it's purpose is to implement an event-based framework using Harmony patches, that plugin developers can utilize to develop plugins for SCP:SL servers.

## How do I install EXILED?
See the [](xref:install) page for installation information.

## How do I install plugins?
All plugins contain a DLL file (found in its latest release) that is used to load the plugin. Place the plugin in the appropriate plugin folder.
- Windows: `%AppData%\EXILED\Configs(ServerPortHere)-configs.yml`
- Linux: `~/.config/EXILED/Configs(ServerPortHere)-configs.yml`

## Where is plugin configuration stored?
Plugin configuration is stored in a separate folder than the base-game config files.
- Windows: `%AppData%\EXILED\Plugins`
- Linux: `~/.config/EXILED/Plugins`

## Is there a plugin for upgrading items in hand, inside SCP-914?
No, this is unnecessary because this is a base-game feature! Simply set the `914_mode` config_gameplay config to `DroppedAndHeld`.

## What is Harmony?
Harmony is a library that examines the code of a program as it is being run, allowing developers to tap into those functions, and run their own code, either adding onto, or completely replacing, the code the program would normally run.


The reason EXILED uses harmony is to allow easier updating of the framework in conjunction with game updates. Under ideal circumstances, a new game update will not break EXILED itself, and the only thing needed to make EXILED work again, is a very simple copy/paste of a few lines of code into the new Assembly-CSharp file. 


By keeping all of our code outside of the Assembly, other developers can have full, unhindered access to the entirety of EXILED's source code, making collaboration easier.  <br />
Additionally, it means that our code will be mostly unaffected by game updates. Unless the game drastically changes code in a very specific function EXILED uses for an event patch, a game update may not even require EXILED itself to also be updated.