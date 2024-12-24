/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-04
 */
namespace HongJinInvestment.OKX.Server;

using Newtonsoft.Json.Linq;

public static class DateTimeUtil
{
    public static DateTime FromUnixTimestamp(long unixTimestampMilliseconds)
    {
        // Unix 纪元时间
        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(unixTimestampMilliseconds);
        // 转换为 DateTime 对象
        return dateTimeOffset.DateTime;
    }

    public static long ToUnixTimestampMilliseconds(DateTime dateTime)
    {
        // 将 DateTime 转换为 DateTimeOffset
        DateTimeOffset dateTimeOffset = new DateTimeOffset(dateTime);
        // 获取 Unix 时间戳（以毫秒为单位）
        return dateTimeOffset.ToUnixTimeMilliseconds();
    }

    public static long GetCurrentTimestamp()
    {
        return ToUnixTimestampMilliseconds(DateTime.Now);
    }

    public static double GetOkxBarTimeSpanDiff(DateTime lhs, DateTime rhs, OkxBarSize okxBarSize)
    {
        TimeSpan timeSpan = rhs - lhs;

        switch (okxBarSize)
        {
            case OkxBarSize._1m:
                return timeSpan.TotalMinutes;
            case OkxBarSize._3m:
                return timeSpan.TotalMinutes / 3;
            case OkxBarSize._5m:
                return timeSpan.TotalMinutes / 5;
            case OkxBarSize._15m:
                return timeSpan.TotalMinutes / 15;
            case OkxBarSize._30m:
                return timeSpan.TotalMinutes / 30;
            case OkxBarSize._1H:
                return timeSpan.TotalHours;
            case OkxBarSize._2H:
                return timeSpan.TotalHours / 2;
            case OkxBarSize._4H:
                return timeSpan.TotalHours / 4;
            case OkxBarSize._6H:
                return timeSpan.TotalHours / 6;
            case OkxBarSize._12H:
                return timeSpan.TotalHours / 12;
            case OkxBarSize._1D:
                return timeSpan.TotalDays;
            case OkxBarSize._2D:
                return timeSpan.TotalDays / 2;
            case OkxBarSize._3D:
                return timeSpan.TotalDays / 3;
            case OkxBarSize._1W:
                return timeSpan.TotalDays / 7;
            case OkxBarSize._1M:
                return Math.Abs(lhs.Year * lhs.Month - rhs.Year * rhs.Month);
            case OkxBarSize._3M:
                return Math.Abs(lhs.Year * lhs.Month - rhs.Year * rhs.Month) / 3;
        }

        return 0;
    }
}
