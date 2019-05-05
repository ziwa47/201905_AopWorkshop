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
        public void withdrawal_from_wallet_to_banking_account_successfully()
        {
            var walletRepo = Substitute.For<IWalletRepo>();
            var bankingAccount = Substitute.For<IBankingAccount>();

            var wallet = new Wallet(walletRepo, bankingAccount, _fee);
            _fee.Get("919").ReturnsForAnyArgs(5m);

            wallet.Withdraw("joey", 1000m, "919");

            walletRepo.Received(1).UpdateDelta("joey", -1000m);
            bankingAccount.Received().Saving("919", 995);
        }

        [Test]
        public void storedValue_from_banking_to_wallet()
        {
            var walletRepo = Substitute.For<IWalletRepo>();
            var bankingAccount = Substitute.For<IBankingAccount>();

            var wallet = new Wallet(walletRepo, bankingAccount, _fee);

            wallet.StoreValue("919", 1000m, "joey");

            bankingAccount.Received(1).Withdraw("919", 1000m);
            walletRepo.Received(1).UpdateDelta("joey", 1000m);
        }
    }
}