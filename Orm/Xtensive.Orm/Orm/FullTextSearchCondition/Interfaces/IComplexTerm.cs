namespace Xtensive.Orm.FullTextSearchCondition.Interfaces
{
  public interface IComplexTerm : IOperand
  {
    IOperand RootOperand { get; }
  }
}