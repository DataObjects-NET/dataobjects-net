// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;

namespace Xtensive.Orm.FullTextSearchCondition.Nodes
{
  /// <summary>
  /// <see cref="IOperator"/> imlementation, base class for other operators
  /// </summary>
  public abstract class Operator : IOperator
  {
    /// <inheritdoc/>
    public IOperand Source { get; private set; }

    /// <inheritdoc/>
    public SearchConditionNodeType NodeType { get; private set; }

    /// <inheritdoc/>
    public IProximityOperand SimpleTerm(string term)
    {
      return SearchConditionNodeFactory.CreateSimpleTerm(this, term);
    }

    /// <inheritdoc/>
    public IProximityOperand PrefixTerm(string prefix)
    {
      return SearchConditionNodeFactory.CreatePrefixTerm(this, prefix);
    }

    /// <inheritdoc/>
    public IGenerationTerm GenerationTerm(GenerationType generationType, ICollection<string> terms)
    {
      return SearchConditionNodeFactory.CreateGenerationTerm(this, generationType, terms);
    }

    /// <inheritdoc/>
    public IProximityTerm GenericProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor)
    {
      ArgumentNullException.ThrowIfNull(proximityTermsConstructor, "proximityTermsConstructor");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTermsConstructor.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateGenericProximityTerm(this, constructionFlow.Operands);
    }

    /// <inheritdoc/>
    public ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor)
    {
      ArgumentNullException.ThrowIfNull(proximityTermsConstructor, "proximityTermsConstructor");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTermsConstructor.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateCustomProximityTerm(this, constructionFlow.Operands);
    }

    /// <inheritdoc/>
    public ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor, long maximumDistance)
    {
      ArgumentNullException.ThrowIfNull(proximityTermsConstructor, "proximityTermsConstructor");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTermsConstructor.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateCustomProximityTerm(this, constructionFlow.Operands, maximumDistance);
    }

    /// <inheritdoc/>
    public ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor, long maximumDistance, bool matchOrder)
    {
      ArgumentNullException.ThrowIfNull(proximityTermsConstructor, "proximityTermsConstructor");

      var proximityOperandRoot = new ProximityOperandEndpoint();
      var constructionFlow = proximityTermsConstructor.Invoke(proximityOperandRoot);
      return SearchConditionNodeFactory.CreateCustomProximityTerm(this, constructionFlow.Operands, maximumDistance, matchOrder);
    }

    /// <inheritdoc/>
    public IWeightedTerm WeightedTerm(Func<WeightedTermEndpoint, IWeightedTermConstructionFlow> weightedTermsConstructor)
    {
      ArgumentNullException.ThrowIfNull(weightedTermsConstructor, "weightedTermsConstructor");

      var endpoint = new WeightedTermEndpoint();
      var constructionFlow = weightedTermsConstructor.Invoke(endpoint);
      return SearchConditionNodeFactory.CreateWeightedTerm(this, constructionFlow.WeightedOperands);
    }

    /// <inheritdoc/>
    public IComplexTerm ComplexTerm(Func<ConditionEndpoint, IOperand> complexTermConstructor)
    {
      ArgumentNullException.ThrowIfNull(complexTermConstructor, "complexTermConstructor");

      var endpoint = SearchConditionNodeFactory.CreateConditonRoot();
      return SearchConditionNodeFactory.CreateComplexTerm(this, complexTermConstructor.Invoke(endpoint));
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