// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

namespace Xtensive.Sql.Compiler
{
  public enum NodeSection
  {
    Entry = 0,
    Exit = 1,
  }

  public enum AlterTableSection
  {
    Entry = 0,
    Exit = 1,
    AddColumn = 2,
    AlterColumn = 3,
    DropColumn = 4,
    DropBehavior = 5,
    AddConstraint = 6,
    DropConstraint = 7,
    RenameColumn = 8,
    To = 9,
  }

  public enum AlterDomainSection
  {
    Entry = 0,
    Exit = 1,
    AddConstraint = 2,
    DropConstraint = 3,
    SetDefault = 4,
    DropDefault = 5,
  }

  public enum ConstraintSection
  {
    Entry = 0,
    Exit = 1,
    Check = 2,
    PrimaryKey = 3,
    Unique = 4,
    ForeignKey = 5,
    ReferencedColumns = 6,
  }
  
  public enum BetweenSection
  {
    Entry = 0,
    Exit = 1,
    Between = 2,
    And = 3,
  }

  public enum CaseSection
  {
    Entry = 0,
    Exit = 1,
    Value = 2,
    When = 3,
    Then = 4,
    Else = 5,
  }

  public enum ColumnSection
  {
    Entry = 0,
    Exit = 1,
    AliasDeclaration = 2,
  }

  public enum CreateDomainSection
  {
    Entry = 0,
    Exit = 1,
    DomainDefaultValue = 2,
    DomainCollate = 3,
  }

  public enum CreateIndexSection
  {
    Entry = 0,
    Exit = 1,
    ColumnsEnter = 2,
    ColumnsExit = 3,
    NonkeyColumnsEnter = 4,
    NonkeyColumnsExit = 5,
    StorageOptions = 6,
    Where = 7,
  }

  public enum CreateTableSection
  {
    Entry = 0,
    TableElementsEntry = 1,
    TableElementEntry = 2,
    TableElementExit = 3,
    TableElementsExit = 4,
    Partition = 5,
    Exit = 6,
  }

  public enum DeclareCursorSection
  {
    Entry = 0,
    Exit = 1,
    Holdability = 2,
    Scrollability = 3,
    Sensivity = 4,
    Cursor = 5,
    Updatability = 6,
    Returnability = 7,
    For = 8
  }

  public enum DeleteSection
  {
    Entry = 0,
    Exit = 1,
    Where = 2,
    From = 3,
  }

  public enum FetchSection
  {
    Entry = 0,
    Exit = 1,
    Targets = 2,
  }
  
  public enum FunctionCallSection
  {
    Entry = 0,
    Exit = 1,
    ArgumentDelimiter = 2,
    ArgumentEntry = 3,
    ArgumentExit = 4,
  }

  public enum SequenceDescriptorSection
  {
    StartValue = 0,
    Increment = 1,
    MaxValue = 2,
    MinValue = 3,
    IsCyclic = 4,
    RestartValue = 5,
    AlterMaxValue = 6,
    AlterMinValue = 7,
  }

  public enum IfSection
  {
    Entry = 0,
    Exit = 1,
    True = 2,
    False = 3,
  }

  public enum InsertSection
  {
    Entry = 0,
    Exit = 1,
    ColumnsEntry = 2,
    ColumnsExit = 4,
    ValuesEntry = 3,
    ValuesExit = 5,
    DefaultValues = 6,
  }

  public enum JoinSection
  {
    Entry = 0,
    Exit = 1,
    Specification = 2,
    Condition = 3,
  }

  public enum LikeSection
  {
    Entry = 0,
    Exit = 1,
    Like = 2,
    Escape = 3,
  }

  public enum MatchSection
  {
    Entry = 0,
    Exit = 1,
    Specification = 2,
  }

  public enum SelectSection
  {
    Entry = 0,
    Exit = 1,
    From = 3,
    Where = 4,
    GroupBy = 5,
    Having = 6,
    OrderBy = 7,
    HintsEntry = 8,
    HintsExit = 9,
    Limit = 10,
    Offset = 11,
  }

  public enum TableSection
  {
    Entry = 0,
    Exit = 1,
    AliasDeclaration = 2,
  }

  public enum TableColumnSection
  {
    Entry = 0,
    Exit = 1,
    Type = 2,
    DefaultValue = 3,
    DropDefault = 4,
    SetDefault = 5,
    GeneratedEntry = 6,
    GeneratedExit = 7,
    SetIdentityInfoElement = 8,
    GenerationExpressionEntry = 9,
    GenerationExpressionExit = 10,
    NotNull = 11,
    Collate = 12,
  }

  public enum TrimSection
  {
    Entry = 0,
    Exit = 1,
    From = 2,
  }

  public enum ExtractSection
  {
    Entry = 0,
    Exit = 1,
    From = 2,
  }

  public enum ArraySection
  {
    Entry = 0,
    Exit = 1,
    EmptyArray = 2,
  }

  public enum UpdateSection
  {
    Entry = 0,
    Exit = 1,
    Set = 2,
    From = 3,
    Where = 4,
  }

  public enum WhileSection
  {
    Entry = 0,
    Exit = 1,
    Statement = 2,
  }

  public enum QueryExpressionSection
  {
    Entry = 0,
    Exit = 1,
    All = 2,
  }
}