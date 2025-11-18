// Copyright (C) 2016-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Xtensive.Orm.FullTextSearchCondition;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.Providers.SqlServer
{
  internal class SearchConditionCompilerV11 : SearchConditionCompiler
  {
    
    protected readonly StringBuilder builder;

    public override string CurrentOutput => builder.ToString();

    public override void Visit(IOperator node)
    {
      node.Source?.AcceptVisitor(this);
      VisitNodeType(node.NodeType);
    }

    public override void Visit(ISimpleTerm node)
    {
      node.Source?.AcceptVisitor(this);
      AppendTerm(node.Term.Trim());
    }

    public override void Visit(IPrefixTerm node)
    {
      node.Source?.AcceptVisitor(this);
      _ = builder.Append($"\"{node.Prefix.Trim()}*\"");
    }

    public override void Visit(IGenerationTerm node)
    {
      node.Source?.AcceptVisitor(this);

      _ = builder.Append("FORMSOF (");
      _ = builder.Append(node.GenerationType.ToString().ToUpper());
      foreach (var term in node.Terms.Select(t => t.Trim())) {
        _ = builder.Append(CommaDelimiter);
        AppendTerm(term);
      }
      _ = builder.Append(ClosingParenthesis);
    }

    public override void Visit(IProximityTerm node)
    {
      node.Source?.AcceptVisitor(this);
      int index = 0;
      foreach (var proximityOperand in node.Terms) {
        if (index != 0) {
          _ = builder.Append(" NEAR ");
        }
        proximityOperand.AcceptVisitor(this);
        index++;
      }
    }

    public override void Visit(ICustomProximityTerm node)
    {
      node.Source?.AcceptVisitor(this);

      _ = builder.Append("NEAR (");
      int index = 0;
      foreach (var proximityTerm in node.Terms) {
        if (index != 0) {
          _ = builder.Append(CommaDelimiter);
        }
        proximityTerm.AcceptVisitor(this);
        index++;
      }
      if (node.MaxDistance.HasValue) {
        _ = builder.Append(CommaDelimiter)
          .Append((node.MaxDistance.Value > (long) 4294967295)
            ? "MAX"
            : node.MaxDistance.Value.ToString(CultureInfo.InvariantCulture));
        if (node.MatchOrder) {
          _ = builder
            .Append(CommaDelimiter)
            .Append(node.MatchOrder.ToString(CultureInfo.InvariantCulture).ToUpper());
        }
      }
      _ = builder.Append(ClosingParenthesis);
    }

    public override void Visit(IWeightedTerm node)
    {
      node.Source?.AcceptVisitor(this);

      _ = builder.Append("ISABOUT (");
      int index = 0;
      foreach (var pair in node.WeighedTerms) {
        if (index != 0) {
          _ = builder.Append(CommaDelimiter);
        }
        pair.Key.AcceptVisitor(this);
        if (pair.Value.HasValue) {
          _ = builder.Append($" WEIGHT ({pair.Value.Value.ToString("F3", CultureInfo.InvariantCulture)})");
        }
        index++;
      }

      _ = builder.Append(ClosingParenthesis);
    }

    public override void Visit(IComplexTerm node)
    {
      node.Source?.AcceptVisitor(this);
      _ = builder.Append(OpeningParenthesis);
      node.RootOperand.AcceptVisitor(this);
      _ = builder.Append(ClosingParenthesis);
    }

    private void VisitNodeType(in SearchConditionNodeType nodeType)
    {
      if (nodeType is SearchConditionNodeType.Root) {
        return;
      }
      _ = builder.Append(nodeType switch {
        SearchConditionNodeType.Or => " OR ",
        SearchConditionNodeType.And => " AND ",
        SearchConditionNodeType.AndNot => " AND NOT ",
        _ => throw new ArgumentException("Not supported operator")
      });
    }

    private void AppendTerm(string s)
    {
      _ = s.Contains(' ', StringComparison.Ordinal)
        ? builder.Append($"\"{s}\"")
        : builder.Append(s);
    }

    public SearchConditionCompilerV11()
    {
      builder = new StringBuilder();
    }
  }
}
