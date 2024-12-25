/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-24
 */
namespace HongJinInvestment.OKX.Server;

using System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;

public static class IniConfigManager
{
    private static readonly Dictionary<string, IniFile> m_IniFiles = new Dictionary<string, IniFile>(StringComparer.OrdinalIgnoreCase);

    public static void LoadConfig(Type configType)
    {
        var fields = configType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => Attribute.IsDefined(field, typeof(IniConfigAttribute)));

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<IniConfigAttribute>();
            if (!m_IniFiles.ContainsKey(attribute.FileName))
            {
                m_IniFiles[attribute.FileName] = new IniFile(attribute.FileName);
            }

            string value = m_IniFiles[attribute.FileName].ReadValue(attribute.Section, attribute.Key, attribute.DefaultValue);
            var convertedValue = Convert.ChangeType(value, field.FieldType);
            field.SetValue(null, convertedValue);
        }
    }

    public static void SaveConfig(Type configType)
    {
        var fields = configType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(field => Attribute.IsDefined(field, typeof(IniConfigAttribute)));

        foreach (var field in fields)
        {
            var attribute = field.GetCustomAttribute<IniConfigAttribute>();
            if (!m_IniFiles.ContainsKey(attribute.FileName))
            {
                m_IniFiles[attribute.FileName] = new IniFile(attribute.FileName);
            }

            string value = field.GetValue(null)?.ToString();
            m_IniFiles[attribute.FileName].WriteValue(attribute.Section, attribute.Key, value);
        }

        foreach (var iniFile in m_IniFiles.Values)
        {
            iniFile.Save();
        }
    }
}
