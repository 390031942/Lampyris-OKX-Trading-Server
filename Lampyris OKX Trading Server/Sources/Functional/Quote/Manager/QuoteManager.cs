/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-06
 */
namespace HongJinInvestment.OKX.Server;

public class QuoteManager:BehaviourSingleton<QuoteManager>
{
    /* 最近的数据下载计数，要与m_HistoricalQuoteBarSizesOnInit相等时，才能开启市场监控*/
    private int m_RecentQuoteFinishedDownloadOnInitCounter = 0;

    /* 运行的定时器任务id列表 */
    private List<int> m_CallTimerIds = new List<int>();

    /* 初始化需要的行情barSize列表 */
    private readonly List<OkxBarSize> m_HistoricalQuoteBarSizesOnInit = new List<OkxBarSize>() 
    {
        OkxBarSize._1m,// OkxBarSize._15m,OkxBarSize._1D
    };

    /* 每分钟需要的行情barSize列表 */
    private readonly List<OkxBarSize> m_HistoricalQuoteBarSizesPerMin = new List<OkxBarSize>()
    {
        OkxBarSize._1m
    };

    /* 是否初始化完毕-状态变量 */
    private bool m_IsInitialized = false;

    public override void OnStart()
    {
        // 启动时需要预下载最新的k线
        HistoricalQuoteDownloader.DownloadAllRecentCandle(OkxInstType.SWAP, m_HistoricalQuoteBarSizesOnInit, (barSize)=> 
        {
            m_RecentQuoteFinishedDownloadOnInitCounter++;
        });
    }

    public override void OnUpdate(float deltaTime)
    {
        if(!m_IsInitialized)
        {
            if(m_RecentQuoteFinishedDownloadOnInitCounter == m_HistoricalQuoteBarSizesOnInit.Count)
            {
                // Tick 行情列表-现货
                m_CallTimerIds.Add(CallTimer.Instance.SetInterval(() =>
                {
                    // m_RealTimeQuoteService.TickQuote(OkxInstType.SPOT);
                }, 200, -1));

                // Tick 行情列表-合约
                m_CallTimerIds.Add(CallTimer.Instance.SetInterval(() =>
                {
                    RealTimeQuoteService.TickQuote(OkxInstType.SWAP);
                }, 200, -1));

                // Tick 分钟/日k线更新
                m_CallTimerIds.Add(CallTimer.Instance.SetInterval(() =>
                {
                    DateTime lastDt = RealTimeQuoteService.QueryLatestDateTime();
                    DateTime curDt = DateTime.Now;
                    if (DateTimeUtil.GetOkxBarTimeSpanDiff(lastDt, curDt, OkxBarSize._1m) != 0)
                    {
                        HistoricalQuoteDownloader.DownloadAllHistoryCandle(OkxInstType.SWAP, m_HistoricalQuoteBarSizesPerMin, 2);
                    }
                }, 1000, -1));

                // 24点刷新逻辑
                m_CallTimerIds.Add(CallTimer.Instance.SetInterval(() =>
                {

                }, 1000, -1));

                // 市场最近的数据初始化后，启动 市场监测
                LogManager.Instance.LogInfo("All recent quote downloads have finished, starting market monitoring.");
                m_CallTimerIds.Add(CallTimer.Instance.SetInterval(() =>
                {
                    MarketMonitorService.Tick();
                }, 1000, -1));
                m_IsInitialized = true;
            }
        }
    }

    public override void OnDestroy()
    {
        foreach(int id in m_CallTimerIds)
        {
            CallTimer.Instance.ClearTimer(id);
        }
    }
}
