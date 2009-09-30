// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.09.03

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Internals
{
  [Serializable]
  internal sealed class EntityPrefetchTask : PrefetchTask
  {
    private QueryTask queryFetchingColumns;

    public override bool IsActive { get { return queryFetchingColumns!=null; } }

    public override List<Tuple> Result
    {
      get { return queryFetchingColumns.Result; }
    }

    public override void RegisterQueryTask()
    {
      if (!IsActive) {
        if (!TryActivate())
          return;
        queryFetchingColumns = CreateQueryTask(Key);
      }
      Processor.Owner.Session.RegisterDelayedQuery(queryFetchingColumns);
    }


    // Constructors

    public EntityPrefetchTask(Key key, TypeInfo type, bool exactType, PrefetchProcessor processor) :
      base(key, type, exactType, processor)
    {
      ArgumentValidator.EnsureArgumentNotNull(key, "key");
    }
  }
}