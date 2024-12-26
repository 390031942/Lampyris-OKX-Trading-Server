/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-24
 */
namespace HongJinInvestment.OKX.Server;

using System;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public class IniConfigAttribute : Attribute
{
    public string Section { get; }
    public string Key { get; }
    public string DefaultValue { get; }

    public IniConfigAttribute(string section, string key, string defaultValue = "")
    {
        Section = section;
        Key = key;
        DefaultValue = defaultValue;
    }
}