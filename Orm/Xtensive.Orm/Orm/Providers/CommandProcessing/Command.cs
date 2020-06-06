// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.10.09

using System;
using System.Collections;
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

    public Tuple ReadTupleWith(DbDataReaderAccessor accessor) => accessor.Read(reader);

    public TupleEnumerator AsEnumeratorOf(QueryRequest request, CancellationToken token = default) =>
      new TupleEnumerator(this, request, token);

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

  public readonly struct TupleEnumerator: IEnumerator<Tuple>, IAsyncEnumerator<Tuple>
  {
    private readonly object source;
    private readonly CancellationToken token;
    private readonly DbDataReaderAccessor accessor;

    public bool IsInMemory => !(source is Command);

    public Tuple Current => source is Command command
      ? command.ReadTupleWith(accessor)
      : ((IEnumerator<Tuple>)source).Current;

    object IEnumerator.Current => Current;

    public bool MoveNext() => source is Command command
      ? command.NextRow()
      : ((IEnumerator<Tuple>) source).MoveNext();

    public async ValueTask<bool> MoveNextAsync() => source is Command command
      ? await command.NextRowAsync(token)
      : ((IEnumerator<Tuple>) source).MoveNext();

    void IEnumerator.Reset() => throw new NotSupportedException();

    public void Dispose()
    {
      if (source is Command command) {
        command.Dispose();
      }
    }

    public async ValueTask DisposeAsync()
    {
      if (source is Command command) {
        await command.DisposeAsync();
      }
    }

    // IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    //
    // public IEnumerator<Tuple> GetEnumerator()
    // {
    //   if (source is Command command) {
    //     var accessor = request.GetAccessor();
    //     return EnumerateCommand(command, accessor);
    //   }
    //
    //   var tuples = (IEnumerable<Tuple>)source;
    //   return tuples.GetEnumerator();
    // }
    //
    // private static IEnumerator<Tuple> EnumerateCommand(Command command, DbDataReaderAccessor accessor)
    // {
    //   using (command) {
    //     while (command.NextRow()) {
    //       yield return command.ReadTupleWith(accessor);
    //     }
    //   }
    // }
    //
    // public async IAsyncEnumerator<Tuple> GetAsyncEnumerator(CancellationToken token = default)
    // {
    //   if (!(source is Command command)) {
    //     throw new NotSupportedException("Async enumeration makes sense only for async source.");
    //   }
    //
    //   var accessor = request.GetAccessor();
    //   using (command) {
    //     while (await command.NextRowAsync(token)) {
    //       token.ThrowIfCancellationRequested();
    //       yield return command.ReadTupleWith(accessor);
    //     }
    //   }
    // }

    public TupleEnumerator(IEnumerable<Tuple> tuples)
    {
      source = tuples;
      accessor = null;
    }

    public TupleEnumerator(Command command, QueryRequest request, CancellationToken token)
    {
      source = command;
      accessor = request.GetAccessor();
      this.token = token;
    }
  }
}