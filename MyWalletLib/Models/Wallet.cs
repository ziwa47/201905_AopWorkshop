namespace MyWalletLib.Models
{
    public class Wallet
    {
        private readonly IWalletRepo _walletRepo;
        private readonly IBankingAccount _bankingAccount;
        private readonly IFee _fee;

        public Wallet(IWalletRepo walletRepo, IBankingAccount bankingAccount, IFee fee)
        {
            _walletRepo = walletRepo;
            _bankingAccount = bankingAccount;
            _fee = fee;
        }

        public void Withdraw(string account, decimal amount, string bankingAccount)
        {
            _walletRepo.UpdateDelta(account, amount * -1);

            var fee = _fee.Get(bankingAccount);

            _bankingAccount.Saving(bankingAccount, amount - fee);
        }

        public void StoreValue(string bankingAccount, decimal amount, string account)
        {
            _bankingAccount.Withdraw(bankingAccount, amount);
            _walletRepo.UpdateDelta(account, amount);
        }
    }
}