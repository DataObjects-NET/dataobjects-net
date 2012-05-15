// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.08.19

using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers
{
  partial class SessionHandler
  {
    internal virtual bool LookupState(Key key, out EntityState entityState)
    {
      return Session.LookupStateInCache(key, out entityState);
    }

    internal virtual bool LookupState(Key key, FieldInfo fieldInfo, out EntitySetState entitySetState)
    {
      return Session.LookupStateInCache(key, fieldInfo, out entitySetState);
    }

    internal virtual EntityState UpdateState(Key key, Tuple tuple)
    {
      return Session.UpdateStateInCache(key, tuple);
    }

    internal virtual EntitySetState UpdateState(Key key, FieldInfo fieldInfo,
      bool isFullyLoaded, List<Key> entityKeys, List<Pair<Key, Tuple>> auxEntities)
    {
      return Session.UpdateStateInCache(key, fieldInfo, entityKeys, isFullyLoaded);
    }
  }
}