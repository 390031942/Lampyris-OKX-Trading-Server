/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-18
 */
namespace HongJinInvestment.OKX.Server;

public static class MarketMonitorService
{
    // 每个instId对应的异动信息
    private class PerInstActiveInfo
    {
        // 新高新低 部分
        public double   HighPerc;
        public double   LowPerc;
        public DateTime HighTimeDateTime;
        public DateTime LowTimestamp;

        // 1min 上升/下降通道
        public DateTime OneMinKTrendTimestamp;

        // 1min 连红连绿
        public DateTime OneMinContinuousColorTimestamp;
    }

    private static List<QuoteCandleData>      ms_QuoteCandleDatas = new List<QuoteCandleData>();

    private static Func<double, double, bool> ms_Greater = (double lhs, double rhs) => { return lhs > rhs; }; 
    private static Func<double, double, bool> ms_Lesser  = (double lhs, double rhs) => { return lhs < rhs; };

    private static Dictionary<string, PerInstActiveInfo> ms_PerInstActiveInfoMap = new();

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
        ms_PerInstActiveInfoMap.Clear();
    }


    public static void Tick()
    {
        QuoteCacheService.Instance.Foreach(OkxInstType.SWAP, (string instId) => 
        {
            QuoteTickerData tickerData = RealTimeQuoteService.Query(instId);
            if(tickerData == null)
            {
                return;
            }
            PerInstActiveInfo perInstActiveInfo = ms_PerInstActiveInfoMap[instId];
            if (perInstActiveInfo == null) 
            {
                return;
            }
            QuoteCacheService.Instance.QueryLastestNoAlloc(instId, OkxBarSize._1m, ms_QuoteCandleDatas, 30);    
            MACalculator.Calculate(ms_QuoteCandleDatas);
            DateTime now = DateTime.Now;

            // 采样最近 1min k线, 判断上升/下降通道
            if(ms_QuoteCandleDatas.Count >= 10)
            {
                bool isRise = true;
                bool isFall = true;

                // 1min 均线上升通道判定
                if (DateTimeUtil.GetOkxBarTimeSpanDiff(perInstActiveInfo.OneMinKTrendTimestamp, now, OkxBarSize._1m) > 0)
                {
                    for (int i = ms_QuoteCandleDatas.Count - MarketMonitorSetting.OneMinMA5Threshold; i < ms_QuoteCandleDatas.Count - 2; i++)
                    {
                        if (CompareMovingAverage(ms_QuoteCandleDatas[i - 1], ms_QuoteCandleDatas[i], ms_Lesser))
                        {
                            isRise = false;
                            break;
                        }
                    }
                    if (!isRise)
                    {
                        // 1min 均线下降通道判定
                        for (int i = ms_QuoteCandleDatas.Count - MarketMonitorSetting.OneMinMA5Threshold; i < ms_QuoteCandleDatas.Count - 2; i++)
                        {
                            if (CompareMovingAverage(ms_QuoteCandleDatas[i - 1], ms_QuoteCandleDatas[i], ms_Greater))
                            {
                                isFall = false;
                                break;
                            }
                        }
                    }

                    if(isRise)
                    {
                        LogManager.Instance.LogInfo($"[异动提示]:{instId} 1分均线上升通道");
                        perInstActiveInfo.OneMinKTrendTimestamp = now;
                    }
                    else if (isFall)
                    {
                        LogManager.Instance.LogInfo($"[异动提示]:{instId} 1分均线下降通道");
                        perInstActiveInfo.OneMinKTrendTimestamp = now;
                    }
                }
            }

            // 采样最近 1min k线, 判断连红,连绿
            if (DateTimeUtil.GetOkxBarTimeSpanDiff(perInstActiveInfo.OneMinKTrendTimestamp, now, OkxBarSize._1m) > 0)
            {
                if (ms_QuoteCandleDatas.Count >= MarketMonitorSetting.OneMinSameColorCandleThreshold)
                {
                    bool MA5candleContinuousRiseUp = true;
                    bool MA5candleContinuousRiseDown = true;

                    for (int i = ms_QuoteCandleDatas.Count - MarketMonitorSetting.OneMinSameColorCandleThreshold; i < ms_QuoteCandleDatas.Count - 2; i++)
                    {
                        if (ms_QuoteCandleDatas[i - 1].Close < ms_QuoteCandleDatas[i].Close)
                        {
                            MA5candleContinuousRiseUp = false;
                            break;
                        }
                    }

                    for (int i = ms_QuoteCandleDatas.Count - MarketMonitorSetting.OneMinSameColorCandleThreshold; i < ms_QuoteCandleDatas.Count - 2; i++)
                    {
                        if (ms_QuoteCandleDatas[i - 1].Close > ms_QuoteCandleDatas[i].Close)
                        {
                            MA5candleContinuousRiseDown = false;
                            break;
                        }
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
                    PerInstActiveInfo newPercentangeInfo = ms_PerInstActiveInfoMap.ContainsKey(instId) ? 
                                                           ms_PerInstActiveInfoMap[instId] : new PerInstActiveInfo();
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
            if (ms_QuoteCandleDatas.Count >= 3)
            {
                var data1 = ms_QuoteCandleDatas[ms_QuoteCandleDatas.Count - 1];
                var data2 = ms_QuoteCandleDatas[ms_QuoteCandleDatas.Count - 3];

                double perc = data1.ChangePercentage(data2);
            }

            // 区间放量
            bool rangeVolActive = false;
            if (ms_QuoteCandleDatas.Count >= 15)
            {
                double moneyAvg3 = 0.0;
                double moneyAvg15 = 0.0;
                for (int i = 0; i < 15; i++)
                {
                    if(i >= 12) 
                    {
                        moneyAvg3 = moneyAvg3 + ms_QuoteCandleDatas[ms_QuoteCandleDatas.Count - i - 1].VolCcy;
                    }
                    moneyAvg15 = moneyAvg15 + ms_QuoteCandleDatas[ms_QuoteCandleDatas.Count - i - 1].VolCcy;
                }
                moneyAvg3 /= 3;
                moneyAvg15 /= 15;

                if(moneyAvg3 > 5 * moneyAvg15)
                {
                    rangeVolActive = true;
                }
            }

            // 脉冲放量
            bool suddenlyBigVolActive = false;
            if (ms_QuoteCandleDatas.Count >= 6)
            {
                double moneySum = 0.0;
                double maxMoney = 0.0;
                double curMinMoney = ms_QuoteCandleDatas[ms_QuoteCandleDatas.Count - 1].VolCcy;

                for (int i = 0; i < 5; i++)
                {
                    moneySum = moneySum + ms_QuoteCandleDatas[ms_QuoteCandleDatas.Count - i - 2].VolCcy;
                    maxMoney = Math.Max(maxMoney, moneySum);
                }

                moneySum /= 5;
                if(curMinMoney > 3 * maxMoney & curMinMoney > 3 * moneySum)
                {
                    suddenlyBigVolActive = true;
                }
            }
        });
    }
}
