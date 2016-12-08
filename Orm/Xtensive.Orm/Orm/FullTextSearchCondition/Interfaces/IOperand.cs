namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  public interface IOperand : ISearchConditionNode
  {
    IOperator Source { get; }

    IOperator And();
    IOperator Or();
    IOperator AndNot();
  }
}