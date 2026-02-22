using BepInEx;

namespace VoicesOfPharloom;

// TODO - adjust the plugin guid as needed
[BepInAutoPlugin(id: "dev.ashiepaws.voices_of_pharloom")]
public partial class VoicesOfPharloomPlugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo($"Plugin {Name} ({Id}) has loaded!");
    }
}
