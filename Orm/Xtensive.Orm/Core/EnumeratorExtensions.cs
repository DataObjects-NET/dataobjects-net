// Copyright (C) 2019 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2019.06.24

using System.Collections.Generic;

namespace Xtensive.Core
{
  public static class EnumeratorExtensions
  {
    public static IEnumerable<TItem> ToEnumerable<TItem>(this IEnumerator<TItem> enumerator)
    {
      using (enumerator) {
        while (enumerator.MoveNext()) 
          yield return enumerator.Current;
      }
    }
  }
}
