// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using System;
using System.Linq.Expressions;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  internal class ExpressionTranslationContext
  {
    public static ExpressionTranslationContext Current { get { return ExpressionTranslationScope.CurrentContext; } }

    public ProviderInfo ProviderInfo { get; private set; }
    public StorageDriver Driver { get; private set; }
    public BooleanExpressionConverter BooleanExpressionConverter { get; private set; }


    // Constructors

    public ExpressionTranslationContext(ProviderInfo providerInfo, StorageDriver driver, BooleanExpressionConverter booleanExpressionConverter)
    {
      ProviderInfo = providerInfo;
      Driver = driver;
      BooleanExpressionConverter = booleanExpressionConverter;
    }
  }
}