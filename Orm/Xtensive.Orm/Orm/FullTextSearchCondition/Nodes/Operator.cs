using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  public abstract class Operator : IOperator
  {
    public IOperand Source { get; private set; }

    public SearchConditionNodeType NodeType { get; private set; }

    public IProximityOperand SimpleTerm(string term)
    {
      return SearchConditionNodeFactory.CreateSimpleTerm(this, term);
    }

    public IProximityOperand PrefixTerm(string prefix)
    {
      return SearchConditionNodeFactory.CreatePrefixTerm(this, prefix);
    }

    public IGenerationTerm GenerationTerm(GenerationType generationType, ICollection<string> terms)
    {
      return SearchConditionNodeFactory.CreateGenerationTerm(this, generationType, terms);
    }

    public IProximityTerm GenericProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms)
    {
      ArgumentValidator.EnsureArgumentNotNull(proximityTerms, "proximityTerms");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTerms.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateGenericProximityTerm(this, constructionFlow.Operands);
    }

    public ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms)
    {
      ArgumentValidator.EnsureArgumentNotNull(proximityTerms, "proximityTerms");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTerms.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateCustomProximityTerm(this, constructionFlow.Operands);
    }

    public ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms, long maximumDistance)
    {
      ArgumentValidator.EnsureArgumentNotNull(proximityTerms, "proximityTerms");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTerms.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateCustomProximityTerm(this, constructionFlow.Operands, maximumDistance);
    }

    public ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTerms, long maximumDistance, bool matchOrder)
    {
      ArgumentValidator.EnsureArgumentNotNull(proximityTerms, "proximityTerms");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTerms.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateCustomProximityTerm(this, constructionFlow.Operands, maximumDistance, matchOrder);
    }

    public IWeightedTerm WeightedTerm(Func<WeightedTermEndpoint, IWeightedTermConstructionFlow> weightedTerm)
    {
      ArgumentValidator.EnsureArgumentNotNull(weightedTerm, "weightedTerm");

      var endpoint = new WeightedTermEndpoint();
      var constructionFlow = weightedTerm.Invoke(endpoint);
      return SearchConditionNodeFactory.CreateWeightedTerm(this, constructionFlow.WeightedOperands);
    }

    public IComplexTerm ComplexTerm(Func<ConditionEndpoint, IOperand> complexTerm)
    {
      ArgumentValidator.EnsureArgumentNotNull(complexTerm, "complexTerm");

      var endpoint = SearchConditionNodeFactory.CreateConditonRoot();
      return SearchConditionNodeFactory.CreateComplexTerm(this, complexTerm.Invoke(endpoint));
    }

    public void AcceptVisitor(ISearchConditionNodeVisitor visitor)
    {
      AcceptVisitorInternal(visitor);
    }

    protected abstract void AcceptVisitorInternal(ISearchConditionNodeVisitor visitor);

    internal Operator(SearchConditionNodeType nodeType, IOperand sourceOperand)
    {
      NodeType = nodeType;
      Source = sourceOperand;
    }
  }
}