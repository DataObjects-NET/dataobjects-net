// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.21

using System;
using System.Collections.Generic;
using System.Diagnostics;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Building.Definitions
{
  /// <summary>
  /// Defines full-text index.
  /// </summary>
  [Serializable]
  public sealed class FullTextIndexDef : Node
  {
    public TypeDef Type { get; private set; }
    public List<FullTextFieldDef> Fields { get; private set; }

    // Constructors

    /// <summary>
    /// Initializes a new instance of this class.
    /// </summary>
    public FullTextIndexDef(TypeDef type)
    {
      Type = type;
      Fields = new List<FullTextFieldDef>();
    }
  }
}