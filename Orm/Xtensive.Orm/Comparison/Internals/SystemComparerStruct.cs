// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.02.10

using System.Runtime.Serialization;
using Xtensive.Core;

namespace Xtensive.Comparison
{
  internal struct SystemComparerStruct<T>
  {
    public readonly static SystemComparerStruct<T> Instance = new SystemComparerStruct<T>(true);

    public readonly Func<T, T, int> Compare;
    public readonly new Predicate<T, T> Equals;
    public readonly new Func<T, int> GetHashCode;


    // Constructors

    private SystemComparerStruct(bool ignore)
    {
      var comparer = 
        (IComparer<T>)Comparer<T>.Default ?? 
        (IComparer<T>)(object)NoSystemComparerHandler<T>.Instance;
      var equalityComparer = 
        (IEqualityComparer<T>)EqualityComparer<T>.Default ?? 
        (IEqualityComparer<T>)(object)NoSystemComparerHandler<T>.Instance;
      Compare = comparer.Compare;
      Equals = equalityComparer.Equals;
      GetHashCode = equalityComparer.GetHashCode;
    }
  }
}