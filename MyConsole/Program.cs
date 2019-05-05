using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using MyConsole;
using MyWalletLib;
using MyWalletLib.Models;

namespace MyConsole
{
    public class AuthorizationInterceptor : IInterceptor
    {
        private readonly IContext _context;

        public AuthorizationInterceptor(IContext context)
        {
            _context = context;
        }

        private static IEnumerable<AuthorizedAttribute> GetAuthorizedAttribute(IInvocation invocation)
        {
            return Attribute.GetCustomAttributes(invocation.MethodInvocationTarget, typeof(AuthorizedAttribute))
                .Cast<AuthorizedAttribute>();
        }

        public void Intercept(IInvocation invocation)
        {
            var authorizedAttributes = GetAuthorizedAttribute(invocation);
            if (authorizedAttributes.Any())
            {
                var currentUser = _context.GetCurrentUser();
                if (!authorizedAttributes.Select(a => a.UserType).Contains(currentUser.UserType))
                {
                    throw new UnauthorizedAccessException(
                        $"{currentUser.Id} is {currentUser.UserType} without authorization for {invocation.TargetType.FullName}.{invocation.Method.Name}()");
                }
            }

            invocation.Proceed();
        }
    }
}

class Program
{
    public static IContainer _container;

    static void Main(string[] args)
    {
        RegisterContainer();
        var wallet = _container.Resolve<IWallet>();


        //Console.WriteLine(wallet.CreateGuid("Joey", 91));
        //Console.WriteLine(wallet.CreateGuid("Joey", 91));
        //Console.WriteLine(wallet.CreateGuid("Tom", 66));
        //Console.WriteLine(wallet.CreateGuid("Joey", 91));

        //var wallet = new LogParametersDecorator( new Wallet(new FakeWalletRepo(), new FakeBankingAccount(), new FakeFee()),new FakeLogger());
        //var wallet = new Wallet(new FakeWalletRepo(), new FakeBankingAccount(), new FakeFee(), new FakeLogger());
        //wallet.Withdraw("joey", 1000m, "919");

        wallet.StoreValue("919", 80, "joey");
    }

    private static void RegisterContainer()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.RegisterType<FakeLogger>().As<ILogger>();
        containerBuilder.RegisterType<FakeWalletRepo>().As<IWalletRepo>();
        containerBuilder.RegisterType<FakeBankingAccount>().As<IBankingAccount>();
        containerBuilder.RegisterType<FakeFee>().As<IFee>();
        containerBuilder.RegisterType<FakeNotify>().As<INotification>();
        containerBuilder.RegisterType<TransactionScopeAdapter>().As<ITransactionScope>();

        containerBuilder
            .RegisterType<Wallet>()
            .As<IWallet>()
            .EnableInterfaceInterceptors()
            .InterceptedBy(
                typeof(LogInterceptor),
                typeof(AuthorizationInterceptor),
                typeof(CacheInterceptor),
                typeof(TransactionInterceptor));

        containerBuilder.RegisterType<MyContext>().As<IContext>();
        containerBuilder.RegisterType<MemoryCacheProvider>().As<ICacheProvider>();
        containerBuilder.RegisterType<LogInterceptor>();
        containerBuilder.RegisterType<AuthorizationInterceptor>();
        containerBuilder.RegisterType<CacheInterceptor>();
        containerBuilder.RegisterType<TransactionInterceptor>();

        //containerBuilder.RegisterDecorator<LogParametersDecorator, IWallet>();
        _container = containerBuilder.Build();
    }
}

public class FakeNotify:INotification
{
    public void Push(Role role, string message)
    {
        Console.WriteLine($"{role}, transaction ex:{message}");
    }
}

public class TransactionInterceptor : IInterceptor
{
    private readonly INotification _notify;

    public TransactionInterceptor(INotification notify)
    {
        _notify = notify;
    }

    public void Intercept(IInvocation invocation)
    {
        var transactionAttribute = GetTransactionAttribute(invocation);
        if (transactionAttribute == null)
        {
            invocation.Proceed();
        }
        else
        {
            using (var transactionScope = Program._container.Resolve<ITransactionScope>())
            {
                try
                {
                    invocation.Proceed();
                    transactionScope.Complete();
                }
                catch (TransactionAbortedException ex)
                {
                    _notify.Push(transactionAttribute.Role, ex.ToString());
                    throw;
                }
            }
        }
    }

    private static TransactionAttribute GetTransactionAttribute(IInvocation invocation)
    {
        return Attribute.GetCustomAttribute(invocation.MethodInvocationTarget, typeof(TransactionAttribute)) as
            TransactionAttribute;
    }
}

public interface INotification
{
    void Push(Role role, string message);
}

public interface ITransactionScope : IDisposable
{
    void Complete();
    void Dispose();
}

public class TransactionScopeAdapter : ITransactionScope
{
    private readonly TransactionScope _transactionScope;

    public TransactionScopeAdapter()
    {
        _transactionScope = new TransactionScope();
    }

    public void Complete()
    {
        Console.WriteLine("transaction complete");
        _transactionScope.Complete();
    }

    public void Dispose()
    {
        Console.WriteLine("transaction dispose");
        _transactionScope.Dispose();
    }
}
public class MyContext : IContext
{
    public Account GetCurrentUser()
    {
        return new Account() {Id = "Kyo", UserType = UserType.Guest};
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

public class CacheInterceptor : IInterceptor
{
    private readonly ICacheProvider _cache;

    public CacheInterceptor(ICacheProvider cache)
    {
        _cache = cache;
    }

    public CacheResultAttribute GetCacheResultAttribute(IInvocation invocation)
    {
        return Attribute.GetCustomAttribute(
                invocation.MethodInvocationTarget,
                typeof(CacheResultAttribute)
            )
            as CacheResultAttribute;
    }

    public string GetInvocationSignature(IInvocation invocation)
    {
        return
            $"{invocation.TargetType.FullName}-{invocation.Method.Name}-{String.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())}";
    }

    public void Intercept(IInvocation invocation)
    {
        var cacheAttr = GetCacheResultAttribute(invocation);

        if (cacheAttr == null)
        {
            invocation.Proceed();
            return;
        }

        string key = GetInvocationSignature(invocation);

        if (_cache.Contains(key))
        {
            invocation.ReturnValue = _cache.Get(key);
            return;
        }

        invocation.Proceed();
        var result = invocation.ReturnValue;

        if (result != null)
        {
            _cache.Put(key, result, cacheAttr.Duration);
        }
    }
}

public interface ICacheProvider
{
    object Get(string key);

    void Put(string key, object value, int duration);

    bool Contains(string key);
}

public class MemoryCacheProvider : ICacheProvider
{
    public object Get(string key)
    {
        return MemoryCache.Default[key];
    }

    public void Put(string key, object value, int duration)
    {
        if (duration <= 0)
            throw new ArgumentException("Duration cannot be less or equal to zero", nameof(duration));

        var policy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTime.Now.AddMilliseconds(duration)
        };

        MemoryCache.Default.Set(key, value, policy);
    }

    public bool Contains(string key)
    {
        return MemoryCache.Default[key] != null;
    }
}
