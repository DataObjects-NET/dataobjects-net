// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using Xtensive.IoC;

namespace Xtensive.Orm.Providers.Sql.Expressions
{
  internal class ExpressionTranslationScope : Scope<ExpressionTranslationContext>
  {
    public static new ExpressionTranslationContext CurrentContext
    {
      get { return Scope<ExpressionTranslationContext>.CurrentContext; }
    }

    public ExpressionTranslationScope(ProviderInfo providerInfo, Driver driver, BooleanExpressionConverter booleanExpressionConverter)
      : base(new ExpressionTranslationContext(providerInfo, driver, booleanExpressionConverter))
    {
    }
  }
}