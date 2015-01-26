// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;

using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A command ready for execution.
  /// </summary>
  public sealed class Command : IDisposable
  {
    private readonly CommandFactory origin;
    private readonly DbCommand underlyingCommand;
    private readonly List<string> statements = new List<string>();

    private bool prepared;
    private DisposableSet resources;
    private DbDataReader reader;

    public int Count { get { return statements.Count; } }
    public bool IsDisposed { get; private set; }

    public void AddPart(CommandPart part)
    {
      if (prepared)
        throw new InvalidOperationException("Unable to change command: it is already prepared");

      statements.Add(part.Statement);

      foreach (var parameter in part.Parameters)
        underlyingCommand.Parameters.Add(parameter);

      if (part.Resources.Count==0)
        return;

      if (resources==null)
        resources = new DisposableSet();
      foreach (var resource in part.Resources)
        resources.Add(resource);
    }

    public int ExecuteNonQuery()
    {
      Prepare();
      return origin.Driver.ExecuteNonQuery(origin.Session, underlyingCommand);
    }

    public void ExecuteReader()
    {
      Prepare();
      reader = origin.Driver.ExecuteReader(origin.Session, underlyingCommand);
    }

#if NET45

    public async Task<int> ExecuteNonQueryAsync(CancellationToken token)
    {
      Prepare();
      return await origin.Driver.ExecuteNonQueryAsync(origin.Session, underlyingCommand, token);
    }

    public async Task ExecuteReaderAsync(CancellationToken token)
    {
      Prepare();
      reader = await origin.Driver.ExecuteReaderAsync(origin.Session, underlyingCommand, token);
    }

    public IEnumerator<Tuple> AsReaderOfAsync(QueryRequest request, CancellationToken token)
    {
      var accessor = request.GetAccessor();
      using (this)
        while (NextRow())
          yield return ReadTupleWith(accessor);
    }

#endif

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

    public IEnumerator<Tuple> AsReaderOf(QueryRequest request)
    {
      var accessor = request.GetAccessor();
      using (this)
        while (NextRow())
          yield return ReadTupleWith(accessor);
    }

    public DbCommand Prepare()
    {
      if (statements.Count==0)
        throw new InvalidOperationException("Unable to prepare command: no parts registered");
      if (prepared)
        return underlyingCommand;
      prepared = true;
      underlyingCommand.CommandText = origin.Driver.BuildBatch(statements.ToArray());
      return underlyingCommand;
    }

    private StorageException TranslateException(Exception exception)
    {
      return origin.Driver.ExceptionBuilder
        .BuildException(exception, underlyingCommand.ToHumanReadableString());
    }

    public void Dispose()
    {
      if (IsDisposed)
        return;
      reader.DisposeSafely();
      resources.DisposeSafely();
      underlyingCommand.DisposeSafely();
      IsDisposed = true;
    }

    // Constructors

    internal Command(CommandFactory origin, DbCommand underlyingCommand)
    {
      this.origin = origin;
      this.underlyingCommand = underlyingCommand;
      IsDisposed = false;
    }
  }
}