// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using Xtensive.Core;

namespace Xtensive.Orm.Providers
{
  internal class ExpressionTranslationScope : Scope<ExpressionTranslationContext>
  {
    public static new ExpressionTranslationContext CurrentContext
    {
      get { return Scope<ExpressionTranslationContext>.CurrentContext; }
    }

    public ExpressionTranslationScope(ProviderInfo providerInfo, StorageDriver driver, BooleanExpressionConverter booleanExpressionConverter)
      : base(new ExpressionTranslationContext(providerInfo, driver, booleanExpressionConverter))
    {
    }
  }
}