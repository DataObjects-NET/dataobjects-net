// Copyright (C) 2003-2016 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2016.12.08

using System;
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.Providers
{
  public sealed class NullSearchConditionCompiler : SearchConditionCompiler
  {
    public override void Visit(IOperator node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(ISimpleTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(IPrefixTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(IGenerationTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(IProximityTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(ICustomProximityTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(IWeightedTerm node)
    {
      throw new NotSupportedException();
    }

    public override void Visit(IComplexTerm node)
    {
      throw new NotSupportedException();
    }
  }
}