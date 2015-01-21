using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xtensive.Core
{
  public static class EnumeratorExtensions
  {
    public static IEnumerable<TItem> ToEnumerable<TItem>(this IEnumerator<TItem> enumerator)
    {
      using (enumerator) {
        while (enumerator.MoveNext()) {
          yield return enumerator.Current;
        }
      }
    }
  }
}
