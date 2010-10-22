// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.09.18

using System;
using Xtensive.Sql;

namespace Xtensive.Storage.Providers.Sql.Expressions
{
  internal class ExpressionTranslationContext
  {
    public static ExpressionTranslationContext Current { get { return ExpressionTranslationScope.CurrentContext; } }

    public Driver Driver { get; private set; }

    // Constructors

    public ExpressionTranslationContext(Driver driver)
    {
      Driver = driver;
    }
  }
}