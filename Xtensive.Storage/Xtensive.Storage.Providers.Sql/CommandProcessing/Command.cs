// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Core.Disposing;

namespace Xtensive.Storage.Providers.Sql
{
  internal sealed class Command : IDisposable
  {
    private readonly CommandProcessor processor;
    private readonly DbCommand underlyingCommand;
    private readonly List<string> statements = new List<string>();
    private readonly List<SqlQueryTask> queryTasks = new List<SqlQueryTask>();

    private DisposableSet disposables;
    
    public List<string> Statements { get { return statements; } }
    public List<SqlQueryTask> QueryTasks { get { return queryTasks; } }

    public void AddPart(CommandPart part)
    {
      statements.Add(part.Query);
      foreach (var parameter in part.Parameters)
        underlyingCommand.Parameters.Add(parameter);
      if (part.Disposables.Count==0)
        return;
      if (disposables==null)
        disposables = new DisposableSet();
      foreach (var disposable in part.Disposables)
        disposables.Add(disposable);
    }

    public void AddPart(CommandPart part, SqlQueryTask task)
    {
      AddPart(part);
      QueryTasks.Add(task);
    }

    public void ExecuteNonQuery()
    {
      PrepareCommand();
      processor.Driver.ExecuteNonQuery(processor.SessionHandler.Session, underlyingCommand);
    }

    public DbDataReader ExecuteReader()
    {
      PrepareCommand();
      return processor.Driver.ExecuteReader(processor.SessionHandler.Session, underlyingCommand);
    }
    
    public void Dispose()
    {
      underlyingCommand.DisposeSafely();
      disposables.DisposeSafely();
    }

    private void PrepareCommand()
    {
      if (statements.Count==0)
        throw new InvalidOperationException();
      underlyingCommand.CommandText = statements.Count > 1
        ? processor.Driver.BuildBatch(statements.ToArray())
        : statements[0];
    }


    // Constructors

    public Command(CommandProcessor processor, DbCommand underlyingCommand)
    {
      this.processor = processor;
      this.underlyingCommand = underlyingCommand;
    }
  }
}