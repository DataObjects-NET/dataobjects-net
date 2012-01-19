// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Diagnostics;

namespace Xtensive.Disposing
{
  /// <summary>
  /// An implementation of <see cref="ICompletableScope"/>, that is always uncompleted.
  /// Useful, when it's necessary to certain action.
  /// </summary>
  public sealed class IncompletableScope : CompletableScope
  {
    public override void Complete()
    {
      return;
    }


    // Constructors
    
    // Disposal

    public override void Dispose()
    {
      return;
    }
  }
}