/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-15
 */
namespace HongJinInvestment.OKX.Server;

using System.Collections;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HongJinInvestment.OKX.Server.Internal;

public class WaitForHttpResponse : IEnumerator
{
    private Task<HttpResponseMessage> m_Task;
    private HttpResponseMessage       m_Response;
    private HttpRequestInternal       m_Client;

    public WaitForHttpResponse(string url)
    {
        m_Client = HttpRequest.GetTemp();
        m_Task = m_Client.GetAsync(url);
    }

    public WaitForHttpResponse(string url, string content, string? mediaType = "application/json")
    {
        m_Client = HttpRequest.GetTemp();
        var requestBody = new StringContent(content, Encoding.UTF8, mediaType);
        m_Task = m_Client.PostAsync(url, requestBody);
    }

    public bool MoveNext()
    {
        if (m_Task.IsCompleted)
        {
            m_Response = m_Task.Result;
            return true;
        }
        return false;
    }

    public void Reset()
    {
        m_Client = HttpRequest.GetTemp();
    }

    public object Current => m_Response;

    public string Result
    {
        get
        {
            if (m_Task != null && m_Task.IsCompleted)
            {
                return m_Task.Result.Content.ReadAsStringAsync().Result;
            }
            return "";
        }
    }
}