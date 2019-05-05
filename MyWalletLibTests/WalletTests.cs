using System;
using Castle.DynamicProxy;
using MyConsole;
using MyWalletLib;
using MyWalletLib.Models;
using NSubstitute;
using NUnit.Framework;

namespace MyWalletLibTests
{
    [TestFixture]
    public class WalletTests
    {
        private IFee _fee;

        [SetUp]
        public void Setup()
        {
            _fee = Substitute.For<IFee>();
        }

        [Test]
        public void with_attribute_should_log()
        {
            var decoratee = new LogDecoratee();
            var logger = Substitute.For<ILogger>();
            var context = Substitute.For<IContext>();
            context.GetCurrentUser().ReturnsForAnyArgs(new Account() { Id = "JoeyChen" });

            var logInterceptor = new LogInterceptor(context, logger);

            var sut = new ProxyGenerator().CreateInterfaceProxyWithTargetInterface<IMember>(decoratee, logInterceptor);

            sut.Register("joey", 39);

            logger.Received(1)
                .Info(Arg.Is<string>(m => m.Contains("joey") && m.Contains("39") && m.Contains("JoeyChen")));
        }


        //[Test]
        //public void withdrawal_from_wallet_to_banking_account_successfully()
        //{
        //    var walletRepo = Substitute.For<IWalletRepo>();
        //    var bankingAccount = Substitute.For<IBankingAccount>();

        //    var wallet = new Wallet(walletRepo, bankingAccount, _fee);
        //    _fee.Get("919").ReturnsForAnyArgs(5m);

        //    wallet.Withdraw("joey", 1000m, "919");

        //    walletRepo.Received(1).UpdateDelta("joey", -1000m);
        //    bankingAccount.Received().Saving("919", 995);
        //}

        //[Test]
        //public void storedValue_from_banking_to_wallet()
        //{
        //    var walletRepo = Substitute.For<IWalletRepo>();
        //    var bankingAccount = Substitute.For<IBankingAccount>();

        //    var wallet = new Wallet(walletRepo, bankingAccount, _fee);

        //    wallet.StoreValue("919", 1000m, "joey");

        //    bankingAccount.Received(1).Withdraw("919", 1000m);
        //    walletRepo.Received(1).UpdateDelta("joey", 1000m);
        //}
    }

    public interface IMember
    {
        bool Register(string name, int age);

        void StoredValue(decimal amount);
    }

    public class LogDecoratee : IMember
    {
        private bool _registerResult;

        internal void SetRegisterResult(bool expected)
        {
            _registerResult = expected;
        }

        [LogParameters]
        public bool Register(string name, int age)
        {
            return _registerResult;
        }

        public void StoredValue(decimal amount)
        {
            Console.WriteLine($"{nameof(StoredValue)}({amount})");
        }
    }
}