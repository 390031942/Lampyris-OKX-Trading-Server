/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-15
 */
namespace HongJinInvestment.OKX.Server;

using System;

public static class OkxRequestUrlParamMaker
{
    public static string GetCandleUrl(bool isHistory, string instId, OkxBarSize barSize, DateTime? after, DateTime? before, int? limit)
    {
        string url = NetworkConfig.BaseUrl + $"/api/v5/market/{(isHistory ? "history-candles" : "candles")}?instId={instId}";

        // barSize
        url += $"&bar={EnumNameManager.GetName(barSize)}";

        // after
        if (after != null)
        {
            url += $"&$after={DateTimeUtil.ToUnixTimestampMilliseconds(after.Value)}";
        }

        // before
        if (before != null)
        {
            url += $"&before={DateTimeUtil.ToUnixTimestampMilliseconds(before.Value)}";
        }

        // limit
        if (limit != null)
        {
            url += $"&limit={limit}";
        }

        return url;
    }

}
