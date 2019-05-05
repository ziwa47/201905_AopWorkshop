namespace MyWalletLib
{
    public interface IBankingAccount
    {
        void Saving(string bankingAccount, decimal amount);
        void Withdraw(string bankingAccount, decimal amount);
    }
}