using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Xtensive.Orm.Model;
using Xtensive.Reflection;
using FieldInfo = Xtensive.Orm.Model.FieldInfo;

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
        break;
      case ExpressionType.MemberAccess:
        return VisitMemberAccess((MemberExpression) e);
        break;
      default:
        throw UnsupportedTypeExpression(e);
      }
    }

    private Exception UnsupportedTypeExpression(Expression e)
    {
      throw new NotSupportedException(string.Format(Strings.ExXIsNotSuitableFieldFoFullTextSearch, rootExpression));
    }

    protected FieldInfo VisitMemberAccess(MemberExpression m)
    {
      Visit(m.Expression);
      var property = m.Member as PropertyInfo;
      if (property==null)
        throw UnsupportedTypeExpression(m);
      if (typeof (Entity).IsAssignableFrom(property.GetGetMethod().ReturnType))
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
