using Castle.DynamicProxy;
using MyWalletLib;
using MyWalletLib.Models;
using System;
using System.Linq;

namespace MyConsole
{
    public class LogInterceptor : IInterceptor
    {
        private readonly IContext _context;
        private readonly ILogger _logger;

        public LogInterceptor(IContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        public void Intercept(IInvocation invocation)
        {
            if (GetLogParametersAttribute(invocation) != null)
            {
                LogParameters(invocation);
            }

            invocation.Proceed();
        }

        private static LogParametersAttribute GetLogParametersAttribute(IInvocation invocation)
        {
            return Attribute.GetCustomAttribute(invocation.MethodInvocationTarget, typeof(LogParametersAttribute)) as
                LogParametersAttribute;
        }

        private void LogParameters(IInvocation invocation)
        {
            var signature = GetInvocationSignature(invocation);
            var accountId = _context.GetCurrentUser().Id;
            var message = $"{accountId} invoke {signature} when {DateTime.Now}";
            _logger.Info(message);
        }

        public string GetInvocationSignature(IInvocation invocation)
        {
            return
                $"{invocation.TargetType.FullName}-{invocation.Method.Name}-{String.Join("-", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray())}";
        }
    }
}