
using Xtensive.Orm.FullTextSearchCondition.Interfaces;

namespace Xtensive.Orm.Providers
{
  public abstract class SearchConditionCompiler : ISearchConditionNodeVisitor
  {
    public virtual string CurrentOutput { get { return string.Empty; } }

    public abstract void Visit(IOperator node);

    public abstract void Visit(ISimpleTerm node);

    public abstract void Visit(IPrefixTerm node);

    public abstract void Visit(IGenerationTerm node);

    public abstract void Visit(IProximityTerm node);

    public abstract void Visit(ICustomProximityTerm node);

    public abstract void Visit(IWeightedTerm node);

    public abstract void Visit(IComplexTerm node);
  }
}
