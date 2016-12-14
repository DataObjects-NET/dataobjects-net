// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

namespace Xtensive.Orm.FullTextSearchCondition
{
  public enum SearchConditionNodeType
  {
    Root,
    Or,
    And,
    AndNot,
    SimpleTerm,
    Prefix,
    GenerationTerm,
    GenericProximityTerm,
    CustomProximityTerm,
    WeightedTerm,
    ComplexTerm,
  }
}