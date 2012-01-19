// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Info
{
  /// <summary>
  /// <para>Defines possible column categories.</para>
  /// <para>This enumeration has a <see cref="FlagsAttribute"/> attribute 
  /// that allows a bitwise combination of its member values.</para>
  /// </summary>
  /// <remarks>
  /// <para>It is well known that RDBMS servers stores data at table columns.
  /// Most of columns requires that their values have been specified by user
  /// but there are some exceptions of that rule (e.g. <see cref="ColumnFeatures.Identity"/>
  /// or <see cref="ColumnFeatures.Computed"/> columns).</para>
  /// <para>If you working on your own <see cref="SqlDriver">RDBMS driver</see> 
  /// implementation you have to provide correct <see cref="ServerInfo">information</see>
  /// about RDBMS capabilities. As a consequence you have to specify what 
  /// column categories are supported (see example below).</para>
  /// </remarks>
  /// <example>
  /// </example>
  [Flags]
  public enum ColumnFeatures
  {
    /// <summary>
    /// RDBMS server does not support any special columns.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// RDBMS server supports identity columns.
    /// </summary>
    Identity = 0x1,

    /// <summary>
    /// RDBMS server supports computed columns.
    /// </summary>
    Computed = 0x2,
  }
}
