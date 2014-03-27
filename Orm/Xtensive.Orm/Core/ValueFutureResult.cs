// Copyright (C) 2014 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2014.03.12

using System;

namespace Xtensive.Core
{
  internal sealed class ValueFutureResult<T> : FutureResult<T>
  {
    private bool isAvailable;
    private T result;

    public override bool IsAvailable
    {
      get { return isAvailable; }
    }

    public override T Get()
    {
      if (!IsAvailable)
        throw new InvalidOperationException(Strings.ExResultIsNotAvailable);

      var localResult = result;
      isAvailable = false;
      result = default(T);
      return localResult;
    }

    public override void Dispose()
    {
    }

    public ValueFutureResult(T result)
    {
      this.result = result;
      isAvailable = true;
    }
  }
}