// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// <para>Defines a list of typical features of identity columns.</para>
  /// <para>You can combine this features to describe certain RDBMS capabilities.</para>
  /// </summary>
  [Flags]
  public enum IdentityFeatures
  {
    /// <summary>
    /// Indicates that RDBMS does not support any feature in the list.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS allows to specify start value
    /// for an identity column.
    /// </summary>
    StartValue = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports automaticaly incrementing
    /// identity columns.
    /// </summary>
    Increment = 0x2,

    /// <summary>
    /// Indicates that RDBMS allows to specify minimum value
    /// for an identity column.
    /// </summary>
    Minimum = 0x4,

    /// <summary>
    /// Indicates that RDBMS allows to specify maximun value
    /// for an identity column.
    /// </summary>
    Maximum = 0x8,

    /// <summary>
    /// Indicates that RDBMS allows cycles in generated
    /// sequences of identity column values.
    /// </summary>
    Cycle = 0x10,
  }
}
