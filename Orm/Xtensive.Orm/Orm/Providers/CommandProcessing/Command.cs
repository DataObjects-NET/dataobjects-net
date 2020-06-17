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
  public sealed class Command : IDisposable, IAsyncDisposable
  {
    private readonly CommandFactory origin;
    private readonly DbCommand underlyingCommand;
    private readonly List<string> statements = new List<string>();

    private bool prepared;
    private DisposableSet resources;
    private DbDataReader reader;

    public int Count => statements.Count;

    public void AddPart(CommandPart part)
    {
      if (prepared) {
        throw new InvalidOperationException("Unable to change command: it is already prepared");
      }

      statements.Add(part.Statement);

      foreach (var parameter in part.Parameters) {
        underlyingCommand.Parameters.Add(parameter);
      }

      if (part.Resources.Count==0) {
        return;
      }

      resources ??= new DisposableSet();

      foreach (var resource in part.Resources) {
        resources.Add(resource);
      }
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

    public async Task<int> ExecuteNonQueryAsync(CancellationToken token)
    {
      Prepare();
      return await origin.Driver.ExecuteNonQueryAsync(origin.Session, underlyingCommand, token).ConfigureAwait(false);
    }

    public async Task ExecuteReaderAsync(CancellationToken token)
    {
      Prepare();
      reader = await origin.Driver.ExecuteReaderAsync(origin.Session, underlyingCommand, token).ConfigureAwait(false);
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

    public async Task<bool> NextResultAsync(CancellationToken token = default)
    {
      try {
        return await reader.NextResultAsync(token);
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

    public async ValueTask<bool> NextRowAsync(CancellationToken token = default)
    {
      try {
        return await reader.ReadAsync(token);
      }
      catch (Exception exception) {
        throw TranslateException(exception);
      }
    }

    internal Tuple ReadTupleWith(DbDataReaderAccessor accessor) => accessor.Read(reader);

    public DataReader CreateReader(DbDataReaderAccessor accessor, CancellationToken token = default) =>
      new DataReader(this, accessor, token);

    public DbCommand Prepare()
    {
      if (statements.Count==0) {
        throw new InvalidOperationException("Unable to prepare command: no parts registered");
      }

      if (prepared) {
        return underlyingCommand;
      }

      prepared = true;
      underlyingCommand.CommandText = origin.Driver.BuildBatch(statements.ToArray());
      return underlyingCommand;
    }

    private StorageException TranslateException(Exception exception) =>
      origin.Driver.ExceptionBuilder.BuildException(exception, underlyingCommand.ToHumanReadableString());

    public void Dispose()
    {
      reader.DisposeSafely();
      resources.DisposeSafely();
      underlyingCommand.DisposeSafely();
    }

    public async ValueTask DisposeAsync()
    {
      reader.DisposeSafely();
      resources.DisposeSafely();
      await underlyingCommand.DisposeAsync();
    }

    // Constructors

    internal Command(CommandFactory origin, DbCommand underlyingCommand)
    {
      this.origin = origin;
      this.underlyingCommand = underlyingCommand;
    }
  }
}