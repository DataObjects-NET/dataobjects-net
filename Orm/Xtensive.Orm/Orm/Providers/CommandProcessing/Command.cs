// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
    private bool isDisposed;
    private DisposableSet resources;
    private DbDataReader reader;

    public int Count => statements.Count;

    internal int ParametersCount => underlyingCommand.Parameters.Count;

    public void AddPart(CommandPart part)
    {
      if (prepared) {
        throw new InvalidOperationException(Strings.ExUnableToChangeCommandItIsAlreadyPrepared);
      }

      statements.Add(part.Statement);

      foreach (var parameter in part.Parameters) {
        _ = underlyingCommand.Parameters.Add(parameter);
      }

      if (part.Resources.Count==0) {
        return;
      }

      resources ??= new DisposableSet();

      foreach (var resource in part.Resources) {
        _ = resources.Add(resource);
      }
    }

    public int ExecuteNonQuery()
    {
      _ = Prepare();
      return origin.Driver.ExecuteNonQuery(origin.Session, underlyingCommand);
    }

    public void ExecuteReader()
    {
      _ = Prepare();
      reader = origin.Driver.ExecuteReader(origin.Session, underlyingCommand);
    }

    public async Task<int> ExecuteNonQueryAsync(CancellationToken token)
    {
      _ = Prepare();
      return await origin.Driver.ExecuteNonQueryAsync(origin.Session, underlyingCommand, token).ConfigureAwait(false);
    }

    public async Task ExecuteReaderAsync(CancellationToken token)
    {
      _ = Prepare();
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
        return await reader.NextResultAsync(token).ConfigureAwait(false);
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
        return await reader.ReadAsync(token).ConfigureAwait(false);
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
      if (statements.Count == 0) {
        throw new InvalidOperationException("Unable to prepare command: no parts registered");
      }

      if (prepared) {
        return underlyingCommand;
      }

      prepared = true;
      underlyingCommand.CommandText = origin.Driver.BuildBatch(statements);
      return underlyingCommand;
    }

    private StorageException TranslateException(Exception exception) =>
      origin.Driver.ExceptionBuilder.BuildException(exception, underlyingCommand.ToHumanReadableString());

    public void Dispose()
    {
      if (!isDisposed) {
        isDisposed = true;
        reader.DisposeSafely();
        resources.DisposeSafely();
        underlyingCommand.DisposeSafely();
      }
    }

    public async ValueTask DisposeAsync()
    {
      if (!isDisposed) {
        isDisposed = true;
        await reader.DisposeSafelyAsync().ConfigureAwait(false);
        await resources.DisposeSafelyAsync().ConfigureAwait(false);
        await underlyingCommand.DisposeSafelyAsync().ConfigureAwait(false);
      }
    }

    // Constructors

    internal Command(CommandFactory origin, DbCommand underlyingCommand)
    {
      this.origin = origin;
      this.underlyingCommand = underlyingCommand;
    }
  }
}
