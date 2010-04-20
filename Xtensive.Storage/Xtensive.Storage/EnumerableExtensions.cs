// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.05.07

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Core.Tuples;
using Tuple = Xtensive.Core.Tuples.Tuple;
using System.Linq;
using Xtensive.Core.Tuples.Transform;

namespace Xtensive.Storage
{
  /// <summary>
  /// Extends <see cref="IEnumerable{T}"/>.
  /// </summary>
  public static class TupleEnumerableExtensions
  {
    /// <summary>
    /// Converts <see cref="IEnumerable{T}"/> to <see cref="IEnumerable{Tuple}"/>.
    /// Supports only primitive types, <see cref="Structure"/>, <see cref="Entity"/>
    /// </summary>
    /// <typeparam name="T">Type of enumerable items.</typeparam>
    /// <param name="source">Source enumerable.</param>
    /// <returns>New <see cref="Enumerable<Tuple>"/></returns>
    /// <exception cref="NotSupportedException"><typeparamref name="T"/> is not supported.</exception>
    public static IEnumerable<Tuple> AsTupleEnumerable<T>(this IEnumerable<T> source)
    {
      ArgumentValidator.EnsureArgumentNotNull(source, "source");
      var type = typeof (T);
      if (type.IsSubclassOf(typeof (Entity)))
        return source.Select(e => ((Entity) (object) e).Tuple.ToReadOnly(TupleTransformType.Auto));

      if (type.IsSubclassOf(typeof (Structure)))
        return source.Select(s => ((Structure) (object) s).Tuple.ToReadOnly(TupleTransformType.Auto));

      if (type.IsPrimitive
        || type==typeof (decimal)
          || type==typeof (string)
            || type==typeof (DateTime)
              || type==typeof (TimeSpan)
        )
        return source.Select(t => (Tuple) Tuple.Create(t));

      throw new NotSupportedException();
    }
  }
}