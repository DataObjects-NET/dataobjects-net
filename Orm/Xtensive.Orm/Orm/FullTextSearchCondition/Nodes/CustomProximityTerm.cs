using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public sealed class CustomProximityTerm : Operand, ICustomProximityTerm
  {
    public IList<IProximityOperand> Terms { get; private set; }

    public long? MaxDistance { get; private set; }

    public bool MatchOrder { get; private set; }

    public override void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      visitor.Visit(this);
    }

    internal CustomProximityTerm(IOperator source, ICollection<IProximityOperand> proximityTerms)
      : this(source, proximityTerms, int.MaxValue, false)
    {
      MaxDistance = null;
    }

    internal CustomProximityTerm(IOperator source, ICollection<IProximityOperand> proximityTerms, long maxDistance)
      : this(source, proximityTerms, maxDistance, false)
    {
    }

    internal CustomProximityTerm(IOperator source, ICollection<IProximityOperand> proximityTerms, long maxDistance, bool matchOrder)
      : base(SearchConditionNodeType.CustomProximityTerm, source)
    {
      if (proximityTerms.Count < 2)
        throw new ArgumentException(string.Format(Strings.ExCollectionShouldContainAtLeastXElements, 2));
      ArgumentValidator.EnsureArgumentNotNull(proximityTerms, "proximityTerms");
      ArgumentValidator.EnsureArgumentIsGreaterThanOrEqual(maxDistance, 0, "maxDistance");

      Terms = new ReadOnlyList<IProximityOperand>(proximityTerms.ToList());
      MaxDistance = maxDistance;
      MatchOrder = matchOrder;
    }
  }
}