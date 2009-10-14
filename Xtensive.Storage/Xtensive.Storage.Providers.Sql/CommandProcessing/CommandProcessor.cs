// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.20

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core;
using Xtensive.Core.Disposing;
using Xtensive.Core.Tuples;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql
{
  internal abstract class CommandProcessor : IDisposable
  {
    protected const string DefaultParameterNamePrefix = "p";
    protected readonly CommandPartFactory factory;
    
    protected Queue<SqlQueryTask> queryTasks = new Queue<SqlQueryTask>();
    protected Queue<SqlPersistTask> persistTasks = new Queue<SqlPersistTask>();

    protected int reenterCount;
    protected Command activeCommand;
    protected DbTransaction transaction;

    public SessionHandler SessionHandler { get; private set; }
    public DomainHandler DomainHandler { get; private set; }
    public Driver Driver { get; private set; }
    public SqlConnection Connection { get; private set; }

    public DbTransaction Transaction {
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
        var part = task.Request.Compile(DomainHandler).GetCommandText();
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
      var accessor = DomainHandler.GetDataReaderAccessor(descriptor);
      using (reader) {
        while (Driver.ReadRow(reader)) {
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
      var nativeCommand = Connection.CreateCommand();
      nativeCommand.Transaction = transaction;
      return new Command(this, nativeCommand);
    }

    
    // Constructors

    protected CommandProcessor(SessionHandler sessionHandler)
    {
      ArgumentValidator.EnsureArgumentNotNull(sessionHandler, "sessionHandler");
      SessionHandler = sessionHandler;
      DomainHandler = (DomainHandler) sessionHandler.Handlers.DomainHandler;
      Driver = DomainHandler.Driver;
      Connection = sessionHandler.Connection;
      factory = new CommandPartFactory(DomainHandler, Connection);
    }
  }
}