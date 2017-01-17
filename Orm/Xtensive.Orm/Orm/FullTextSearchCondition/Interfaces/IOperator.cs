// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using System.Collections.Generic;
using Xtensive.Orm.FullTextSearchCondition.Nodes;

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  /// <summary>
  /// Operator of a search condition.
  /// </summary>
  public interface IOperator : ISearchConditionNode
  {
    /// <summary>
    /// Source operand.
    /// </summary>
    IOperand Source { get; }

    /// <summary>
    /// Creates simple term and sets this instance as its source.
    /// </summary>
    /// <param name="term">A word or a phrase which will be treated as it is.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    IProximityOperand SimpleTerm(string term);

    /// <summary>
    /// Creates prefix term and sets this instance as its source.
    /// </summary>
    /// <param name="prefix">A word or a phrases which will be treated as prefix.
    ///  Asterisk char or another replacer of the following characters will be added authomatically.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    IProximityOperand PrefixTerm(string prefix);

    /// <summary>
    /// Creates generation term and sets this instance as its source.
    /// </summary>
    /// <param name="generationType">Algorighm which will be used for derrived terms generation.</param>
    /// <param name="terms">Collection of basis terms for generation.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    IGenerationTerm GenerationTerm(GenerationType generationType, ICollection<string> terms);

    /// <summary>
    /// Creates proximity term and sets this instance as its source.
    /// </summary>
    /// <param name="proximityTermsConstructor">A construction flow to create sequence of proximal terms.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    IProximityTerm GenericProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor);

    /// <summary>
    /// Creates proximity term and sets this instance as its source.
    /// </summary>
    /// <param name="proximityTermsConstructor">A construction flow to create sequence of proximal terms.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor);

    /// <summary>
    /// Creates proximity term and sets this instance as its source.
    /// </summary>
    /// <param name="proximityTermsConstructor">A construction flow to create sequence of proximal terms.</param>
    /// <param name="maximumDistance">Maximum distance between terms.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor, long maximumDistance);

    /// <summary>
    /// Creates proximity term and sets this instance as its source.
    /// </summary>
    /// <param name="proximityTermsConstructor">A construction flow to create sequence of proximal terms</param>
    /// <param name="maximumDistance">Maximum distance between terms.</param>
    /// <param name="matchOrder">Specifies whether order of terms is important.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    ICustomProximityTerm CustomProximityTerm(Func<ProximityOperandEndpoint, IProximityOperandsConstructionFlow> proximityTermsConstructor, long maximumDistance, bool matchOrder);

    /// <summary>
    /// Creates weighted term and sets this instance as its source.
    /// </summary>
    /// <param name="weightedTermsConstructor">A construction flow to create sequence of terms assigned with weights</param>
    /// <returns>Operand, ready to continue search condition building</returns>
    IWeightedTerm WeightedTerm(Func<WeightedTermEndpoint, IWeightedTermConstructionFlow> weightedTermsConstructor);

    /// <summary>
    /// Creates complex term and sets this instance as its source.
    /// </summary>
    /// <param name="complexTermConstructor">A construction flow to create wrapped terms.</param>
    /// <returns>Operand, ready to continue search condition building.</returns>
    IComplexTerm ComplexTerm(Func<ConditionEndpoint, IOperand> complexTermConstructor);
  }
}