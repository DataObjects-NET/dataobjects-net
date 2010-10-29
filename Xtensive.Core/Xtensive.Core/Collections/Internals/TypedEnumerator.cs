// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections.Generic;
using System.Collections;
using Xtensive.Core;

namespace Xtensive.Collections
{
  internal class TypedEnumerator<T> : IEnumerator<T>
  {
    private IEnumerator innerEnumerator;

    public object Current
    {
      get { return (T)innerEnumerator.Current; }
    }

    T IEnumerator<T>.Current {
      get {
        return (T)innerEnumerator.Current;
      }
    }

    public bool MoveNext()
    {
      return innerEnumerator.MoveNext();
    }

    public void Reset()
    {
      innerEnumerator.Reset();
    }

    public void Dispose()
    {
      innerEnumerator = null;
    }


    // Constructors

    public TypedEnumerator(IEnumerator innerEnumerator)
    {
      ArgumentValidator.EnsureArgumentNotNull(innerEnumerator, "innerEnumerator");
      this.innerEnumerator = innerEnumerator;
    }
  }
}
