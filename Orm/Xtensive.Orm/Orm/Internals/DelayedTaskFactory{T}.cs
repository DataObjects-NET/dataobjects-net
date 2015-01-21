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
      var task = new DelayedTask<T>(session, delayedResult);
      session.AsyncQueriesManager.AddNewDelayedTask(delayedResult.Task,task);
      return task;
    }
  }
#endif
}
