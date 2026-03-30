// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.04.21

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Internals;
using Xtensive.Reflection;
using Xtensive.Tuples;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Linq
{
  /// <summary>
  /// <see cref="Expression"/> related extension methods.
  /// </summary>
  public static class ExpressionExtensions
  {
    private static readonly ConcurrentDictionary<Type, MethodInfo> valueAccessors =
      new ConcurrentDictionary<Type, MethodInfo>();

    private static readonly Func<Type, MethodInfo> TupleValueAccessorFactory;

    private static readonly Type MemoryExtensionsType = typeof(MemoryExtensions);
    private static readonly int[] MemoryExtensionsContainsMethodTokens;
    private static readonly MethodInfo EnumerableContains;

    ///<summary>
    /// Makes <see cref="Tuples.Tuple.GetValueOrDefault{T}"/> method call.
    ///</summary>
    ///<param name="target">Target expression.</param>
    ///<param name="accessorType">Type of accessor.</param>
    ///<param name="index">Tuple field index.</param>
    ///<returns><see cref="MethodCallExpression"/></returns>
    public static MethodCallExpression MakeTupleAccess(this Expression target, Type accessorType, int index) =>
      Expression.Call(
        target,
        valueAccessors.GetOrAdd(accessorType, TupleValueAccessorFactory),
        Expression.Constant(index)
      );

    /// <summary>
    /// Makes <c>IsNull</c> condition expression.
    /// </summary>
    /// <param name="target">Target expression</param>
    /// <param name="ifNull">Result expression if <paramref name="target"/> is <see langword="null" />.</param>
    /// <param name="ifNotNull">Result expression if <paramref name="target"/> is not <see langword="null" />.</param>
    /// <returns><see cref="ConditionalExpression"/></returns>
    public static ConditionalExpression MakeIsNullCondition(
      this Expression target, Expression ifNull, Expression ifNotNull) =>
      Expression.Condition(
        Expression.Equal(target, Expression.Constant(null, target.Type)),
        ifNull,
        ifNotNull
      );

    /// <summary>
    /// Converts expression type to nullable type (for value types).
    /// </summary>
    /// <param name="expression">The expression.</param>
    public static Expression LiftToNullable(this Expression expression) =>
      expression.Type.IsNullable()
        ? expression
        : Expression.Convert(expression, expression.Type.ToNullable());

    /// <summary>
    /// Converts specified <see cref="Expression"/> to <see cref="ExpressionTree"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>Expression tree that wraps <paramref name="expression"/>.</returns>
    public static ExpressionTree ToExpressionTree(this Expression expression) => new ExpressionTree(expression);

    /// <summary>
    /// Transforms <see cref="MemoryExtensions.Contains{T}(ReadOnlySpan{T}, T)"/> applied call into <see cref="Enumerable.Contains{TSource}(IEnumerable{TSource}, TSource)"/>
    /// if detected.
    /// </summary>
    /// <param name="mc">Possible candidate for transformation.</param>
    /// <returns>New instance of expression, if transformation was required, otherwise, the same expression.</returns>
    public static MethodCallExpression TryTransformToOldFashionContains(this MethodCallExpression mc)
    {
      if (mc.Method.DeclaringType == MemoryExtensionsType) {
        var genericMethod = mc.Method.GetGenericMethodDefinition();
        if (MemoryExtensionsContainsMethodTokens.Contains(genericMethod.MetadataToken)) {
          var arguments = mc.Arguments;

          Type elementType;
          Expression[] newArguments;

          if (arguments[0] is MethodCallExpression mcInner && mcInner.Method.Name.Equals(WellKnown.Operator.Implicit, StringComparison.Ordinal)) {
            var wrappedArray = mcInner.Arguments[0];
            elementType = wrappedArray.Type.GetElementType();
            newArguments = new[] { wrappedArray, arguments[1] };
          }
          else if (arguments[0] is UnaryExpression uInner
            && uInner.Method is not null
            && uInner.Method.Name.Equals(WellKnown.Operator.Implicit, StringComparison.Ordinal)) {

            elementType = uInner.Operand.Type.GetElementType();
            newArguments = new[] { uInner.Operand, arguments[1] };
          }
          else {
            return mc;
          }

          var genericContains = EnumerableContains.CachedMakeGenericMethod(elementType);
          var replacement = Expression.Call(genericContains, newArguments);
          return replacement;
        }
        return mc;
      }
      return mc;
    }

    // Type initializer

    static ExpressionExtensions()
    {
      var tupleGenericAccessor = WellKnownOrmTypes.Tuple.GetMethods()
        .Single(mi => mi.Name == nameof(Tuple.GetValueOrDefault) && mi.IsGenericMethod);
      TupleValueAccessorFactory = type => tupleGenericAccessor.CachedMakeGenericMethod(type);

      var genericReadOnlySpan = typeof(ReadOnlySpan<>);
      var genericSpan = typeof(Span<>);

      var filteredByNameItems = MemoryExtensionsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
        .Where(m => m.Name.Equals(nameof(System.MemoryExtensions.Contains), StringComparison.OrdinalIgnoreCase));

      var candiates = new List<int>();

      foreach (var method in filteredByNameItems) {
        var parameters = method.GetParameters();
        var genericDef = parameters[0].ParameterType.GetGenericTypeDefinition();
        if (genericDef == genericReadOnlySpan) {
          if (parameters.Length == 2 || parameters.Length == 3)
            candiates.Add(method.MetadataToken);
        }
        else if (genericDef == genericSpan && parameters.Length == 2) {
          candiates.Add(method.MetadataToken);
        }
      }
      MemoryExtensionsContainsMethodTokens = candiates.ToArray();
      EnumerableContains = typeof(System.Linq.Enumerable).GetMethodEx(nameof(System.Linq.Enumerable.Contains), BindingFlags.Public | BindingFlags.Static, new string[1], new object[2]);
    }
  }
}