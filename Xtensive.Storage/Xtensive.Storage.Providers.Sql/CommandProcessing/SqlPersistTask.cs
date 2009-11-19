// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using Xtensive.Core.Tuples;

namespace Xtensive.Storage.Providers.Sql
{
  /// <summary>
  /// A persist task (i.e. INSERT, UPDATE, DELETE) for <see cref="CommandProcessor"/>.
  /// </summary>
  public sealed class SqlPersistTask : SqlTask
  {
    /// <summary>
    /// A request.
    /// </summary>
    public readonly PersistRequest Request;

    /// <summary>
    /// A tuple containing parameter for request.
    /// </summary>
    public readonly Tuple Tuple;

    /// <inheritdoc/>
    public override void ProcessWith(CommandProcessor processor)
    {
      processor.ProcessTask(this);
    }


    // Constructors

    public SqlPersistTask(PersistRequest request, Tuple tuple)
    {
      Request = request;
      Tuple = tuple;
    }
  }
}