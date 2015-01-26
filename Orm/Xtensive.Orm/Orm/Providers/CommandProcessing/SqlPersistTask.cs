// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System;
using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  /// <summary>
  /// A persist task (i.e. INSERT, UPDATE, DELETE) for <see cref="CommandProcessor"/>.
  /// </summary>
  public sealed class SqlPersistTask : SqlTask
  {
    /// <summary>
    /// A key of an entity to persist (optional).
    /// </summary>
    public readonly Key EntityKey;

    /// <summary>
    /// Requests to execute.
    /// </summary>
    public readonly IEnumerable<PersistRequest> RequestSequence;

    /// <summary>
    /// A tuple that stores changed column values.
    /// </summary>
    public readonly Tuple Tuple;

    /// <summary>
    /// A tuple that stored original column values.
    /// </summary>
    public readonly Tuple OriginalTuple;

    /// <summary>
    /// A value indicating if number of affected rows should be checked.
    /// </summary>
    public readonly bool ValidateRowCount;

    /// <inheritdoc/>
    public override void ProcessWith(ISqlTaskProcessor processor, Guid uniqueIdentifier)
    {
      processor.ProcessTask(this, uniqueIdentifier);
    }

    // Constructors

    public SqlPersistTask(PersistRequest request, Tuple tuple)
    {
      RequestSequence = EnumerableUtils.One(request);
      Tuple = tuple;
    }

    public SqlPersistTask(Key key, IEnumerable<PersistRequest> requestSequence, Tuple tuple)
    {
      EntityKey = key;
      RequestSequence = requestSequence;
      Tuple = tuple;
    }

    public SqlPersistTask(Key key, IEnumerable<PersistRequest> requestSequence, Tuple tuple, Tuple originalTuple, bool validateRowCount)
    {
      EntityKey = key;
      RequestSequence = requestSequence;
      Tuple = tuple;
      OriginalTuple = originalTuple;
      ValidateRowCount = validateRowCount;
    }
  }
}