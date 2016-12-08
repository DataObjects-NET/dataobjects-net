

namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  public interface ISearchConditionNodeVisitor
  {
    void Visit(IOperator node);
    void Visit(ISimpleTerm node);
    void Visit(IPrefixTerm node);
    void Visit(IGenerationTerm node);
    void Visit(IProximityTerm node);
    void Visit(ICustomProximityTerm node);
    void Visit(IWeightedTerm node);
  }
}