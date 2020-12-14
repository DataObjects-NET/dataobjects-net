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

      if (resources == null) {
        resources = new DisposableSet();
      }

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

    public IEnumerator<Tuple> AsReaderOfAsync(QueryRequest request, CancellationToken token)
    {
      var accessor = request.GetAccessor();
      using (this) {
        while (NextRow()) {
          yield return ReadTupleWith(accessor);
        }
      }
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

    public IEnumerator<Tuple> AsReaderOf(QueryRequest request)
    {
      var accessor = request.GetAccessor();
      using (this) {
        while (NextRow()) {
          yield return ReadTupleWith(accessor);
        }
      }
    }

    public DbCommand Prepare()
    {
      if (statements.Count==0) {
        throw new InvalidOperationException(Strings.ExUnableToPrepareCommandNoPartsRegistered);
      }

      if (prepared) {
        return underlyingCommand;
      }

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
      reader.DisposeSafely();
      resources.DisposeSafely();
      underlyingCommand.DisposeSafely();
    }

    // Constructors

    internal Command(CommandFactory origin, DbCommand underlyingCommand)
    {
      this.origin = origin;
      this.underlyingCommand = underlyingCommand;
    }
  }
}