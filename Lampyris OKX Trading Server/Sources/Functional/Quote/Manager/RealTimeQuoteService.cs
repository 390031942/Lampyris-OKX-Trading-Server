﻿/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-11
 */

namespace HongJinInvestment.OKX.Server;

public static class RealTimeQuoteService
{
    private static Dictionary<string, QuoteTickerData> ms_RealTimeQuoteDataMap = new ();

    private static List<QuoteTickerData> ms_RealTimeQuoteDataList = new ();
    public static IReadOnlyCollection<QuoteTickerData> TickQuote(OkxInstType instType)
    {
        string url = NetworkConfig.BaseUrl + $"/api/v5/market/tickers?instType={instType}";
        HttpRequest.GetSync(url,(json =>
        {
            try
            {
                 OkxResponseJsonParser.ParseTickerListNoAlloc(json,ms_RealTimeQuoteDataList);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError($"Parsing json failed, reason: {ex.Message}");
            }
        }));

        ms_RealTimeQuoteDataMap.Clear();
        if (ms_RealTimeQuoteDataList != null)
        {
            foreach (QuoteTickerData data in ms_RealTimeQuoteDataList)
            {
                ms_RealTimeQuoteDataMap[data.InstId] = data;
            }
            return ms_RealTimeQuoteDataList.AsReadOnly();
        }

        return null;
    }

    public static QuoteTickerData Query(string instId)
    {
        return ms_RealTimeQuoteDataMap[instId];
    }
}