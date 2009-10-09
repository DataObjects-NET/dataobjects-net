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
    private readonly DbCommand underlyingCommand;
    private readonly Driver driver;
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
      return driver.ExecuteScalar(underlyingCommand);
    }

    public void ExecuteNonQuery()
    {
      PrepareCommand();
      driver.ExecuteNonQuery(underlyingCommand);
    }

    public DbDataReader ExecuteReader()
    {
      PrepareCommand();
      return driver.ExecuteReader(underlyingCommand);
    }

    public void Dispose()
    {
      underlyingCommand.DisposeSafely();
      disposables.DisposeSafely();
    }

    private void PrepareCommand()
    {
      underlyingCommand.CommandText = driver.BuildBatch(queries.ToArray());
    }


    // Constructors

    public Command(Driver driver, DbCommand underlyingCommand)
    {
      this.driver = driver;
      this.underlyingCommand = underlyingCommand;
    }
  }
}