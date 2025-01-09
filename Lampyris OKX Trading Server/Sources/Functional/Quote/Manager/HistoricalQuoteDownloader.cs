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

    public static void DownloadAllRecentCandle(OkxInstType okxInstType, List<OkxBarSize> barSizes, Action<OkxBarSize>? callback = null)
    {
        IReadOnlyCollection<QuoteTickerData> dataList = RealTimeQuoteService.TickQuote(okxInstType);
        List<string> instIdList = new List<string>();
        foreach (QuoteTickerData data in dataList)
        {
            instIdList.Add(data.InstId);
        }
        foreach(OkxBarSize barSize in barSizes)
        {
            CoroutineManager.Instance.StartCoroutine(DownloadRecentCandleProcess(instIdList, barSize, callback, 0.05 * barSizes.Count));
        }
    }

    public static void DownloadAllHistoryCandle(OkxInstType okxInstType, List<OkxBarSize> barSizes, int n, Action<OkxBarSize>? callback = null)
    {
        IReadOnlyCollection<QuoteTickerData> dataList = RealTimeQuoteService.TickQuote(okxInstType);
        List<string> instIdList = new List<string>();
        foreach (QuoteTickerData data in dataList)
        {
            instIdList.Add(data.InstId);
        }
        foreach (OkxBarSize barSize in barSizes)
        {
            CoroutineManager.Instance.StartCoroutine(DownloadHistoryCandleProcess(instIdList, barSize, n, callback, 0.05 * barSizes.Count));
        }
    }

    private static IEnumerator DownloadRecentCandleProcess(List<string> instIdList, OkxBarSize okxBarSize, Action<OkxBarSize>? callback, double delaySec = 0.1)
    {
        LogManager.Instance.LogInfo($"Start to download recent candle, okxBarSize = {okxBarSize}");
        int progress = 0;
        foreach (string instId in instIdList)
        {
            QuoteCandleData lastestCandleData = QuoteCacheService.Instance.QueryLastest(instId, okxBarSize);
            var canldeFuture = HistoricalQuoteService.QueryRecentCandleAsync(instId, okxBarSize);
            yield return canldeFuture;

            QuoteCacheService.Instance.Storage(instId, okxBarSize, canldeFuture.GetResult());

            yield return new WaitForSeconds(delaySec);

            LogManager.Instance.LogInfo($"Downloading recent candle, okxBarSize = {okxBarSize}, process = {++progress}/{instIdList.Count}");
        }

        LogManager.Instance.LogInfo($"Downloading recent candle, okxBarSize = {okxBarSize} Finished!");
        callback?.Invoke(okxBarSize);
    }

    private static IEnumerator DownloadHistoryCandleProcess(List<string> instIdList, OkxBarSize okxBarSize, int n, Action<OkxBarSize>? callback, double delaySec = 0.1)
    {
        LogManager.Instance.LogInfo($"Start to download historical candle, okxBarSize = {okxBarSize}");
        int progress = 0;
        foreach (string instId in instIdList)
        {
            QuoteCandleData lastestCandleData = QuoteCacheService.Instance.QueryLastest(instId, okxBarSize);
            var canldeFuture = HistoricalQuoteService.QueryHistoryCandleAsync(instId, okxBarSize,limit:n);
            yield return canldeFuture;

            QuoteCacheService.Instance.Storage(instId, okxBarSize, canldeFuture.GetResult());

            yield return new WaitForSeconds(delaySec);

            LogManager.Instance.LogInfo($"Downloading historical candle, okxBarSize = {okxBarSize}, process = {++progress}/{instIdList.Count}");
        }

        LogManager.Instance.LogInfo($"Downloading historical candle, okxBarSize = {okxBarSize} Finished!");
        callback?.Invoke(okxBarSize);
    }
}