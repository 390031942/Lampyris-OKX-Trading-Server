﻿/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-18
 */
namespace HongJinInvestment.OKX.Server;

public class MACalculator
{
    public static void Calculate(List<QuoteCandleData> quoteCandleDatas)
    {
        if (quoteCandleDatas == null || quoteCandleDatas.Count >= 5)
            return;

        for (int i = 0; i < quoteCandleDatas.Count; i++)
        {
            if (i >= 4)
            {
                quoteCandleDatas[i].MA5 = quoteCandleDatas.Skip(i - 4).Take(5).Average(x => x.Close);
            }
            if (i >= 9)
            {
                quoteCandleDatas[i].MA10 = quoteCandleDatas.Skip(i - 9).Take(10).Average(x => x.Close);
            }
            if (i >= 19)
            {
                quoteCandleDatas[i].MA20 = quoteCandleDatas.Skip(i - 19).Take(20).Average(x => x.Close);
            }
        }
    }
}
