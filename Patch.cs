using HarmonyLib;
using System.Collections.Generic;
using System.Linq;  // For ToList()
using System.Reflection.Emit;
using ReluProtocol.Enum;  // For MsgErrorCode
using ReluProtocol;  // For HostClientInfo

// Steam Lobby Cap (Transpiler: Replaces ldc.i4.4 with config value)
[HarmonyPatch(typeof(SteamInviteDispatcher), "CreateLobby")]
class SteamLobbyPatch
{
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        for (int i = 0; i < codes.Count; i++)
        {
            if (codes[i].opcode == OpCodes.Ldc_I4_4)  // Hardcoded 4
            {
                codes[i] = new CodeInstruction(OpCodes.Ldc_I4, MorePlayersPlugin.MaxPlayers?.Value ?? 4);  // Replace with config
            }
        }
        return codes;  // Non-null return
    }
}

// IVroom CanEnterChannel (Prefix: Overrides >=4 check; matches decomp sig)
[HarmonyPatch(typeof(IVroom), "CanEnterChannel")]
class IVroomCanEnterPatch
{
    static bool Prefix(IVroom __instance, long playerUID, ref MsgErrorCode __result)
    {
        #pragma warning disable CS8602  // Suppress null warnings
        var vPlayerDict = HarmonyLib.AccessTools.Field(typeof(IVroom), "_vPlayerDict").GetValue(__instance);
        var countProp = HarmonyLib.AccessTools.Property(vPlayerDict?.GetType(), "Count");
        int count = (int)(countProp?.GetValue(vPlayerDict) ?? 0);
        #pragma warning restore CS8602

        if (count >= MorePlayersPlugin.MaxPlayers?.Value)
        {
            MorePlayersPlugin.Logger.LogInfo($"CanEnterChannel: Allowing extra player {playerUID} (modded max: {MorePlayersPlugin.MaxPlayers?.Value})");
            __result = MsgErrorCode.Success;  // Force success (override error)
            return false;  // Skip original method
        }
        return true;  // Run original (under limit)
    }
}

// VRoomManager GetHostClientInfo (Postfix: Overrides maxCnt=4)
[HarmonyPatch(typeof(VRoomManager), "GetHostClientInfo")]
class VRoomManagerHostInfoPatch
{
    static void Postfix(ref HostClientInfo __result)
    {
        __result.maxCnt = MorePlayersPlugin.MaxPlayers?.Value ?? 4;
    }
}
