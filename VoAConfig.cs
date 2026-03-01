using BepInEx.Configuration;

namespace VoicesOfPharloom;

public class VoAConfig
{
    private static ConfigEntry<bool> _ShowDialogueKeys;
    public static bool ShowDialogueKeys => _ShowDialogueKeys.Value;

    public static void Init(ConfigFile config)
    {
        _ShowDialogueKeys = config.Bind("General", "Show Dialogue Keys", false, "Shows the current dialogue key whenever a dialogue box is opened.");
    }
}