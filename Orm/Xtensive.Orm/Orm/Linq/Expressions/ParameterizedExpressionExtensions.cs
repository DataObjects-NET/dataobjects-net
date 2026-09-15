// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.05.05

using System.Linq;
using System.Collections.Generic;

namespace Xtensive.Orm.Linq.Expressions
{
  internal static class ParameterizedExpressionExtensions
  {
    /// <summary>
    /// Gets instances of any implementation of <see cref="FieldExpression"/>
    /// </summary>
    /// <param name="fields">Sequence of expressions.</param>
    /// <returns>Filtered results.</returns>
    public static IEnumerable<FieldExpression> OfAnyFieldExpression(this IEnumerable<PersistentFieldExpression> fields)
    {
      // Faster that .OfType<FieldExpression>()
      return fields
        .Where(static f => f.ExtendedType is ExtendedExpressionType.Field
            or ExtendedExpressionType.EntityField
            or ExtendedExpressionType.EntitySet
            or ExtendedExpressionType.StructureField)
        .Cast<FieldExpression>();
    }

    /// <summary>
    /// Gets instances of only <see cref="FieldExpression"/> type.
    /// </summary>
    /// <param name="fields">Sequence of expressions.</param>
    /// <returns>Filtered results.</returns>
    public static IEnumerable<FieldExpression> OfExactlyFieldExpression(this IEnumerable<PersistentFieldExpression> fields)
    {
      // A lot faster than fields.OfType<FieldExpression>().Where(static f => f.ExtendedType is ExtendedExpressionType.Field)
      // because contains less casts and more basic comparisons
      return fields
        .Where(static f => f.ExtendedType is ExtendedExpressionType.Field)
        .Cast<FieldExpression>();
    }
  }
}