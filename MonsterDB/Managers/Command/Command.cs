using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterDB;

public class Command
{
    public readonly string m_description;
    private readonly bool m_isSecret;
    public readonly bool m_adminOnly;
    private readonly Func<Terminal.ConsoleEventArgs, bool> m_command;
    private readonly Func<List<string>>? m_optionFetcher;
    public bool Run(Terminal.ConsoleEventArgs args) => !IsAdmin() || m_command(args);
    private bool IsAdmin()
    {
        if (!ZNet.m_instance) return true;
        if (!m_adminOnly || ZNet.m_instance.LocalPlayerIsAdminOrHost()) return true;
        MonsterDBPlugin.LogWarning("Admin only");
        return false;
    }
    public bool IsSecret() => m_isSecret;
    public List<string> FetchOptions() => m_optionFetcher == null ? new List<string>() : m_optionFetcher();
    public bool HasOptions() => m_optionFetcher != null;
        
    public Command(string input, string description, Func<Terminal.ConsoleEventArgs, bool> command, Func<List<string>>? optionsFetcher = null, bool isSecret = false, bool adminOnly = false)
    {
        m_description = description;
        m_command = command;
        m_isSecret = isSecret;
        CommandManager.commands[input] = this;
        m_optionFetcher = optionsFetcher;
        m_adminOnly = adminOnly;
    }
}