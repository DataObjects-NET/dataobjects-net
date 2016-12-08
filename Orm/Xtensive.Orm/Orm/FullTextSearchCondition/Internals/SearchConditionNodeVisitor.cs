using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Nodes;

namespace Xtensive.Orm.FullTextSearchCondition.Internals
{
  internal class SearchConditionNodeVisitor : ISearchConditionNodeVisitor
  {
    private StringBuilder builder;

    public string CurrentOutput
    {
      get { return builder.ToString(); }
    }

    public void Visit(IOperator node)
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

    public void Visit(ConditionEndpoint node)
    {
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

      builder.Append(" AND ");
    }

    private void VisitAndNot(IOperator node)
    {
      var conditionEndpoint = node as AndNot;
      if (conditionEndpoint==null)
        throw new InvalidOperationException(string.Format(Strings.TypeXIsNotSupportedYNode, node.GetType(), node.NodeType));

      builder.Append(" AND NOT ");
    }

    private void VisitOr(IOperator node)
    {
      var conditionEndpoint = node as Or;
      if (conditionEndpoint==null)
        throw new InvalidOperationException(string.Format(Strings.TypeXIsNotSupportedYNode, node.GetType(), node.NodeType));

      builder.Append(" OR ");
    }

    public void Visit(ISimpleTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);

      var term = node.Term.Trim();
      if (term.IndexOf(' ')!=-1)
        term = string.Format("\"{0}\"", term);

      builder.Append(term);
    }

    public void Visit(IPrefixTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);

      builder.AppendFormat("\"{0}*\"", node.Prefix.Trim());
    }

    public void Visit(IGenerationTerm node)
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

    public void Visit(IProximityTerm node)
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

    public void Visit(ICustomProximityTerm node)
    {
      if (node.Source!=null)
        node.Source.AcceptVisitor(this);
      builder.Append("NEAR (");
      int index = 0;
      foreach (var proximityTerm in node.Terms) {
        if (index!=0)
          builder.Append(", ");
        proximityTerm.AcceptVisitor(this);
        index++;
      }
      if (node.MaxDistance.HasValue) {
        builder.Append(", ");
        if (node.MaxDistance.Value > (long) 4294967295)
          builder.Append("MAX");
        else
          builder.Append(node.MaxDistance.Value.ToString(CultureInfo.InvariantCulture));
        if (node.MatchOrder) {
          builder.Append(", ");
          builder.Append(node.MatchOrder.ToString(CultureInfo.InvariantCulture).ToUpper());
        }
      }
      builder.Append(")");
    }

    public void Visit(IWeightedTerm node)
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

    public SearchConditionNodeVisitor()
    {
      builder = new StringBuilder();
    }
  }
}