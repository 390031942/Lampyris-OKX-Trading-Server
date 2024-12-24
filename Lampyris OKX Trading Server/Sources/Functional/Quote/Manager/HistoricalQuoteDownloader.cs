/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-11
 */
namespace HongJinInvestment.OKX.Server;

using System.Collections;

public static class HistoricalQuoteDownloader
{
    public static void DownloadSingleCandle(string instId)
    {

    }

    public static void DownloadCandles(List<string> instIdList)
    {

    }

    public static void DownloadAllRecentCandle(OkxInstType okxInstType)
    {
        IReadOnlyCollection<QuoteTickerData> dataList = RealTimeQuoteService.TickQuote(okxInstType);
        List<string> instIdList = new List<string>();
        foreach (QuoteTickerData data in dataList)
        {
            instIdList.Add(data.InstId);
        }

        CoroutineManager.Instance.StartCoroutine(DownloadRecentCandleProcess(instIdList, OkxBarSize._1m));
        CoroutineManager.Instance.StartCoroutine(DownloadRecentCandleProcess(instIdList, OkxBarSize._1D));
    }

    private static IEnumerator DownloadRecentCandleProcess(List<string> instIdList, OkxBarSize okxBarSize)
    {
        LogManager.Instance.LogInfo($"Start to download recent candle, okxBarSize = {okxBarSize}");
        int progress = 0;
        foreach (string instId in instIdList)
        {
            QuoteCandleData lastestCandleData = QuoteCacheService.Instance.QueryLastest(instId, okxBarSize);
            var canldeFuture = HistoricalQuoteService.QueryRecentCandleAsync(instId, okxBarSize);
            yield return canldeFuture;

            QuoteCacheService.Instance.Storage(instId, okxBarSize, canldeFuture.GetResult());

            yield return new WaitForSeconds(0.1);

            LogManager.Instance.LogInfo($"Downloading recent candle, okxBarSize = {okxBarSize}, process = {++progress}/{instIdList.Count}");
        }
    }

    private static IEnumerator DownloadHistoryCandleProcess(List<string> instIdList, OkxBarSize okxBarSize)
    {
        LogManager.Instance.LogInfo($"Start to download history candle, okxBarSize = {okxBarSize}");
        int progress = 0;
        foreach (string instId in instIdList)
        {
            QuoteCandleData lastestCandleData = QuoteCacheService.Instance.QueryLastest(instId, okxBarSize);
            var canldeFuture = HistoricalQuoteService.QueryRecentCandleAsync(instId, okxBarSize);
            yield return canldeFuture;

            QuoteCacheService.Instance.Storage(instId, okxBarSize, canldeFuture.GetResult());

            yield return new WaitForSeconds(0.1);

            LogManager.Instance.LogInfo($"Downloading recent candle, okxBarSize = {okxBarSize}, process = {++progress}/{instIdList.Count}");
        }
    }
}