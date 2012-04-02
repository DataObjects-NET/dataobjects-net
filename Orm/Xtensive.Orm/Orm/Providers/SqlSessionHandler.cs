// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2008.05.20

using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.IoC;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Rse.Providers;
using Xtensive.Parameters;
using Xtensive.Sql;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// <see cref="Session"/>-level handler for SQL storages.
  /// </summary>
  public partial class SqlSessionHandler : SessionHandler,
    IProviderExecutor,
    IDirectSqlService
  {
    private StorageDriver driver;
    private DomainHandler domainHandler;
    private SqlConnection connection;
    private CommandProcessor commandProcessor;

    /// <inheritdoc/>
    public override bool TransactionIsStarted { get { return connection.ActiveTransaction!=null; } }

    /// <summary>
    /// Gets <see cref="SqlConnection"/> associated with current instance.
    /// </summary>
    public SqlConnection Connection
    {
      get
      {
        EnsureConnectionIsOpen();
        return connection;
      }
    }

    /// <summary>
    /// Gets <see cref="CommandFactory"/> associated with current instance.
    /// </summary>
    public CommandFactory CommandFactory
    {
      get
      {
        EnsureConnectionIsOpen();
        return commandProcessor.Factory;
      }
    }

    /// <inheritdoc/>
    public override void SetCommandTimeout(int? commandTimeout)
    {
      connection.CommandTimeout = commandTimeout;
    }

    #region Transaction control methods

    /// <inheritdoc/>
    public override void BeginTransaction(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.BeginTransaction(
        Session, connection, IsolationLevelConverter.Convert(transaction.IsolationLevel));
    }

    /// <inheritdoc/>
    public override void CreateSavepoint(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.MakeSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void RollbackToSavepoint(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.RollbackToSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void ReleaseSavepoint(Transaction transaction)
    {
      EnsureConnectionIsOpen();
      driver.ReleaseSavepoint(Session, connection, transaction.SavepointName);
    }

    /// <inheritdoc/>
    public override void CommitTransaction(Transaction transaction)
    {
      if (connection.ActiveTransaction!=null)
        driver.CommitTransaction(Session, connection);
      EndNativeTransaction();
    }

    /// <inheritdoc/>
    public override void RollbackTransaction(Transaction transaction)
    {
      if (connection.ActiveTransaction!=null)
        driver.RollbackTransaction(Session, connection);
      EndNativeTransaction();
    }

    #endregion
    
    #region Private / internal members

    private void RegisterQueryTask(QueryTask task, QueryRequest request)
    {
      task.Result = new List<Tuple>();
      commandProcessor.RegisterTask(new SqlLoadTask(request, task.Result, task.ParameterContext));
    }

    private void EnsureConnectionIsOpen()
    {
      driver.EnsureConnectionIsOpen(Session, connection);
    }

    private void EndNativeTransaction()
    {
      commandProcessor.ClearTasks();
    }

    #endregion

    /// <inheritdoc/>
    public override void ExecuteQueryTasks(IEnumerable<QueryTask> queryTasks, bool allowPartialExecution)
    {
      EnsureConnectionIsOpen();

      var nonBatchedTasks = new List<QueryTask>();
      foreach (var task in queryTasks) {
        var sqlProvider = task.DataSource as SqlProvider;
        if (sqlProvider!=null && sqlProvider.Request.CheckOptions(QueryRequestOptions.AllowOptimization))
          RegisterQueryTask(task, sqlProvider.Request);
        else
          nonBatchedTasks.Add(task);
      }

      if (nonBatchedTasks.Count==0) {
        commandProcessor.ExecuteTasks(allowPartialExecution);
        return;
      }

      commandProcessor.ExecuteTasks();
      foreach (var task in nonBatchedTasks) {
        using (new EnumerationContext(Session).Activate())
        using (task.ParameterContext.ActivateSafely())
          task.Result = task.DataSource.ToList();
      }
    }

    /// <inheritdoc/>
    protected override void AddBaseServiceRegistrations(List<ServiceRegistration> registrations)
    {
      registrations.Add(new ServiceRegistration(typeof (ISqlExecutor), new SqlExecutor(driver, connection, Session)));
      registrations.Add(new ServiceRegistration(typeof (IDirectSqlService), this));
      registrations.Add(new ServiceRegistration(typeof (IProviderExecutor), this));
    }

    /// <inheritdoc/>
    public override void Persist(EntityChangeRegistry registry, bool allowPartialExecution)
    {
      domainHandler.Persister.Persist(registry, commandProcessor);
      commandProcessor.ExecuteTasks(allowPartialExecution);
    }

    /// <inheritdoc/>
    protected override void Initialize()
    {
      base.Initialize();

      domainHandler = Handlers.DomainHandler;
      driver = Handlers.StorageDriver;

      connection = driver.CreateConnection(Session);
      commandProcessor = domainHandler.CommandProcessorFactory.CreateCommandProcessor(Session, connection);
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      driver.CloseConnection(Session, connection);
    }
  }
}
