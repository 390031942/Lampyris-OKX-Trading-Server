/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-06
 */
namespace HongJinInvestment.OKX.Server;

public class QuoteManager:BehaviourSingleton<QuoteManager>
{
    public override void OnStart()
    {
        // 启动时需要预下载最新的k线
        HistoricalQuoteDownloader.DownloadAllRecentCandle(OkxInstType.SWAP);

        // Tick行情列表-现货
        CallTimer.Instance.SetInterval(() =>
        {
            // m_RealTimeQuoteService.TickQuote(OkxInstType.SPOT);
        }, 5000, -1);
        
        // Tick行情列表-合约
        CallTimer.Instance.SetInterval(() =>
        {
            RealTimeQuoteService.TickQuote(OkxInstType.SWAP);
        }, 5000, -1);

        // Tick分钟/日k线更新
        CallTimer.Instance.SetInterval(() =>
        {
            int minute = -1;
            int curMinute = DateTime.Now.Minute;
            if (curMinute < minute)
            {
                minute = curMinute;
                HistoricalQuoteDownloader.DownloadAllRecentCandle(OkxInstType.SWAP);
            }
            RealTimeQuoteService.TickQuote(OkxInstType.SWAP);
        }, 1000, -1);
    }

    public override void OnUpdate(float deltaTime)
    {

    }

    public override void OnDestroy()
    {
    }

    public QuoteCandleData Query(string instId)
    {
        return                                                                                  
    }
}
