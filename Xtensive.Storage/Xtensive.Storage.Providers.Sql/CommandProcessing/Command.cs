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
    private readonly List<string> queries = new List<string>();
    private DisposableSet disposables;
    
    public int CommandCount { get { return queries.Count; } }

    public void AddPart(string query)
    {
      queries.Add(query);
    }

    public void AddPart(CommandPart part)
    {
      queries.Add(part.Query);
      foreach (var parameter in part.Parameters)
        underlyingCommand.Parameters.Add(parameter);
      if (part.Disposables.Count==0)
        return;
      if (disposables==null)
        disposables = new DisposableSet();
      foreach (var disposable in part.Disposables)
        disposables.Add(disposable);
    }

    public string GetParameterPrefix()
    {
      return string.Format("p{0}_", queries.Count);
    }

    public object ExecuteScalar()
    {
      PrepareCommand();
      return processor.Driver.ExecuteScalar(processor.SessionHandler.Session, underlyingCommand);
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
      if (queries.Count==0)
        throw new InvalidOperationException();
      underlyingCommand.CommandText = queries.Count > 1
        ? processor.Driver.BuildBatch(queries.ToArray())
        : queries[0];
    }


    // Constructors

    public Command(CommandProcessor processor, DbCommand underlyingCommand)
    {
      this.processor = processor;
      this.underlyingCommand = underlyingCommand;
    }
  }
}