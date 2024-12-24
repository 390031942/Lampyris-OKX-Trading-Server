/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-15
 */
namespace HongJinInvestment.OKX.Server;

public class WaitForSeconds : AsyncOperation
{
    private double m_WaitTimeSeconds;
    private double m_StartTimestamp;

    public WaitForSeconds(double seconds)
    {
        m_WaitTimeSeconds = seconds;
        m_StartTimestamp = DateTimeUtil.GetCurrentTimestamp();
    }

    public override bool MoveNext()
    {
        return (DateTimeUtil.GetCurrentTimestamp() - m_StartTimestamp) >= 1000 * m_WaitTimeSeconds;
    }

    public override void Reset()
    {
        m_StartTimestamp = DateTimeUtil.GetCurrentTimestamp();
    }
}
