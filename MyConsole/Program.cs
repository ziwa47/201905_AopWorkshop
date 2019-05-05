using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.DynamicProxy;
using MyWalletLib;
using MyWalletLib.Models;

namespace MyConsole
{
    class Program
    {
        private static IContainer _container;

        static void Main(string[] args)
        {
            RegisterContainer();
            var wallet = _container.Resolve<IWallet>();
            //var wallet = new LogParametersDecorator( new Wallet(new FakeWalletRepo(), new FakeBankingAccount(), new FakeFee()),new FakeLogger());
            //var wallet = new Wallet(new FakeWalletRepo(), new FakeBankingAccount(), new FakeFee(), new FakeLogger());
            wallet.Withdraw("joey", 1000m, "919");

            wallet.StoreValue("919",80,"joey");
        }

        private static void RegisterContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<FakeLogger>().As<ILogger>();
            containerBuilder.RegisterType<FakeWalletRepo>().As<IWalletRepo>();
            containerBuilder.RegisterType<FakeBankingAccount>().As<IBankingAccount>();
            containerBuilder.RegisterType<FakeFee>().As<IFee>();

            containerBuilder.RegisterType<Wallet>().As<IWallet>()
                .EnableInterfaceInterceptors();

            containerBuilder.RegisterType<MyContext>().As<IContext>();
            containerBuilder.RegisterType<LogInterceptor>();
            //containerBuilder.RegisterDecorator<LogParametersDecorator, IWallet>();
            _container = containerBuilder.Build();
        }
    }

    public class MyContext:IContext
    {
        public Account GetCurrentUser()
        {
            return new Account() { Id = "Kyo" };
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