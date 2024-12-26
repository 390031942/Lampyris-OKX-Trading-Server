/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-24
 */
namespace HongJinInvestment.OKX.Server;

using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class IniFileAttribute : Attribute
{
    public string FileName { get; }
    public IniFileAttribute(string fileName)
    {
        FileName = fileName;
    }

    public IniFileAttribute()
    {
        FileName = "common_setting";
    }
}