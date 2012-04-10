// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.21

using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers.Sql
{
  /// <summary>
  /// A persist task (i.e. INSERT, UPDATE, DELETE) for <see cref="CommandProcessor"/>.
  /// </summary>
  public sealed class SqlPersistTask : SqlTask
  {
    /// <summary>
    /// Requests to execute.
    /// </summary>
    public readonly IEnumerable<PersistRequest> RequestSequence;

    /// <summary>
    /// A tuple containing parameter for requests.
    /// </summary>
    public readonly Tuple Tuple;

    
    public override void ProcessWith(ISqlTaskProcessor processor)
    {
      processor.ProcessTask(this);
    }


    // Constructors

    public SqlPersistTask(PersistRequest request, Tuple tuple)
    {
      RequestSequence = EnumerableUtils.One(request);
      Tuple = tuple;
    }

    public SqlPersistTask(IEnumerable<PersistRequest> requestSequence, Tuple tuple)
    {
      RequestSequence = requestSequence;
      Tuple = tuple;
    }
  }
}