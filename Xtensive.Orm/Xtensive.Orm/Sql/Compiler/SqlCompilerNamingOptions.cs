// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.08.31

using System;

namespace Xtensive.Sql.Compiler
{
  [Flags]
  public enum SqlCompilerNamingOptions
  {
    TableAliasing = 1,
    TableQualifiedColumns = 2,
  }
}