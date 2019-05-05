using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWalletLib;
using MyWalletLib.Models;

namespace MyConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var wallet = new LogParametersDecorator( new Wallet(new FakeWalletRepo(), new FakeBankingAccount(), new FakeFee()),new FakeLogger());
            //var wallet = new Wallet(new FakeWalletRepo(), new FakeBankingAccount(), new FakeFee(), new FakeLogger());
            wallet.Withdraw("joey", 1000m, "919");

            wallet.StoreValue("919",80,"joey");
        }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeFee : IFee
    {
        public decimal Get(string bankingAccount)
        {
            return 5;
        }
    }

    internal class FakeBankingAccount : IBankingAccount
    {
        public void Saving(string bankingAccount, decimal amount)
        {
            Console.WriteLine($"Saving({bankingAccount},{amount})");
        }

        public void Withdraw(string bankingAccount, decimal amount)
        {
            Console.WriteLine($"Withdraw({bankingAccount},{amount})");
        }
    }

    internal class FakeWalletRepo : IWalletRepo
    {
        public void UpdateDelta(string account, decimal amount)
        {
            Console.WriteLine($"UpdateDelta({account},{amount})");
        }
    }
}