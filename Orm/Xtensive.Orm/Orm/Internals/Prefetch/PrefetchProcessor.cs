// Copyright (C) 2010-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2010.11.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Xtensive.Orm.Internals.Prefetch
{
  [Serializable]
  internal class PrefetchProcessor<T> : IEnumerable<T>
  {
    public IEnumerator<T> GetEnumerator()
    {
      throw new NotImplementedException();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}