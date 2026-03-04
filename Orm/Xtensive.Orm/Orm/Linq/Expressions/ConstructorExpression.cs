// Copyright (C) 2009-2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2009.10.16

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Orm.Linq.Expressions;
using Xtensive.Orm.Linq.Expressions.Visitors;
using System.Linq;

namespace Xtensive.Orm.Linq
{
  [Serializable]
  internal class ConstructorExpression : ParameterizedExpression,
    IMappedExpression
  {
    public Dictionary<MemberInfo, Expression> Bindings { get; private set; }

    public Dictionary<MemberInfo, Expression> NativeBindings { get; private set; }

    public ConstructorInfo Constructor { get; private set; }

    public IEnumerable<Expression> ConstructorArguments { get; private set; }

    public Expression BindParameter(ParameterExpression parameter, Dictionary<Expression, Expression> processedExpressions)
    {
      Func<Expression, Expression> genericBinder =
        e => GenericExpressionVisitor<IMappedExpression>.Process(e, mapped => mapped.BindParameter(parameter, processedExpressions));

      return new ConstructorExpression(
        Type,
        Bindings.ToDictionary(kvp => kvp.Key, kvp => genericBinder(kvp.Value), Bindings.Count),
        NativeBindings.ToDictionary(kvp=>kvp.Key, kvp => genericBinder(kvp.Value), NativeBindings.Count),
        Constructor,
        ReferenceEquals(ConstructorArguments, Array.Empty<Expression>())
          ? ConstructorArguments
          : ConstructorArguments.Select(genericBinder).ToArray());
    }

    public Expression RemoveOuterParameter(Dictionary<Expression, Expression> processedExpressions)
    {
      Func<Expression, Expression> genericRemover =
        e => GenericExpressionVisitor<IMappedExpression>.Process(e, mapped => mapped.RemoveOuterParameter(processedExpressions));

      var result = new ConstructorExpression(
        Type,
        Bindings.ToDictionary(kvp => kvp.Key, kvp => genericRemover(kvp.Value)),
        NativeBindings = NativeBindings.ToDictionary(kvp => kvp.Key, kvp => genericRemover(kvp.Value)),
        Constructor,
        ReferenceEquals(ConstructorArguments, Array.Empty<Expression>())
          ? ConstructorArguments
          : ConstructorArguments.Select(genericRemover).ToArray());
      return result;
    }

    public Expression Remap(int offset, Dictionary<Expression, Expression> processedExpressions)
    {
      Func<IMappedExpression, Expression> remapper = delegate(IMappedExpression mapped) {
        var parametrizedExpression = mapped as ParameterizedExpression;
        if (parametrizedExpression!=null && (parametrizedExpression.OuterParameter==OuterParameter || OuterParameter==null))
          return mapped.Remap(offset, new Dictionary<Expression, Expression>());
        return (Expression) mapped;
      };

      var newBindings = Bindings.ToDictionary(kvp => kvp.Key, kvp => GenericExpressionVisitor<IMappedExpression>.Process(kvp.Value, remapper));
      var newConstructorArguments = ReferenceEquals(ConstructorArguments, Array.Empty<Expression>())
        ? ConstructorArguments
        : ConstructorArguments.Select(arg =>  GenericExpressionVisitor<IMappedExpression>.Process(arg, remapper));
      var newNativeBindings = NativeBindings.ToDictionary(kvp => kvp.Key, kvp => GenericExpressionVisitor<IMappedExpression>.Process(kvp.Value, remapper));
      var result = new ConstructorExpression(
        Type,
        newBindings,
        newNativeBindings,
        Constructor,
        newConstructorArguments);
      return result;
    }

    public Expression Remap(IReadOnlyList<int> map, Dictionary<Expression, Expression> processedExpressions)
    {
      Func<IMappedExpression, Expression> remapper = delegate(IMappedExpression mapped) {
        var parametrizedExpression = mapped as ParameterizedExpression;
        if (parametrizedExpression!=null && (parametrizedExpression.OuterParameter==OuterParameter || OuterParameter==null))
          return mapped.Remap(map, new Dictionary<Expression, Expression>());
        return (Expression) mapped;
      };

      var newBindings = Bindings.ToDictionary(kvp => kvp.Key, kvp => GenericExpressionVisitor<IMappedExpression>.Process(kvp.Value, remapper));
      var newConstructorArguments = ReferenceEquals(ConstructorArguments, Array.Empty<Expression>())
        ? ConstructorArguments
        : ConstructorArguments.Select(arg =>  GenericExpressionVisitor<IMappedExpression>.Process(arg, remapper));
      var newNativeBindings = NativeBindings.ToDictionary(kvp => kvp.Key, kvp => GenericExpressionVisitor<IMappedExpression>.Process(kvp.Value, remapper));
      return new ConstructorExpression(Type, newBindings, newNativeBindings, Constructor, newConstructorArguments);
    }

    public ConstructorExpression(Type type,
      Dictionary<MemberInfo, Expression> bindings,
      Dictionary<MemberInfo, Expression> nativeBindings,
      ConstructorInfo constructor,
      IEnumerable<Expression> constructorArguments)
      : base(ExtendedExpressionType.Constructor, type, null, false)
    {
      Bindings = bindings ?? new Dictionary<MemberInfo, Expression>();
      NativeBindings = nativeBindings;
      // consistently use keep empty array instead of any other empty collection,
      // there are checks that help to not instanciate collections
      ConstructorArguments = constructorArguments ?? Array.Empty<Expression>();
      Constructor = constructor;
    }
  }
}