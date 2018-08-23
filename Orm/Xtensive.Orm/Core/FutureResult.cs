// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.15

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Xtensive.Core
{
  internal abstract class FutureResult<T> : IDisposable
  {
    public abstract bool IsAvailable { get; }
    public abstract T Get();
    public abstract void Dispose();
  }
}