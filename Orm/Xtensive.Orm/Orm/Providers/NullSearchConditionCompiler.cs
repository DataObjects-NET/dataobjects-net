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
  }
}