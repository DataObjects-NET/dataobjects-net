// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Collections.Generic;
using System.Globalization;
using Xtensive.Collections;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;

namespace Xtensive.Orm.FullTextSearchCondition
{
  public sealed class WeightedTermEndpoint : IWeightedTermConstructionFlow
  {
    private readonly IDictionary<IWeighableTerm, float?> weightedOperands;
    private readonly IOperator rootOperator;

    IDictionary<IWeighableTerm, float?> IWeightedTermConstructionFlow.WeightedOperands
    {
      get { return new ReadOnlyDictionary<IWeighableTerm, float?>(weightedOperands); }
    }

    public IWeightedTermConstructionFlow SimpleTerm(string term)
    {
      var operand = SearchConditionNodeFactory.CreateSimpleTerm(rootOperator, term);
      weightedOperands.Add(operand, null);
      return this;
    }

    public IWeightedTermConstructionFlow SimpleTerm(string term, float weight)
    {
      EnsureWeightIsCorrect(weight);
      var operand = SearchConditionNodeFactory.CreateSimpleTerm(rootOperator, term);
      weightedOperands.Add(operand, weight);
      return this;
    }

    public IWeightedTermConstructionFlow PrefixTerm(string prefix)
    {
      var operand = SearchConditionNodeFactory.CreatePrefixTerm(rootOperator, prefix);
      weightedOperands.Add(operand, null);
      return this;
    }

    public IWeightedTermConstructionFlow PrefixTerm(string prefix, float weight)
    {
      EnsureWeightIsCorrect(weight);
      var operand = SearchConditionNodeFactory.CreatePrefixTerm(rootOperator, prefix);
      weightedOperands.Add(operand, weight);
      return this;
    }

    public IWeightedTermConstructionFlow GenerationTerm(GenerationType generationType, ICollection<string> terms)
    {
      var operand = SearchConditionNodeFactory.CreateGenerationTerm(rootOperator, generationType, terms);
      weightedOperands.Add(operand, null);
      return this;
    }

    public IWeightedTermConstructionFlow GenerationTerm(GenerationType generationType, ICollection<string> terms, float weight)
    {
      EnsureWeightIsCorrect(weight);
      var operand = SearchConditionNodeFactory.CreateGenerationTerm(rootOperator, generationType, terms);
      weightedOperands.Add(operand, weight);
      return this;
    }

    public IWeightedTermConstructionFlow ProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityComposer)
    {
      var constructionFlow = proximityComposer.Invoke(new ProximityOperandEndpoint());
      var operand = SearchConditionNodeFactory.CreateGenericProximityTerm(rootOperator,constructionFlow.Operands);
      weightedOperands.Add(operand, null);
      return this;
    }

    public IWeightedTermConstructionFlow ProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityComposer, float weight)
    {
      EnsureWeightIsCorrect(weight);
      var constructionFlow = proximityComposer.Invoke(new ProximityOperandEndpoint());
      var operand = SearchConditionNodeFactory.CreateGenericProximityTerm(rootOperator, constructionFlow.Operands);
      weightedOperands.Add(operand, weight);
      return this;
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndSimpleTerm(string term)
    {
      return SimpleTerm(term);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndSimpleTerm(string term, float weight)
    {
      return SimpleTerm(term, weight);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndPrefixTerm(string prefix)
    {
      return PrefixTerm(prefix);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndPrefixTerm(string prefix, float weight)
    {
      return PrefixTerm(prefix, weight);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndGenerationTerm(GenerationType generationType, ICollection<string> terms)
    {
      return GenerationTerm(generationType, terms);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndGenerationTerm(GenerationType generationType, ICollection<string> terms, float weight)
    {
      return GenerationTerm(generationType, terms, weight);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityComposer)
    {
      return ProximityTerm(proximityComposer);
    }

    IWeightedTermConstructionFlow IWeightedTermConstructionFlow.AndProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityComposer, float weight)
    {
      return ProximityTerm(proximityComposer, weight);
    }

    private void EnsureWeightIsCorrect(float weight)
    {
      if (weight < 0 || weight > 1)
        throw new ArgumentException(
          string.Format(Strings.ExTermWeightValueMustBeBetweenXAndY, 0f.ToString("F1", CultureInfo.InvariantCulture), 1f.ToString("F1", CultureInfo.InvariantCulture)));
    }

    internal WeightedTermEndpoint()
    {
      rootOperator = SearchConditionNodeFactory.CreateConditonRoot();
      weightedOperands = new Dictionary<IWeighableTerm, float?>();
    }
  }
}