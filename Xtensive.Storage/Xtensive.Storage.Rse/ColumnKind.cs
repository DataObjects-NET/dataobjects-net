// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2008.07.04

namespace Xtensive.Storage.Rse
{
  /// <summary>
  /// Kind of column.
  /// </summary>
  public enum ColumnKind
  {
    /// <summary>
    /// Column is not bound to the key.
    /// </summary>
    Unbound,
    
    /// <summary>
    /// Column is part of the key.
    /// </summary>
    PartOfKey,
    
    /// <summary>
    /// Column is related to the key.
    /// </summary>
    RelatedToKey,
  }
}