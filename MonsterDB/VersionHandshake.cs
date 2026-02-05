using System;
using System.Collections.Generic;
using HarmonyLib;

namespace MonsterDB;

public static class VersionHandshake
{
    private static string ConnectionError;
    private static readonly List<ZRpc> ValidatedPeers;
    private static readonly string RequestAdminSyncMethodName;
    static VersionHandshake()
    {
        ConnectionError = "";
        ValidatedPeers = new List<ZRpc>();
        RequestAdminSyncMethodName = $"{MonsterDBPlugin.ModName}RequestAdminSync";
    }

    public static void Setup()
    {
        Harmony harmony = MonsterDBPlugin.harmony;
        harmony.Patch(AccessTools.Method(typeof(ZNet), nameof(ZNet.OnNewConnection)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(VersionHandshake), nameof(RegisterAndCheckVersion))));
        harmony.Patch(AccessTools.Method(typeof(ZNet), nameof(ZNet.RPC_PeerInfo)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(VersionHandshake), nameof(VerifyClient))),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(VersionHandshake), nameof(RequestAdminSync))));
        harmony.Patch(AccessTools.Method(typeof(FejdStartup), nameof(FejdStartup.ShowConnectError)),
            postfix: new HarmonyMethod(AccessTools.Method(typeof(VersionHandshake), nameof(ShowConnectionError))));
        harmony.Patch(AccessTools.Method(typeof(ZNet), nameof(ZNet.Disconnect)),
            prefix: new HarmonyMethod(AccessTools.Method(typeof(VersionHandshake), nameof(RemoveDisconnectedPeerFromVerified))));
    }

    private static void RPC_MonsterDB_VersionCheck(ZRpc rpc, ZPackage pkg)
    {
        string? version = pkg.ReadString();

        MonsterDBPlugin.LogInfo($"Version check, local: {MonsterDBPlugin.ModVersion}, remove: {version}");
        if (version != MonsterDBPlugin.ModVersion)
        {
            ConnectionError =
                $"{MonsterDBPlugin.ModName} Installed: {MonsterDBPlugin.ModVersion}\n Needed: {version}";
            if (!ZNet.instance.IsServer()) return;
            // Different versions - force disconnect client from server
            MonsterDBPlugin.LogWarning(
                $"Peer ({rpc.m_socket.GetHostName()}) has incompatible version, disconnecting...");
            rpc.Invoke("Error", 3);
        }
        else
        {
            if (!ZNet.instance.IsServer())
            {
                // Enable mod on client if versions match
                MonsterDBPlugin.LogInfo("Received same version from server!");
            }
            else
            {
                // Add client to validated list
                MonsterDBPlugin.LogInfo(
                    $"Adding peer ({rpc.m_socket.GetHostName()}) to validated list");
                ValidatedPeers.Add(rpc);
            }
        }
    }

    public static void RegisterAndCheckVersion(ZNetPeer peer)
    {
        // Register version check call
        MonsterDBPlugin.LogDebug("Registering version RPC handler");
        peer.m_rpc.Register(nameof(RPC_MonsterDB_VersionCheck),
            new Action<ZRpc, ZPackage>(RPC_MonsterDB_VersionCheck));

        // Make calls to check versions
        MonsterDBPlugin.LogInfo("Invoking version check");
        ZPackage zpackage = new();
        zpackage.Write(MonsterDBPlugin.ModVersion);
        peer.m_rpc.Invoke(nameof(RPC_MonsterDB_VersionCheck), zpackage);
    }

    public static bool VerifyClient(ZRpc rpc, ref ZNet __instance)
    {
        if (!__instance.IsServer() || ValidatedPeers.Contains(rpc)) return true;
        // Disconnect peer if they didn't send mod version at all
        MonsterDBPlugin.LogWarning(
            $"Peer ({rpc.m_socket.GetHostName()}) never sent version or couldn't due to previous disconnect, disconnecting");
        rpc.Invoke("Error", 3);
        return false; // Prevent calling underlying method
    }

    public static void RequestAdminSync()
    {
        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(),
            RequestAdminSyncMethodName,
            new ZPackage());
    }

    public static void ShowConnectionError(FejdStartup __instance)
    {
        if (__instance.m_connectionFailedPanel.activeSelf)
        {
            __instance.m_connectionFailedError.fontSizeMax = 25;
            __instance.m_connectionFailedError.fontSizeMin = 15;
            __instance.m_connectionFailedError.text += "\n" + ConnectionError;
            MonsterDBPlugin.LogFatal(ConnectionError);
        }
    }

    public static void RemoveDisconnectedPeerFromVerified(ZNetPeer peer, ref ZNet __instance)
    {
        if (!__instance.IsServer()) return;
        // Remove peer from validated list
        MonsterDBPlugin.LogInfo(
            $"Peer ({peer.m_rpc.m_socket.GetHostName()}) disconnected, removing from validated list");
        ValidatedPeers.Remove(peer.m_rpc);
    }
}