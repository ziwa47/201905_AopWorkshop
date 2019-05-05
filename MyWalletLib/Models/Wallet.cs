namespace MyWalletLib.Models
{
    public interface IWallet
    {
        void Withdraw(string account, decimal amount, string bankingAccount);
        void StoreValue(string bankingAccount, decimal amount, string account);
    }

    public class LogParametersDecorator:IWallet
    {
        private IWallet _wallet;
        private readonly ILogger _log;

        public LogParametersDecorator(IWallet wallet, ILogger log)
        {
            _wallet = wallet;
            _log = log;
        }

        private void LogParametersWithWithdraw(string account, decimal amount, string bankingAccount)
        {
            _log.Info(
                $"{nameof(account)} - {account}, {nameof(amount)} - {amount}, {nameof(bankingAccount)} - {bankingAccount}");
        }

        private void LogParametersWithStoredValue(string bankingAccount, decimal amount, string account)
        {
            _log.Info(
                $"{nameof(account)} - {account}, {nameof(amount)} - {amount}, {nameof(bankingAccount)} - {bankingAccount}");
        }

        public void Withdraw(string account, decimal amount, string bankingAccount)
        {
            LogParametersWithWithdraw(account, amount, bankingAccount);
            _wallet.Withdraw(account,amount,bankingAccount);
        }

        public void StoreValue(string bankingAccount, decimal amount, string account)
        {
            LogParametersWithStoredValue(bankingAccount,amount,account);
            _wallet.StoreValue(bankingAccount,amount,account);
        }
    }

    public class Wallet : IWallet
    {
        private readonly IWalletRepo _walletRepo;
        private readonly IBankingAccount _bankingAccount;
        private readonly IFee _fee;
        private ILogger _log;

        public Wallet(IWalletRepo walletRepo, IBankingAccount bankingAccount, IFee fee, ILogger log)
        {
            _walletRepo = walletRepo;
            _bankingAccount = bankingAccount;
            _fee = fee;
            _log = log;
        }

        public void Withdraw(string account, decimal amount, string bankingAccount)
        {
            //_logParametersDecorator.LogParametersWithWithdraw(account, amount, bankingAccount);
            _walletRepo.UpdateDelta(account, amount * -1);
            var fee = _fee.Get(bankingAccount);

            _bankingAccount.Saving(bankingAccount, amount - fee);
        }

        public void StoreValue(string bankingAccount, decimal amount, string account)
        {
            //_logParametersDecorator.LogParametersWithStoredValue(bankingAccount, amount, account);
            _bankingAccount.Withdraw(bankingAccount, amount);
            _walletRepo.UpdateDelta(account, amount);
        }
    }

    public interface ILogger
    {
        void Info(string message);
    }
}