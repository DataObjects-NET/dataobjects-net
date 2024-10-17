using System;
using System.Transactions;
using Xtensive.Orm.Configuration;
using Xtensive.Orm.Reprocessing.Configuration;

namespace Xtensive.Orm.Reprocessing
{
  /// <summary>
  /// Extends <see cref="Domain"/>.
  /// </summary>
  public static class DomainExtensions
  {
    /// <summary>
    /// Executes a reprocessable task.
    /// </summary>
    /// <param name="domain">The domain of the task.</param>
    /// <param name="isolationLevel">Isolation level of the task.</param>
    /// <param name="transactionOpenMode">Transaction open mode of the task.</param>
    /// <param name="strategy">Execute strategy of the task.</param>
    /// <param name="action">Task with <see cref="Void"/> result.</param>
    public static void Execute(
      this Domain domain,
      Action<Session> action,
      IExecuteActionStrategy strategy = null,
      IsolationLevel isolationLevel = IsolationLevel.Unspecified,
      TransactionOpenMode? transactionOpenMode = null)
    {
      ExecuteInternal(domain, isolationLevel, transactionOpenMode, strategy, action);
    }

    /// <summary>
    /// Executes a reprocessable task.
    /// </summary>
    /// <param name="domain">The domain of the task.</param>
    /// <param name="isolationLevel">Isolation level of the task.</param>
    /// <param name="transactionOpenMode">Transaction open mode of the task.</param>
    /// <param name="strategy">Execute strategy of the task.</param>
    /// <param name="func">Task with T result.</param>
    /// <typeparam name="T">Return type of the task.</typeparam>
    /// <returns>The task result.</returns>
    public static T Execute<T>(
      this Domain domain,
      Func<Session, T> func,
      IExecuteActionStrategy strategy = null,
      IsolationLevel isolationLevel = IsolationLevel.Unspecified,
      TransactionOpenMode? transactionOpenMode = null)
    {
      return ExecuteInternal(domain, isolationLevel, transactionOpenMode, strategy, func);
    }

    /// <summary>
    /// Gets the reprocessing configuration.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration GetReprocessingConfiguration(this Domain domain)
    {
      var result = domain.Extensions.Get<ReprocessingConfiguration>();
      if (result==null) {
        result = ReprocessingConfiguration.Load();
        domain.Extensions.Set(result);
      }
      return result;
    }

    /// <summary>
    /// Gets the reprocessing configuration from given <paramref name="configuration"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration"><see cref="System.Configuration.Configuration"/></param>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration GetReprocessingConfiguration(this Domain domain, System.Configuration.Configuration configuration)
    {
      var result = domain.Extensions.Get<ReprocessingConfiguration>();
      if (result==null) {
        result = ReprocessingConfiguration.Load(configuration);
        domain.Extensions.Set(result);
      }
      return result;
    }

    /// <summary>
    /// Gets the reprocessing configuration from given <paramref name="configuration"/> and <paramref name="sectionName"/>.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="configuration">Non-default application configuration<see cref="System.Configuration.Configuration"/>.</param>
    /// <param name="sectionName">Non-default section.</param>
    /// <returns>The reprocessing configuration.</returns>
    public static ReprocessingConfiguration GetReprocessingConfiguration(this Domain domain, System.Configuration.Configuration configuration, string sectionName)
    {
      var result = domain.Extensions.Get<ReprocessingConfiguration>();
      if (result==null) {
        result = ReprocessingConfiguration.Load(configuration, sectionName);
        domain.Extensions.Set(result);
      }
      return result;
    }

    /// <summary>
    /// Starts <see cref="IExecuteConfiguration"/> flow
    /// and provides <see cref="IsolationLevel"/> to use.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="isolationLevel">Isolation level to use.</param>
    /// <returns>Created <see cref="IExecuteConfiguration"/>.</returns>
    public static IExecuteConfiguration WithIsolationLevel(this Domain domain, IsolationLevel isolationLevel)
    {
      return new ExecuteConfiguration(domain).WithIsolationLevel(isolationLevel);
    }

