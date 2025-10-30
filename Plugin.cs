using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

[BepInPlugin("com.yourname.mimesismoreplayers", "MIMESIS More Players", "1.0.0")]
public class MorePlayersPlugin : BaseUnityPlugin
{
    public static MorePlayersPlugin? Instance { get; private set; }
    public static ConfigEntry<int>? MaxPlayers { get; private set; }
    public static new ManualLogSource Logger { get; private set; } = null!;

    private void Awake()
    {
        Instance = this;
        Logger = BepInEx.Logging.Logger.CreateLogSource("MIMESIS More Players");
        MaxPlayers = Config.Bind("General", "MaxPlayers", 8, "Max players per lobby (4-16)");
        new Harmony("com.yourname.mimesismoreplayers").PatchAll();
        Logger.LogInfo($"MIMESIS More Players loaded! Max set to {MaxPlayers.Value}");
    }
}
