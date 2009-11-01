// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Dom.Dml
{
  [Serializable]
  public enum SqlFunctionType
  {
    BitLength,
    CharLength,
    Concat,
//    Convert,
    CurrentDate,
    CurrentTime,
    CurrentTimeStamp,
    Extract,
    Length,
    Lower,
    OctetLength,
    Position,
    Substring,
//    Translate,
//    Trim,
    Upper,
    UserDefined,
    CurrentUser,
    SessionUser,
    SystemUser,
    User,
    NullIf,
    Coalesce,
    // mathematical functions
    Abs,
    Acos,
    Asin,
    Atan,
    Ceiling,
    Cos,
    Cot,
    Degrees,
    Exp,
    Floor,
    Log,
    Log10,
    Pi,
    Power,
    Radians,
    Rand,
    Round,
    Sign,
    Sqrt,
    Square,
    Tan
  }
}
