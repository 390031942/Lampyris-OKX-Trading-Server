/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-05
 */
namespace HongJinInvestment.OKX.Server;

public class Singleton<T> where T : class, new()
{
    private static T? m_Instance;

    public static T Instance
    {
        get { return m_Instance ??= new T(); }
    }
}
