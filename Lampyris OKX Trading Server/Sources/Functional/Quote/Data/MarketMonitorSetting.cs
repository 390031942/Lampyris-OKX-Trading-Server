/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-24
 */
namespace HongJinInvestment.OKX.Server;

[IniFile("MarketMonitorSetting.ini")]
public static class MarketMonitorSetting
{
    [IniConfig("OneMinCandle", "1minK_SameColorCandleThreshold")]
    public static int OneMinSameColorCandleThreshold = 5;

    [IniConfig("OneMinCandle", "1minK_MA5_Threshold")]
    public static int OneMinMA5Threshold = 10;
}
