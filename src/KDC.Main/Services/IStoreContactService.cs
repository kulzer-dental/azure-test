using KDC.Main.Config;

namespace KDC.Main.Services
{
    public interface IStoreContactService
    {
        string GetSupportEmail(string storeCode);
        StoreContact? GetStoreContacts(string storeCode);
    }
}
