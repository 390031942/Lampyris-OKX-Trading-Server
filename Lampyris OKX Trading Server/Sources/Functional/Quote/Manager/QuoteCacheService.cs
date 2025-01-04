/*
 * Copyright (C) 2024 The Hong-Jin Investment Company.
 * This file is part of the OKX Trading Server.
 * File created at 2024-12-13
 */
namespace HongJinInvestment.OKX.Server;

public class QuoteCacheService:BehaviourSingleton<QuoteCacheService>
{
    /*
     * Key   = instId
     * Value = {Key = OkxBarSize, Value = List<QuoteCandleData>}
     */
    private Dictionary<string, Dictionary<OkxBarSize, List<QuoteCandleData>>> m_CandleDataMap = new ();

    private Dictionary<OkxInstType, HashSet<string>> m_Inst2InstIdListMap = new ();


    public override void OnStart()
    {
        // 从文件中加载出来行情instId列表，并对每个instId逐个进行反序列化
        List<string> quoteList = SerializationManager.Instance.TryDeserializeObjectFromFile<List<string>>("quote-list.bin");
        List<string> okxBarSize = EnumNameManager.GetNames(typeof(OkxBarSize));

        if(quoteList != null)
        {
            foreach (string instId in quoteList)
            {
                m_CandleDataMap[instId] = new Dictionary<OkxBarSize, List<QuoteCandleData>>();
                foreach(var enumName in okxBarSize)
                {
                    OkxBarSize barSize = (OkxBarSize)EnumNameManager.GetEnum(typeof(OkxBarSize), enumName);
                    string fileName = $"quote-cache/{instId}_{enumName}.bin";
                    var quoteCacheData = SerializationManager.Instance.TryDeserializeObjectFromFile<List<QuoteCandleData>>(fileName);
                    if (quoteCacheData != null)
                    {
                        m_CandleDataMap[instId][barSize] = quoteCacheData;
                    }
                }
            }
        }
    }

    public List<QuoteCandleData> Query(string instId, OkxBarSize barSize, DateTime startTime, DateTime endTime)
    {
        return new List<QuoteCandleData>();
    }

    public void Storage(string instId, OkxBarSize barSize, List<QuoteCandleData> dataList)
    {
        if (string.IsNullOrEmpty(instId)) 
        {
            LogManager.Instance.LogError("param \"instId\" can not be null or empty!");
            return;
        }

        if(dataList == null || dataList.Count <= 0)
        {
            LogManager.Instance.LogError("param \"dataList\" can not be null or empty!");
            return;

        } 

        if (!m_CandleDataMap.ContainsKey(instId))
        {
            m_CandleDataMap[instId] = new Dictionary<OkxBarSize, List<QuoteCandleData>>();
        }

        var barSizeDataMap = m_CandleDataMap[instId];
        if(!barSizeDataMap.ContainsKey(barSize))
        {
            barSizeDataMap[barSize] = new List<QuoteCandleData>();
        }

        var storageList = barSizeDataMap[barSize];

        if (dataList != null)
        {
            int firstIndex = storageList.BinarySearch(dataList.First<QuoteCandleData>());

            // 开头都没找到，后面也不用找了
            if(firstIndex < 0)
            {
                // 直接找到第一个大于dataList.First<QuoteCandleData>().dateTime的索引
                int lowerIndex = storageList.LowerBound(dataList.First<QuoteCandleData>());

                // 在lowerIndex后插入
                storageList.InsertRange(lowerIndex, dataList);
            }
            else
            {
                int endIndex = storageList.BinarySearch(dataList.Last<QuoteCandleData>());
                // 末尾都没找到说明中间往后缺了一段数据
                if (endIndex < 0)
                {
                    // 直接找到第一个小于dataList.Last<QuoteCandleData>().dateTime的索引
                    int upperIndex = storageList.UpperBound(dataList.Last<QuoteCandleData>());

                    // 在upperIndex后插入
                    storageList.InsertRange(upperIndex, dataList);
                }
            }
        }
    }

    public QuoteCandleData QueryLastest(string instId, OkxBarSize okxBarSize)
    {
        if(!m_CandleDataMap.ContainsKey(instId))
            return null;

        var barSizeDataMap = m_CandleDataMap[instId];

        if (!barSizeDataMap.ContainsKey(okxBarSize))
            return null;

        var storageList = barSizeDataMap[okxBarSize];
        if (storageList != null)
        {
            return storageList.Last<QuoteCandleData>();
        }

        return null;
    }

    public List<QuoteCandleData> QueryLastest(string instId, OkxBarSize okxBarSize, int n)
    {
        List<QuoteCandleData> result = new List<QuoteCandleData>();
        QueryLastestNoAlloc(instId, okxBarSize, result, n);
        return result;
    }

    public void QueryLastestNoAlloc(string instId, OkxBarSize okxBarSize, List<QuoteCandleData> result, int n)
    {
        result.Clear();
        if (!m_CandleDataMap.ContainsKey(instId))
            return;

        var barSizeDataMap = m_CandleDataMap[instId];
        if (!barSizeDataMap.ContainsKey(okxBarSize))
            return;

        var storageList = barSizeDataMap[okxBarSize];
        if (storageList != null)
        {
            for (int i = Math.Max(0, storageList.Count - n); i < storageList.Count; i++)
            {
                result.Add(storageList[i]);
            }
        }
    }

    public void Foreach(OkxInstType okxInstType, Action<string> foreachFunc)
    {
        if (foreachFunc == null)
            return;

        if (!m_Inst2InstIdListMap.ContainsKey(okxInstType))
            return;

        var instIdList = m_Inst2InstIdListMap[okxInstType];
        foreach(var instId in instIdList)
        {
            if(foreachFunc != null)
            {
                foreachFunc(instId);
            }    
        }
    }

    public void StorageInstId(OkxInstType okxInstType, string instId)
    {
        if(!m_Inst2InstIdListMap.ContainsKey(okxInstType))
        {
            m_Inst2InstIdListMap[okxInstType] = new HashSet<string>();
        }
        var instIdList = m_Inst2InstIdListMap[okxInstType];
        instIdList.Add(instId);
    }

    public override void OnDestroy()
    {
        foreach (var pair in m_CandleDataMap)
        {
            string instId = pair.Key;
            foreach (var pair2 in pair.Value)
            {
                var barSize = pair2.Key;
                var candleDatas = pair2.Value;

                var barSizeName = EnumNameManager.GetName(barSize);
                SerializationManager.Instance.Register(pair.Value, $"quote-cache/{instId}_{barSize}.bin");
            }
        }
    }
}
