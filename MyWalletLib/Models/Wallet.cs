using System;
using System.Threading;
using Autofac.Extras.DynamicProxy;

namespace MyWalletLib.Models
{
    public interface IWallet
    {
        void Withdraw(string account, decimal amount, string bankingAccount);
        void StoreValue(string bankingAccount, decimal amount, string account);
        string CreateGuid(string account, int token);
    }

    public class Wallet : IWallet
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

        [LogParameters]
        [Authorized(UserType.VIP)]
        [Authorized(UserType.NormalUser)] 
        public void Withdraw(string account, decimal amount, string bankingAccount)
        {
            _walletRepo.UpdateDelta(account, amount * -1);
            var fee = _fee.Get(bankingAccount);

            _bankingAccount.Saving(bankingAccount, amount - fee);
        }

        [LogParameters]
        public void StoreValue(string bankingAccount, decimal amount, string account)
        {
            _bankingAccount.Withdraw(bankingAccount, amount);
            _walletRepo.UpdateDelta(account, amount);
        }

        [CacheResult(Duration = 1000)]
        public string CreateGuid(string account, int token)
        {
            Console.WriteLine($"sleep 1.5 seconds, account:{account}, token:{token}");
            Thread.Sleep(1500);
            return Guid.NewGuid().ToString("N");
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class AuthorizedAttribute : Attribute
    {
        public UserType UserType { get; }

        public AuthorizedAttribute(UserType userType)
        {
            UserType = userType;
        }
    }

    public enum UserType
    {
        VIP,
        Guest,
        NormalUser
    }
}

public class Member
{
    public bool Register(string name, int age)
    {
        return true;
    }
}

