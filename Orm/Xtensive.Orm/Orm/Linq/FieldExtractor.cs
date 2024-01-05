// Copyright (C) 2016-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.12.14

using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Orm.Internals;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;
using TypeInfo = Xtensive.Orm.Model.TypeInfo;

namespace Xtensive.Orm.Linq
{
  internal sealed class FieldExtractor
  {
    private readonly TypeInfo modelType;
    private Expression rootExpression;

    public static FieldInfo Extract(LambdaExpression fieldAccessLambda, Type elementType, TypeInfo modelType)
    {
      return new FieldExtractor(modelType).Visit(fieldAccessLambda.Body);
    }

    private FieldInfo Visit(Expression e)
    {
      if (rootExpression==null)
        rootExpression = e;
      switch (e.NodeType) {
      case ExpressionType.Parameter:
        return VisitParameter((ParameterExpression) e);
      case ExpressionType.MemberAccess:
        return VisitMemberAccess((MemberExpression) e);
      default:
        throw UnsupportedTypeExpression(e);
      }
    }

    private Exception UnsupportedTypeExpression(Expression e)
    {
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSuitableFieldFoFullTextSearch, rootExpression));
    }

    private FieldInfo VisitMemberAccess(MemberExpression m)
    {
      Visit(m.Expression);
      var property = m.Member as PropertyInfo;
      if (property==null)
        throw UnsupportedTypeExpression(m);
      if (WellKnownOrmTypes.Entity.IsAssignableFrom(property.GetGetMethod().ReturnType))
        throw UnsupportedTypeExpression(m);
      if (rootExpression!=m)
        return null;
      if (!property.GetAttributes<FieldAttribute>(AttributeSearchOptions.InheritAll).Any())
        throw UnsupportedTypeExpression(m);
      if (!property.GetAttributes<FullTextAttribute>(AttributeSearchOptions.InheritAll).Any())
        throw UnsupportedTypeExpression(m);
      var fieldInfo = modelType.Fields.FirstOrDefault(f => f.UnderlyingProperty==property);
      return fieldInfo;
    }

    private FieldInfo VisitParameter(ParameterExpression p)
    {
      if (p.Type!=modelType.UnderlyingType)
        throw new InvalidOperationException(string.Format(Strings.ExFieldAccessExpressionXDoesNotAccessToYTypeMembers, rootExpression, modelType.UnderlyingType));
      return null;
    }

    private FieldExtractor(TypeInfo typeInfo)
    {
      modelType = typeInfo;
    }
  }
}
