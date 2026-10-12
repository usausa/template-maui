namespace Template.MobileApp.Models.Sample;

using Template.MobileApp.Components;

//--------------------------------------------------------------------------------
// Data
//--------------------------------------------------------------------------------

public partial class SuicaAccessData : ObservableObject
{
    [ObservableProperty]
    public partial int Balance { get; set; }

    [ObservableProperty]
    public partial int TransactionId { get; set; }
}

public partial class SuicaLogData : ObservableObject
{
    [ObservableProperty]
    public partial byte Terminal { get; set; }

    [ObservableProperty]
    public partial byte Process { get; set; }

    [ObservableProperty]
    public partial DateTime DateTime { get; set; }

    [ObservableProperty]
    public partial int Balance { get; set; }

    [ObservableProperty]
    public partial int TransactionId { get; set; }

    [ObservableProperty]
    public partial int? Difference { get; set; }
}

//--------------------------------------------------------------------------------
// Helper
//--------------------------------------------------------------------------------

// Suica の読み取り (ポーリングで IDm を取り、残高と履歴のブロックを読んで解析する)
public static class SuicaHelper
{
    public static (string Idm, SuicaAccessData Access, List<SuicaLogData> Logs)? ParseTag(INfc nfcF)
    {
        //var idm = nfcF.ExecutePolling(unchecked((short)0x0003));
        var idm = nfcF.ExecutePolling(unchecked((short)0xFFFF));
        if (idm.Length == 0)
        {
            return null;
        }

        var block = new ReadBlock { BlockNo = 0 };
        if (!nfcF.ExecuteReadWoe(idm, 0x008B, block))
        {
            return null;
        }

        var blocks1 = Enumerable.Range(0, 8).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        var blocks2 = Enumerable.Range(8, 8).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        var blocks3 = Enumerable.Range(16, 4).Select(x => new ReadBlock { BlockNo = (byte)x }).ToArray();
        if (!nfcF.ExecuteReadWoe(idm, 0x090F, blocks1) ||
            !nfcF.ExecuteReadWoe(idm, 0x090F, blocks2) ||
            !nfcF.ExecuteReadWoe(idm, 0x090F, blocks3))
        {
            return null;
        }

        var logs = blocks1.Concat(blocks2).Concat(blocks3)
            .Where(static x => SuicaLogic.IsValidLog(x.BlockData))
            .Select(static x => new SuicaLogData
            {
                Terminal = SuicaLogic.ExtractLogTerminal(x.BlockData),
                Process = SuicaLogic.ExtractLogProcess(x.BlockData),
                DateTime = SuicaLogic.ExtractLogDateTime(x.BlockData),
                Balance = SuicaLogic.ExtractLogBalance(x.BlockData),
                TransactionId = SuicaLogic.ExtractLogTransactionId(x.BlockData)
            })
            .ToList();

        // 履歴は新しい順なので、次の行が 1 つ前の履歴
        for (var i = 0; i < logs.Count - 1; i++)
        {
            logs[i].Difference = logs[i].Balance - logs[i + 1].Balance;
        }

        return (
            Convert.ToHexString(idm),
            new SuicaAccessData
            {
                Balance = SuicaLogic.ExtractAccessBalance(block.BlockData),
                TransactionId = SuicaLogic.ExtractAccessTransactionId(block.BlockData)
            },
            logs);
    }
}
