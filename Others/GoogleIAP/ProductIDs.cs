
using System.Collections.Generic;

public static class ProductIDs
{
    #region Members

    public static readonly string ReloadItem = "waterdrop.reload.item";
    public static readonly string HintItem = "waterdrop.hint.item";
    public static readonly string RecallItem = "waterdrop.recall.item";

    public static readonly Dictionary<ProductName, string> Products = new Dictionary<ProductName, string>()
    {
        {ProductName.ReloadItem, ReloadItem},
        {ProductName.HintItem, HintItem},
        {ProductName.RecallItem, RecallItem},
    };

    #endregion Members
}

public enum ProductName
{
    ReloadItem,
    HintItem,
    RecallItem,
    None,
}