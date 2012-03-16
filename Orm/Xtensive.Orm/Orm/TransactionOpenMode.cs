// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.11.24

namespace Xtensive.Orm
{
  /// <summary>
  /// Describes transaction opening mode.
  /// </summary>
  public enum TransactionOpenMode
  {
    /// <summary>
    /// Existing transaction will be used if it is already open, 
    /// otherwise new transaction will be open.
    /// </summary>
    Auto = 1,

    /// <summary>
    /// New transaction will be open, i.e. nested one if some transaction is already open.
    /// </summary>
    New = 2,

    /// <summary>
    /// Default value is <see cref="Auto"/>.
    /// </summary>
    Default = Auto
  }
}