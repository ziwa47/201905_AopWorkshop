namespace MyWalletLib
{
    public interface IWalletRepo
    {
        void UpdateDelta(string account, decimal amount);
    }
}