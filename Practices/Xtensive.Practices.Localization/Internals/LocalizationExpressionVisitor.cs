// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Gamzov
// Created:    2009.12.16

using System;
using System.Linq.Expressions;
using Xtensive.Core;
using Xtensive.Linq;
using Xtensive.Orm;
using ExpressionVisitor = Xtensive.Linq.ExpressionVisitor;

namespace Xtensive.Practices.Localization.Internals
{
  internal class LocalizationExpressionVisitor : ExpressionVisitor
  {
    public Expression VisitExpression(Expression query)
    {
      return Visit(query);
    }

    /// <inheritdoc/>
    protected override Expression VisitMemberAccess(MemberExpression me)
    {
      var domain = Domain.Demand();
      var map = domain.Extensions.Get<TypeLocalizationMap>();
      if (map == null)
        return me;

      var localizationInfo = map.Get(me.Member.ReflectedType);
      if (localizationInfo == null || !localizationInfo.LocalizationTypeInfo.Fields.Contains(me.Member.Name))
        return base.VisitMemberAccess(me);

      var localizationSetExpression = Expression.MakeMemberAccess(me.Expression, localizationInfo.LocalizationSetProperty);
      var localizationExpression = LocalizationExpressionBuilder.BuildExpression(localizationInfo, me);
      
      return localizationExpression.BindParameters(localizationSetExpression);
    }
  }
}