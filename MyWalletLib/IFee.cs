namespace MyWalletLib
{
    public interface IFee
    {
        decimal Get(string bankingAccount);
    }
}