// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.15

using System;

namespace Xtensive.Orm.Upgrade
{
  internal class FutureResult<T> : IDisposable
  {
    private Func<T> worker;
    private IAsyncResult asyncResult;

    public T Get()
    {
      if (worker==null)
        throw new InvalidOperationException();

      var workerCopy = worker;
      var asyncResultCopy = asyncResult;

      asyncResult = null;
      worker = null;

      if (asyncResultCopy!=null)
        return workerCopy.EndInvoke(asyncResultCopy);

      return workerCopy.Invoke();
    }

    public void Dispose()
    {
      if (asyncResult==null)
        return;
      try {
        Get();
      }
      catch (Exception exception) {
        Log.Warning(exception);
      }
    }

    // Constructors

    public FutureResult(Func<T> action, bool isAsync)
    {
      worker = action;

      if (isAsync)
        asyncResult = action.BeginInvoke(null, null);
    }
  }
}