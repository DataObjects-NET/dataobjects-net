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
  }
}