// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;
using Xtensive.Disposing;
using Xtensive.Core;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A command ready for execution.
  /// </summary>
  public sealed class Command : IDisposable
  {
    private readonly CommandFactory origin;
    private readonly DbCommand underlyingCommand;

    private bool prepared;
    private DisposableSet resources;
    private DbDataReader reader;

    public List<string> Statements { get; private set; }

    public List<SqlQueryTask> QueryTasks { get; private set; }

    public void AddPart(CommandPart part)
    {
      prepared = false;
      Statements.Add(part.Query);
      foreach (var parameter in part.Parameters)
        underlyingCommand.Parameters.Add(parameter);
      if (part.Resources.Count==0)
        return;
      if (resources==null)
        resources = new DisposableSet();
      foreach (var resource in part.Resources)
        resources.Add(resource);
    }

    public void AddPart(CommandPart part, SqlQueryTask task)
    {
      AddPart(part);
      QueryTasks.Add(task);
    }

    public void ExecuteNonQuery()
    {
      Prepare();
      origin.Driver.ExecuteNonQuery(origin.Session, underlyingCommand);
    }

    public void ExecuteReader()
    {
      Prepare();
      reader = origin.Driver.ExecuteReader(origin.Session, underlyingCommand);
    }

    public bool NextResult()
    {
      try {
        return reader.NextResult();
      }
      catch(Exception exception) {
        throw TranslateException(exception);
      }
    }

    public bool NextRow()
    {
      try {
        return reader.Read();
      }
      catch (Exception exception) {
        throw TranslateException(exception);
      }
    }

    public Tuple ReadTupleWith(DbDataReaderAccessor accessor)
    {
      return accessor.Read(reader);
    }

    public IEnumerator<Tuple> AsReaderOf(TupleDescriptor descriptor)
    {
      var accessor = origin.Driver.GetDataReaderAccessor(descriptor);
      using (this)
        while (NextRow())
          yield return ReadTupleWith(accessor);
    }

    public DbCommand Prepare()
    {
      if (Statements.Count==0)
        throw new InvalidOperationException("Unable to prepare command: no statements registered");

      if (prepared)
        return underlyingCommand;
      prepared = true;
      underlyingCommand.CommandText = origin.Driver.BuildBatch(Statements.ToArray());
      return underlyingCommand;
    }

    private StorageException TranslateException(Exception exception)
    {
      return origin.Driver.ExceptionBuilder
        .BuildException(exception, underlyingCommand.ToHumanReadableString());
    }

    public void Dispose()
    {
      reader.DisposeSafely();
      resources.DisposeSafely();
      underlyingCommand.DisposeSafely();
    }

    // Constructors

    internal Command(CommandFactory origin, DbCommand underlyingCommand)
    {
      QueryTasks = new List<SqlQueryTask>();
      Statements = new List<string>();
      this.origin = origin;
      this.underlyingCommand = underlyingCommand;
    }
  }
}