using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class GenericProximityTerm : Operand, IProximityTerm, IWeighableTerm
  {
    public IList<IProximityOperand> Terms { get; private set; }

    protected override void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal GenericProximityTerm(IOperator source, ICollection<IProximityOperand> terms)
      : base(SearchConditionNodeType.GenericProximityTerm, source)
    {
      if (terms.Count < 2)
        throw new ArgumentException(string.Format(Strings.ExCollectionShouldContainAtLeastXElements, 2));
      Terms = new ReadOnlyList<IProximityOperand>(terms.ToList());
    }
  }
}