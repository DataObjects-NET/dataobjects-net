// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.11.25

namespace Xtensive.Storage.Rse.Compilation.Expressions
{
  public enum ExtendedExpressionType
  {
    RseResult = 1000,
    FieldAccess,
    ParameterAccess,
    IndexAccess,
    Range,
    Seek
  }
}