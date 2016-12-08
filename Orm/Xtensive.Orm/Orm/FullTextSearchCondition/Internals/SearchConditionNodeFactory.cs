
using System.Collections.Generic;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Nodes;

namespace Xtensive.Orm.FullTextSearchCondition.Internals
{
  internal static class SearchConditionNodeFactory
  {
    public static Or CreateOr(IOperand source)
    {
      return new Or(source);
    }

    public static And CreateAnd(IOperand source)
    {
      return new And(source);
    }

    public static AndNot CreateAndNot(IOperand source)
    {
      return new AndNot(source);
    }

    public static SimpleTerm CreateSimpleTerm(IOperator source, string term)
    {
      return new SimpleTerm(source, term);
    }

    public static PrefixTerm CreatePrefixTerm(IOperator source, string prefix)
    {
      return new PrefixTerm(source, prefix);
    }

    public static GenerationTerm CreateGenerationTerm(IOperator source, GenerationType generationType, ICollection<string> terms)
    {
      return new GenerationTerm(source, generationType, terms);
    }

    public static GenericProximityTerm CreateGenericProximityTerm(IOperator source, ICollection<IProximityOperand> terms)
    {
      return new GenericProximityTerm(source, terms);
    }

    public static CustomProximityTerm CreateCustomProximityTerm(IOperator source, ICollection<IProximityOperand> terms)
    {
      return new CustomProximityTerm(source, terms);
    }

    public static CustomProximityTerm CreateCustomProximityTerm(IOperator source, ICollection<IProximityOperand> terms, long maximumDistance)
    {
      return new CustomProximityTerm(source, terms, maximumDistance);
    }

    public static CustomProximityTerm CreateCustomProximityTerm(IOperator source, ICollection<IProximityOperand> terms, long maximumDistance, bool matchOrder)
    {
      return new CustomProximityTerm(source, terms, maximumDistance, matchOrder);
    }

    public static WeightedTerm CreateWeightedTerm(IOperator source, IDictionary<IWeighableTerm, float?> weightedTerms)
    {
      return new WeightedTerm(source, weightedTerms);
    }

    public static ConditionEndpoint CreateConditonRoot()
    {
      return new ConditionEndpoint();
    }
  }
}