    /// <summary>
    /// Starts <see cref="IExecuteConfiguration"/> flow
    /// and provides <see cref="IExecuteActionStrategy"/> to use.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="strategy">Strategy to use.</param>
    /// <returns>Created <see cref="IExecuteConfiguration"/>.</returns>
    public static IExecuteConfiguration WithStrategy(this Domain domain, IExecuteActionStrategy strategy)
    {
      return new ExecuteConfiguration(domain).WithStrategy(strategy);
    }

    /// <summary>
    /// Starts <see cref="IExecuteConfiguration"/> flow
    /// and provides <see cref="TransactionOpenMode"/> to use.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="transactionOpenMode">Transaction open mode to use.</param>
    /// <returns>Created <see cref="IExecuteConfiguration"/>.</returns>
    public static IExecuteConfiguration WithTransactionOpenMode(this Domain domain, TransactionOpenMode transactionOpenMode)
    {
      return new ExecuteConfiguration(domain).WithTransactionOpenMode(transactionOpenMode);
    }

    /// <summary>
    /// Starts <see cref="IExecuteConfiguration"/> flow
    /// and provides <see cref="Session"/> to use.
    /// </summary>
    /// <param name="domain">The domain.</param>
    /// <param name="session">The external session to use.</param>
    /// <returns>Created <see cref="IExecuteConfiguration"/>.</returns>
    public static IExecuteConfiguration WithSession(this Domain domain, Session session)
    {
      return new ExecuteConfiguration(domain).WithSession(session);
    }

    #region Non-public methods

    internal static void ExecuteInternal(
      this Domain domain,
      IsolationLevel isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      IExecuteActionStrategy strategy,
      Action<Session> action)
    {
      _ = ExecuteInternal<object>(
        domain,
        isolationLevel,
        transactionOpenMode,
        strategy,
        sessionInstance => {
          action(sessionInstance);
          return null;
        });
    }

    internal static void ExecuteInternal(
      this Domain domain,
      Session session,
      IsolationLevel isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      IExecuteActionStrategy strategy,
      Action<Session> action)
    {
      _ = ExecuteInternal<object>(
        domain,
        session,
        isolationLevel,
        transactionOpenMode,
        strategy,
        sessionInstance => {
          action(sessionInstance);
          return null;
        });
    }

    internal static T ExecuteInternal<T>(
      this Domain domain,
      IsolationLevel isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      IExecuteActionStrategy strategy,
      Func<Session, T> func)
    {
      var config = domain.Configuration.ExtensionConfigurations.Get<ReprocessingConfiguration>()
        ?? domain.GetReprocessingConfiguration();
      if (strategy == null) {
        strategy = ExecuteActionStrategy.GetSingleton(config.DefaultExecuteStrategy);
      }
      if (transactionOpenMode == null) {
        transactionOpenMode = config.DefaultTransactionOpenMode;
      }
      return strategy.Execute(new ExecutionContext<T>(domain, isolationLevel, transactionOpenMode.Value, func));
    }

    internal static T ExecuteInternal<T>(
      this Domain domain,
      Session session,
      IsolationLevel isolationLevel,
      TransactionOpenMode? transactionOpenMode,
      IExecuteActionStrategy strategy,
      Func<Session, T> func)
    {
      var config = domain.Configuration.ExtensionConfigurations.Get<ReprocessingConfiguration>()
        ?? domain.GetReprocessingConfiguration();
      if (strategy == null) {
        strategy = ExecuteActionStrategy.GetSingleton(config.DefaultExecuteStrategy);
      }
      if (transactionOpenMode == null) {
        transactionOpenMode = config.DefaultTransactionOpenMode;
      }
      if (session.IsDisposed) {
        throw new ObjectDisposedException(nameof(session));
      }
      return strategy.Execute(new ExecutionContext<T>(domain, session, isolationLevel, transactionOpenMode.Value, func));
    }

    #endregion
  }
}