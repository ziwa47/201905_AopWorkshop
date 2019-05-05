namespace MyWalletLib.Models
{
    public class LogParametersDecorator : IWallet
    {
        private readonly IWallet _wallet;
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
            _wallet.Withdraw(account, amount, bankingAccount);
        }

        public void StoreValue(string bankingAccount, decimal amount, string account)
        {
            LogParametersWithStoredValue(bankingAccount, amount, account);
            _wallet.StoreValue(bankingAccount, amount, account);
        }
    }
}