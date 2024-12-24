/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-18
 */
namespace HongJinInvestment.OKX.Server;

public static class MarketMonitorService
{
    private class NewPercentangeInfo
    {
        public double HighPerc;
        public double LowPerc;
        public long   HighTimestamp;
        public long   LowTimestamp;
    }

    private static List<QuoteCandleData>      ms_QuoteCandleDatas = new List<QuoteCandleData>();

    private static Func<double, double, bool> ms_Greater = (double lhs, double rhs) => { return lhs > rhs; }; 
    private static Func<double, double, bool> ms_Lesser  = (double lhs, double rhs) => { return lhs < rhs; };

    private static Dictionary<string, NewPercentangeInfo> ms_NewPercentageInfoMap = new();

    private static bool CompareMovingAverage(QuoteCandleData lhs, QuoteCandleData rhs, Func<double,double,bool> compareFunc) 
    {
        if(!(compareFunc(lhs.MA5 ,rhs.MA5)  &&
             compareFunc(lhs.MA10,rhs.MA10) && 
             compareFunc(lhs.MA20,rhs.MA20) &&
             compareFunc(lhs.MA5 ,rhs.MA10) &&  
             compareFunc(lhs.MA10,rhs.MA20)))
        {
            return false;
        }

        return true;
    }

    public static void Reset()
    {
        ms_NewPercentageInfoMap.Clear();
    }


    public static void TestSuddenlyActive()
    {
        QuoteCacheService.Instance.Foreach(OkxInstType.SWAP, (string instId) => 
        {
            QuoteCacheService.Instance.QueryLastestNoAlloc(instId, OkxBarSize._1m, ms_QuoteCandleDatas, 30);
            MACalculator.Calculate(ms_QuoteCandleDatas);

            // 采样最近10根 1min k线, 判断上升通道
            if(ms_QuoteCandleDatas.Count >= 10)
            {
                bool MA5RiseUp   = true;
                bool MA5RiseDown = true;

                // 1min 均线上升通道判定
                for (int i = ms_QuoteCandleDatas.Count - 10; i < ms_QuoteCandleDatas.Count - 2; i++)
                {
                    if (CompareMovingAverage(ms_QuoteCandleDatas[i - 1], ms_QuoteCandleDatas[i], ms_Lesser))
                    {
                        MA5RiseUp = false;
                        break;
                    }
                }

                // 1min 均线下降通道判定
                for (int i = ms_QuoteCandleDatas.Count - 10; i < ms_QuoteCandleDatas.Count - 2; i++)
                {
                    if (CompareMovingAverage(ms_QuoteCandleDatas[i - 1], ms_QuoteCandleDatas[i], ms_Greater))
                    {
                        MA5RiseDown = false;
                        break;
                    }
                }
            }

            // 采样最近5根 1min k线, 判断连红,连绿
            if (ms_QuoteCandleDatas.Count >= 5)
            {
                bool candleContinuousRiseUp = true;
                bool candleContinuousRiseDown = true;

                for (int i = ms_QuoteCandleDatas.Count - 5; i < ms_QuoteCandleDatas.Count - 2; i++)
                {
                    if (ms_QuoteCandleDatas[i - 1].Close <  ms_QuoteCandleDatas[i].Close)
                    {
                        candleContinuousRiseUp = false;
                        break;
                    }
                }

                for (int i = ms_QuoteCandleDatas.Count - 5; i < ms_QuoteCandleDatas.Count - 2; i++)
                {
                    if (ms_QuoteCandleDatas[i - 1].Close > ms_QuoteCandleDatas[i].Close)
                    {
                        candleContinuousRiseDown = false;
                        break;
                    }
                }
            }

            // 采样最近5根 15min k线, 判断连红连绿+上升通道
            if (ms_QuoteCandleDatas.Count >= 5)
            {
                bool candleContinuousRiseUp = true;
                bool candleContinuousRiseDown = true;

                for (int i = ms_QuoteCandleDatas.Count - 5; i < ms_QuoteCandleDatas.Count - 2; i++)
                {
                    if (ms_QuoteCandleDatas[i - 1].Close < ms_QuoteCandleDatas[i].Close)
                    {
                        candleContinuousRiseUp = false;
                        break;
                    }
                }

                for (int i = ms_QuoteCandleDatas.Count - 5; i < ms_QuoteCandleDatas.Count - 2; i++)
                {
                    if (ms_QuoteCandleDatas[i - 1].Close > ms_QuoteCandleDatas[i].Close)
                    {
                        candleContinuousRiseDown = false;
                        break;
                    }
                }
            }

            // 24小时新高/新低且大于1%
            if (ms_QuoteCandleDatas.Count >= 10)
            {
                QuoteTickerData realTimeData = RealTimeQuoteService.Query(instId);
                if (realTimeData != null)
                {
                    NewPercentangeInfo newPercentangeInfo = ms_NewPercentageInfoMap.ContainsKey(instId) ? 
                                                            ms_NewPercentageInfoMap[instId] : new NewPercentangeInfo();
                    if (realTimeData.Percentage >= 1)
                    {
                        if(realTimeData.Percentage > newPercentangeInfo.HighPerc)
                        {
                            
                        }
                    }
                    else if(realTimeData.Percentage <= -1)
                    {
                        if (realTimeData.Percentage < newPercentangeInfo.LowPerc)
                        {

                        }
                    }
                }
            }
            // 分钟级涨速/跌速>1.5%
        });
    }
}
