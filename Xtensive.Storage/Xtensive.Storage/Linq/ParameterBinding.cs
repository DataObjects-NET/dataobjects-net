// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.02.10

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using Xtensive.Core;

namespace Xtensive.Storage.Linq
{
  internal sealed class ParameterBinding : IDisposable
  {
    private readonly Action disposeAction;

    public void Dispose()
    {
      disposeAction();
    }

    public static  ParameterBinding operator & (ParameterBinding x, ParameterBinding y)
    {
      ArgumentValidator.EnsureArgumentNotNull(x, "x");
      ArgumentValidator.EnsureArgumentNotNull(y, "y");
      return new ParameterBinding(y.disposeAction + x.disposeAction);
    }


    // Constructor

    public ParameterBinding(Action disposeAction)
    {
      this.disposeAction = disposeAction;
    }
  }
}