using System;

[Serializable]
public struct StoreProduct
{
    public NormalButton button;
    public ProductInfo productInfo;
}

[Serializable]
public struct ProductInfo
{
    public ProductName productName;
    public int minGold;
    public int maxGold;
    public int hintTurns;
    public int rewindTurns;
    public int reloadTurns;
    public bool removeAds;
    public float price;
}