/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-15
 */
namespace HongJinInvestment.OKX.Server;

using System.Collections;
using System.Collections.Generic;

public class WaitForQuoteCandleResult : AsyncOperation
{
    private Task<HttpResponseMessage> m_Task;

    public List<QuoteCandleData> GetResult()
    {
        if (m_Task.Result.IsSuccessStatusCode)
        {
            string json = m_Task.Result.Content.ReadAsStringAsync().Result;
            return OkxResponseJsonParser.ParseCandleList(json);
        }

        return null;
    }

    public WaitForQuoteCandleResult(string url)
    {
        m_Task = HttpRequest.GetTemp().GetAsync(url);
    }

    public override bool MoveNext()
    {
        return m_Task.IsCompleted;
    }
}
