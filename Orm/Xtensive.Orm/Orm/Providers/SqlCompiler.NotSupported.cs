// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using Xtensive.Orm.Rse.Providers;

namespace Xtensive.Orm.Providers
{
  public partial class SqlCompiler 
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitRaw(RawProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitFreeText(FreeTextProvider provider)
    {
      throw new NotSupportedException();
    }

    /// <inheritdoc/>
    protected override SqlProvider VisitContainsTable(ContainsTableProvider provider)
    {
      throw new NotSupportedException();
    }
  }
}