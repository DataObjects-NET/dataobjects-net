// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System.Collections;
using System.Collections.Generic;

namespace Xtensive.Core.Collections
{
  internal class TypedEnumerable<T> : IEnumerable<T>
  {
    IEnumerable innerEnumerable;

    public IEnumerator<T> GetEnumerator()
    {
      return new TypedEnumerator<T>(innerEnumerable.GetEnumerator());
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return innerEnumerable.GetEnumerator();
    }


    // Constructors

    public TypedEnumerable(IEnumerable innerEnumerable)
    {
      this.innerEnumerable = innerEnumerable;
    }
  }
}