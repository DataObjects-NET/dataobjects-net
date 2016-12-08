using System.Collections.Generic;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class WeightedTerm : Operand, IWeightedTerm
  {
    public IDictionary<IWeighableTerm, float?> WeighedTerms { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal WeightedTerm(IOperator source, IDictionary<IWeighableTerm, float?> weightedTerms)
      : base(SearchConditionNodeType.WeightedTerm, source)
    {
      WeighedTerms = weightedTerms;
    }
  }
}