// Copyright (C) 2009-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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