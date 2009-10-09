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
using Xtensive.Core.Parameters;
using Xtensive.Core.Tuples;
using Xtensive.Sql;
using Xtensive.Sql.ValueTypeMapping;

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

    protected int spinCount;
    protected Command activeCommand;
    protected DbTransaction transaction;

    public DbTransaction Transaction
    {
      get { return transaction; }
      set {
        if (spinCount > 0 && value!=transaction)
          throw new InvalidOperationException();
        transaction = value;
      }
    }

    #region Low-level execute methods

    public object ExecuteScalarQuickly(SqlScalarTask task)
    {
      AllocateCommand();
      try {
        activeCommand.AddPart(task.Request.Compile(domainHandler).GetCommandText());
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
        activeCommand.AddPart(factory.CreatePersistCommandPart(task, DefaultParameterNamePrefix));
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
        activeCommand.AddPart(factory.CreateQueryCommandPart(task, DefaultParameterNamePrefix));
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
        spinCount++;
      else 
        activeCommand = CreateCommand();
    }

    protected void DisposeCommand()
    {
      activeCommand.DisposeSafely();
      if (spinCount > 0) {
        spinCount--;
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