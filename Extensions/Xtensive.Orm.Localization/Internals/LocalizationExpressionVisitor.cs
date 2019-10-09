// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.16

using System.Linq.Expressions;
using Xtensive.Core;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Orm.Localization
{
  internal class LocalizationExpressionVisitor : ExpressionVisitor
  {
    public TypeLocalizationMap Map { get; private set; }

    public Expression VisitExpression(Expression query)
    {
      return Visit(query);
    }

    /// <inheritdoc/>
    protected override Expression VisitMemberAccess(MemberExpression me)
    {
      if (Map == null)
        return me;

      var localizationInfo = Map.Get(me.Member.ReflectedType);
      if (localizationInfo == null || !localizationInfo.LocalizationTypeInfo.Fields.Contains(me.Member.Name))
        return base.VisitMemberAccess(me);

      var localizationSetExpression = Expression.MakeMemberAccess(me.Expression, localizationInfo.LocalizationSetProperty);
      var localizationExpression = LocalizationExpressionBuilder.BuildExpression(localizationInfo, me, Map.Configuration.DefaultCulture.Name);
      
      return localizationExpression.BindParameters(localizationSetExpression);
    }

    public LocalizationExpressionVisitor(TypeLocalizationMap map)
    {
      Map = map;
    }
  }
}