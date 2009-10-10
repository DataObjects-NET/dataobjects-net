// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal abstract class CommandProcessor : IDisposable
  {
    protected const string DefaultParameterNamePrefix = "p";

    protected readonly DomainHandler domainHandler;
    protected readonly Driver driver;
    protected readonly SqlConnection connection;
    protected readonly CommandPartFactory factory;
    
    protected Queue<SqlQueryTask> queryTasks = new Queue<SqlQueryTask>();
    protected Queue<SqlPersistTask> persistTasks = new Queue<SqlPersistTask>();

    protected int reenterCount;
    protected Command activeCommand;
    protected DbTransaction transaction;

    public DbTransaction Transaction
    {
      get { return transaction; }
      set {
        if (reenterCount > 0 && value!=transaction)
          throw new InvalidOperationException();
        transaction = value;
      }
    }

    #region Low-level execute methods

    public object ExecuteScalarQuickly(SqlScalarTask task)
    {
      AllocateCommand();
      try {
        var part = task.Request.Compile(domainHandler).GetCommandText();
        activeCommand.AddPart(part);
        return activeCommand.ExecuteScalar();
      }
      finally {
        DisposeCommand();
      }
    }
    
    public void ExecutePersistQuickly(SqlPersistTask task)
    {
      AllocateCommand();
      try {
        var part = factory.CreatePersistCommandPart(task, DefaultParameterNamePrefix);
        activeCommand.AddPart(part);
        activeCommand.ExecuteNonQuery();
      }
      finally {
        DisposeCommand();
      }
    }

    public DbDataReader ExecuteQueryQuickly(SqlQueryTask task)
    {
      AllocateCommand();
      try {
        var part = factory.CreateQueryCommandPart(task, DefaultParameterNamePrefix);
        activeCommand.AddPart(part);
        return activeCommand.ExecuteReader();
      }
      finally {
        DisposeCommand();
      }
    }

    #endregion

    public abstract void ExecuteRequests(bool allowPartialExecution);
    public abstract IEnumerator<Tuple> ExecuteRequestsWithReader(SqlQueryRequest request);
    
    public void RegisterTask(SqlQueryTask task)
    {
      queryTasks.Enqueue(task);
    }

    public void RegisterTask(SqlPersistTask task)
    {
      persistTasks.Enqueue(task);
    }
    
    public void ClearTasks()
    {
      queryTasks.Clear();
      persistTasks.Clear();
    }

    protected IEnumerator<Tuple> RunTupleReader(DbDataReader reader, TupleDescriptor descriptor)
    {
      var accessor = domainHandler.GetDataReaderAccessor(descriptor);
      using (reader) {
        while (driver.ReadRow(reader)) {
          var tuple = Tuple.Create(descriptor);
          accessor.Read(reader, tuple);
          yield return tuple;
        }
      }
    }

    protected void AllocateCommand()
    {
      if (activeCommand!=null)
        reenterCount++;
      else 
        activeCommand = CreateCommand();
    }

    protected void DisposeCommand()
    {
      activeCommand.DisposeSafely();
      if (reenterCount > 0) {
        reenterCount--;
        activeCommand = CreateCommand();
      }
      else
        activeCommand = null;
    }


    public void Dispose()
    {
      DisposeCommand();
    }

    private Command CreateCommand()
    {
      if (transaction==null)
        throw new InvalidOperationException();
      var nativeCommand = connection.CreateCommand();
      nativeCommand.Transaction = transaction;
      return new Command(driver, nativeCommand);
    }
    
    // Constructors

    protected CommandProcessor(DomainHandler domainHandler, SqlConnection connection)
    {
      this.domainHandler = domainHandler;
      this.connection = connection;
      driver = domainHandler.Driver;
      factory = new CommandPartFactory(domainHandler, connection);
    }
  }
}