/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-05
 */
namespace HongJinInvestment.OKX.Server;

public enum OkxInstType
{
    SPOT = 0, // 现货
    SWAP = 1, // 永续合约
    FUTURES = 2, // 交割合约 (未使用)
    OPTION = 3, // 期权 (未使用)
}