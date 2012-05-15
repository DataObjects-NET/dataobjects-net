// Copyright (C) 2011 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2011.10.13

using System;
using System.Diagnostics;
using System.Text;

namespace Xtensive.Orm.Providers.PostgreSql
{
  public class PartialIndexFilterNormalizer : Providers.PartialIndexFilterNormalizer
  {
    /// <inheritdoc/>
    public override string Normalize(string expression)
    {
      var builder = new StringBuilder(expression);
      builder.Replace("(", string.Empty);
      builder.Replace(")", string.Empty);
      builder.Replace("E'", "'");
      builder.Replace("::text", string.Empty);
      return builder.ToString();
    }
  }
}