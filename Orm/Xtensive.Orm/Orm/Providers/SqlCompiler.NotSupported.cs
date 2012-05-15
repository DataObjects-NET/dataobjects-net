// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.11.13

using System;
using Xtensive.Orm.Rse.Providers.Compilable;

namespace Xtensive.Orm.Providers
{
  partial class SqlCompiler 
  {
    /// <inheritdoc/>
    protected override SqlProvider VisitRaw(RawProvider provider)
    {
      throw new NotSupportedException();
    }
  }
}