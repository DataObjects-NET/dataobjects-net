// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System.Collections.Generic;
using Xtensive.Collections;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;
using Xtensive.Orm.FullTextSearchCondition.Internals;

namespace Xtensive.Orm.FullTextSearchCondition
{
  public sealed class ProximityOperandEndpoint : IProximityOperandsConstructionFlow
  {
    private readonly IList<IProximityOperand> operands;
    private readonly IOperator rootOperator;

    IList<IProximityOperand> IProximityOperandsConstructionFlow.Operands
    {
      get { return new ReadOnlyList<IProximityOperand>(operands); }
    }

    public IProximityOperandsConstructionFlow SimpleTerm(string term)
    {
      var operand = SearchConditionNodeFactory.CreateSimpleTerm(rootOperator, term);
      operands.Add(operand);
      return this;
    }

    public IProximityOperandsConstructionFlow PrefixTerm(string prefix)
    {
      var operand = SearchConditionNodeFactory.CreatePrefixTerm(rootOperator, prefix);
      operands.Add(operand);
      return this;
    }

    IProximityOperandsConstructionFlow IProximityOperandsConstructionFlow.NearSimpleTerm(string term)
    {
      var operand = SearchConditionNodeFactory.CreateSimpleTerm(rootOperator, term);
      operands.Add(operand);
      return this;
    }

    IProximityOperandsConstructionFlow IProximityOperandsConstructionFlow.NearPrefixTerm(string prefix)
    {
      var operand = SearchConditionNodeFactory.CreatePrefixTerm(rootOperator, prefix);
      operands.Add(operand);
      return this;
    }

    internal ProximityOperandEndpoint()
    {
      rootOperator = SearchConditionNodeFactory.CreateConditonRoot();
      operands = new List<IProximityOperand>();
    }
  }
}