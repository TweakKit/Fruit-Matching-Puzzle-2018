using System;

using UnityEngine.Purchasing;

public class IAPManager : PersistentSingleton<IAPManager>, IStoreListener
{
    #region Members

    private IStoreController _storeController;
    private IExtensionProvider _extensionProvider;
    private Action _finishedTransactionCallBack;

    #endregion Members

    #region API Methods

    private void Start()
    {
        if (_storeController == null)
            Init();
    }

    #endregion API Methods

    #region Class Methods

    private void Init()
    {
        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(ProductIDs.ReloadItem, ProductType.Consumable);
        builder.AddProduct(ProductIDs.HintItem, ProductType.Consumable);
        builder.AddProduct(ProductIDs.RecallItem, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);
    }

    public bool BuyProduct(ProductName productName, Action finishedTransactionCallBack)
    {
        _finishedTransactionCallBack = finishedTransactionCallBack;
        string productID = ProductIDs.Products[productName];
        return HandleBuyProduct(productID);
    }

    public string GetProductPrice(ProductName productName)
    {
        string productID = ProductIDs.Products[productName];
        return _storeController.products.WithID(productID).metadata.localizedPriceString;
    }

    public void OnInitialized(IStoreController storeController, IExtensionProvider extensionProvider)
    {
        _storeController = storeController;
        _extensionProvider = extensionProvider;
    }

    public void OnInitializeFailed(InitializationFailureReason error) { }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        foreach (var productName in Enum.GetValues(typeof(ProductName)))
        {
            string productID = ProductIDs.Products[(ProductName)productName];
            if (args.purchasedProduct.definition.id.Equals(productID, StringComparison.Ordinal))
            {
                _finishedTransactionCallBack?.Invoke();
                break;
            }
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason) { }
    public bool IsInitialized() => _storeController != null && _extensionProvider != null;

    private bool HandleBuyProduct(string productId)
    {
        if (IsInitialized())
        {
            Product product = _storeController.products.WithID(productId);
            if (product != null && product.availableToPurchase)
            {
                _storeController.InitiatePurchase(product);
                return true;
            }
            else return false;
        }
        else return false;
    }

    #endregion Class Methods
}