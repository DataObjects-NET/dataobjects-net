using System;
using System.Collections.Generic;
using Xtensive.Orm.FullTextSearchCondition.Nodes;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  public interface IOperator : ISearchConditionNode
  {
    IOperand Source { get; }

    IProximityOperand SimpleTerm(string term);

    IProximityOperand PrefixTerm(string prefix);

    IGenerationTerm GenerationTerm(GenerationType generationType, ICollection<string> terms);

    IProximityTerm GenericProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms);

    ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms);

    ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms, long maximumDistance);

    ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms, long maximumDistanc, bool matchOrder);

    IWeightedTerm WeightedTerm(Func<WeightedTermEndpoint, IWeightedTermConstructionFlow> weightedTerms);

    IComplexTerm ComplexTerm(Func<ConditionEndpoint, IOperand> complexTerm);
  }
}