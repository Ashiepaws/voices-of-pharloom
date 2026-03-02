using BepInEx.Configuration;

namespace VoicesOfPharloom;

public class VoPConfig
{
    private static ConfigEntry<bool> _ShowDialogueKeys;
    public static bool ShowDialogueKeys => _ShowDialogueKeys.Value;

    private static ConfigEntry<bool> _DumpAllText;
    public static bool DumpAllText => _DumpAllText.Value;

    public static void Init(ConfigFile config)
    {
        _ShowDialogueKeys = config.Bind("General", "Show Dialogue Keys", false, "Shows the current dialogue key whenever a dialogue box is opened.");
        _DumpAllText = config.Bind("General", "Dump All Text", false, "Dumps all text to a dump folder on game load. Useful for finding dialogue keys. WARNING: This dumps ALL text, including non-dialogue text.");
    }
}