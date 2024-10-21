// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System;
using System.Collections.Generic;
using System.Linq;
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
    public readonly IReadOnlyCollection<PersistRequest> RequestSequence;

    /// <summary>
    /// A tuple that stores changed column values.
    /// </summary>
    public readonly Tuple Tuple;

    /// <summary>
    /// A tuples that store changed column values for multi-record INSERT.
    /// <see cref="Tuple"/> should remain <see langword="null" />
    /// </summary>
    public readonly IReadOnlyList<Tuple> Tuples;

    /// <summary>
    /// A tuple that stored original column values.
    /// </summary>
    public readonly Tuple OriginalTuple;

    /// <summary>
    /// A value indicating if number of affected rows should be checked.
    /// </summary>
    public readonly bool ValidateRowCount;

    /// <inheritdoc/>
    public override void ProcessWith(ISqlTaskProcessor processor, CommandProcessorContext context)
    {
      processor.ProcessTask(this, context);
    }

    // Constructors

    public SqlPersistTask(PersistRequest request, Tuple tuple = null)
    {
      RequestSequence = new PersistRequest[1] { request };
      Tuple = tuple;
    }

    public SqlPersistTask(PersistRequest request, IReadOnlyList<Tuple> tuples)
    {
      RequestSequence = new PersistRequest[1] { request };
      Tuples = tuples;
    }

    [Obsolete]
    public SqlPersistTask(Key key, IEnumerable<PersistRequest> requestSequence, Tuple tuple)
      : this(key, (requestSequence as IReadOnlyCollection<PersistRequest>)?? requestSequence.ToList(), tuple)
    {
    }

    [Obsolete]
    public SqlPersistTask(Key key, IEnumerable<PersistRequest> requestSequence, Tuple tuple, Tuple originalTuple, bool validateRowCount)
      : this(key, (requestSequence as IReadOnlyCollection<PersistRequest>) ?? requestSequence.ToList(), tuple, originalTuple, validateRowCount)
    {
    }

    public SqlPersistTask(Key key, IReadOnlyCollection<PersistRequest> requestSequence, Tuple tuple)
    {
      EntityKey = key;
      RequestSequence = requestSequence;
      Tuple = tuple;
    }

    public SqlPersistTask(Key key, IReadOnlyCollection<PersistRequest> requestSequence, Tuple tuple, Tuple originalTuple, bool validateRowCount)
    {
      EntityKey = key;
      RequestSequence = requestSequence;
      Tuple = tuple;
      OriginalTuple = originalTuple;
      ValidateRowCount = validateRowCount;
    }
  }
}