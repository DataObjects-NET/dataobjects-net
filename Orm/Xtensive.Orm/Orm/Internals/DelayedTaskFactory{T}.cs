// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2014.08.29

namespace Xtensive.Orm.Internals
{
#if NET45
  internal class DelayedTaskFactory<T>
  {
    public DelayedTask<T> CreateNew<T>(Session session, DelayedQueryResult<T> delayedResult)
    {
      return new DelayedTask<T>(session, delayedResult);
    }

    public DelayedTask<T> CreateNew<T>(DelayedQueryResult<T> delayedResult)
    {
      var session = Session.Demand();
      return new DelayedTask<T>(session, delayedResult);
    }
  }
#endif
}
