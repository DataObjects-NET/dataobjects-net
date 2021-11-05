// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql
{
  [Serializable]
  public enum SqlNodeType
  {
    Action,
    Add,
    All,
    Alter,
    And,
    Any,
    Array,
    Assign,
    Avg,
    Batch,
    BeginEndBlock,
    Between,
    BitAnd,
    BitNot,
    BitOr,
    BitXor,
    Break,
    Case,
    Cast,
    CloseCursor,
    Collate,
    Column,
    ColumnRef,
    Command,
    Comment,
    Concat,
    Conditional,
    Continue,
    Container,
    Count,
    Create,
    Cursor,
    DateTimePlusInterval,
    DateTimeMinusInterval,
    DateTimeMinusDateTime,
    DateTimeOffsetPlusInterval,
    DateTimeOffsetMinusInterval,
    DateTimeOffsetMinusDateTimeOffset,
    DeclareCursor,
    DefaultValue,
    Delete,
    Divide,
    Drop,
    DynamicFilter,
    RawConcat,
    Equals,
    Except,
    Exists,
    Extract,
    Fetch,
    FunctionCall,
    CustomFunctionCall,
    GreaterThan,
    GreaterThanOrEquals,
    In,
    Insert,
    Intersect,
    IsNull,
    IsNotNull,
    Join,
    Hint,
    Placeholder,
    LessThan,
    LessThanOrEquals,
    Like,
    Literal,
    Match,
    Max,
    Min,
    Modulo,
    Multiply,
    Native,
    NextValue,
    Not,
    NotBetween,
    NotEquals,
    NotIn,
    Negate,
    Null,
    OpenCursor,
    Or,
    Order,
    Overlaps,
    Parameter,
    Rename,
    Row,
    RowNumber,
    Round,
    Select,
    Some,
    SubSelect,
    Subtract,
    Sum,
    Table,
    Trim,
    Union,
    Unique,
    Update,
    Variable,
    Variant,
    DeclareVariable,
    While,
    Fragment,
  }
}