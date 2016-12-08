using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Xtensive.Orm.FullTextSearchCondition;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Nodes;

namespace Xtensive.Orm.Providers.SqlServer
{
  internal class SearchConditionCompilerV09 : SearchConditionCompiler
  {
    protected readonly StringBuilder builder;

    public override string CurrentOutput
    {
      get { return builder.ToString(); }
    }

    public override void Visit(IOperator node)
    {
      switch (node.NodeType) {
        case SearchConditionNodeType.Root:
          VisitRoot(node);
          break;
        case SearchConditionNodeType.Or:
          VisitOr(node);
          break;
        case SearchConditionNodeType.And:
          VisitAnd(node);
          break;
        case SearchConditionNodeType.AndNot:
          VisitAndNot(node);
          break;
        default:
          throw new ArgumentException("Not supported operator");
      }
    }

    public override void Visit(ISimpleTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);

      var term = node.Term.Trim();
      if (term.IndexOf(' ')!=-1)
        term = string.Format("\"{0}\"", term);

      builder.Append(term);
    }

    public override void Visit(IPrefixTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);

      builder.AppendFormat("\"{0}*\"", node.Prefix.Trim());
    }

    public override void Visit(IGenerationTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);

      builder.Append("FORMSOF (");
      builder.Append(node.GenerationType.ToString().ToUpper());
      foreach (var term in node.Terms.Select(t=>t.Trim())) {
        builder.Append(", ");
        if (term.IndexOf(' ')==-1)
          builder.Append(term);
        else
          builder.AppendFormat("\"{0}\"", term);
      }
      builder.Append(")");
    }

    public override void Visit(IProximityTerm node)
    {
      if(node.Source!=null)
        node.Source.AcceptVisitor(this);
      int index = 0;
      foreach (var proximityOperand in node.Terms) {
        if (index!=0) {
          builder.Append(" NEAR ");
        }
        proximityOperand.AcceptVisitor(this);
        index++;
      }
    }

    public override void Visit(ICustomProximityTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(IWeightedTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);

      builder.Append("ISABOUT (");
      int index = 0;
      foreach (var pair in node.WeighedTerms) {
        if (index!=0)
          builder.Append(", ");
        pair.Key.AcceptVisitor(this);
        if (pair.Value.HasValue)
          builder.AppendFormat(" WEIGHT ({0})", pair.Value.Value.ToString("F3", CultureInfo.InvariantCulture));
        index++;
      }

      builder.Append(")");
    }

    public override void Visit(IComplexTerm node)
    {
      if (node.Source != null)
        node.Source.AcceptVisitor(this);
      builder.Append("(");
      node.RootOperand.AcceptVisitor(this);
      builder.Append(")");
    }

    private void VisitRoot(IOperator node)
    {
      var conditionEndpoint = node as ConditionEndpoint;
      if (conditionEndpoint==null)
        throw new InvalidOperationException(string.Format(Strings.TypeXIsNotSupportedYNode, node.GetType(),node.NodeType) );
    }

    private void VisitAnd(IOperator node)
    {
      var conditionEndpoint = node as And;
      if (conditionEndpoint==null)
        throw new InvalidOperationException(string.Format(Strings.TypeXIsNotSupportedYNode, node.GetType(), node.NodeType));

      if (node.Source != null)
        node.Source.AcceptVisitor(this);
      builder.Append(" AND ");
    }

    private void VisitAndNot(IOperator node)
    {
      var conditionEndpoint = node as AndNot;
      if (conditionEndpoint==null)
        throw new InvalidOperationException(string.Format(Strings.TypeXIsNotSupportedYNode, node.GetType(), node.NodeType));

      if (node.Source != null)
        node.Source.AcceptVisitor(this);
      builder.Append(" AND NOT ");
    }

    private void VisitOr(IOperator node)
    {
      var conditionEndpoint = node as Or;
      if (conditionEndpoint==null)
        throw new InvalidOperationException(string.Format(Strings.TypeXIsNotSupportedYNode, node.GetType(), node.NodeType));

      if (node.Source != null)
        node.Source.AcceptVisitor(this);
      builder.Append(" OR ");
    }

    internal SearchConditionCompilerV09()
    {
      builder = new StringBuilder();
    }
  }
}
