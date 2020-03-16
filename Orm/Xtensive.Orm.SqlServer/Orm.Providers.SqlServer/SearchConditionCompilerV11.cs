// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Globalization;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.Providers.SqlServer
{
  internal class SearchConditionCompilerV11 : SearchConditionCompilerV09
  {
    public override void Visit(ICustomProximityTerm node)
    {
      if (node.Source != null)
        node.Source.AcceptVisitor(this);
      builder.Append("NEAR (");
      int index = 0;
      foreach (var proximityTerm in node.Terms) {
        if (index != 0)
          builder.Append(", ");
        proximityTerm.AcceptVisitor(this);
        index++;
      }
      if (node.MaxDistance.HasValue) {
        builder.Append(", ");
        if (node.MaxDistance.Value > (long)4294967295)
          builder.Append("MAX");
        else
          builder.Append(node.MaxDistance.Value.ToString(CultureInfo.InvariantCulture));
        if (node.MatchOrder)
        {
          builder.Append(", ");
          builder.Append(node.MatchOrder.ToString(CultureInfo.InvariantCulture).ToUpper());
        }
      }
      builder.Append(")");
    }
  }
}
