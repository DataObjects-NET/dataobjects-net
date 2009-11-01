// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.

using System;

namespace Xtensive.Sql.Common
{
  /// <summary>
  /// <para>Defines optional types of RDBMS entities.</para>
  /// <para>An exact number of supported entities could vary in depends
  /// of a certain RDBMS server. So you can combine the flags below
  /// to specify that capabilities.</para>
  /// </summary>
  [Flags]
  public enum ServerEntities
  {
    /// <summary>
    /// Indicates that RDBMS server does not support
    /// optional types of entities.
    /// </summary>
    None = 0x0,

    /// <summary>
    /// Indicates that RDBMS supports collations.
    /// </summary>
    Collations = 0x1,

    /// <summary>
    /// Indicates that RDBMS supports constraints.
    /// </summary>
    Constraints = 0x2,

    /// <summary>
    /// Indicates that RDBMS supports value domains.
    /// </summary>
    Domains = 0x4,

    /// <summary>
    /// Indicates that RDBMS supports events.
    /// </summary>
    Events = 0x8,

    /// <summary>
    /// Indicates that RDBMS supports schemas.
    /// </summary>
    Schemas = 0x10,

    /// <summary>
    /// Indicates that RDBMS supports sequences.
    /// </summary>
    Sequences = 0x20,

    /// <summary>
    /// Indicates that RDBMS supports stored procedures.
    /// </summary>
    StoredProcedures = 0x40,

    /// <summary>
    /// Indicates that RDBMS supports synonyms.
    /// </summary>
    Synonyms = 0x80,

    /// <summary>
    /// Indicates that RDBMS supports tablespaces (filegroups).
    /// </summary>
    Filegroups = 0x100,

    /// <summary>
    /// Indicates that RDBMS supports triggers.
    /// </summary>
    Triggers = 0x200,

    /// <summary>
    /// Indicates that RDBMS supports user defined functions.
    /// </summary>
    UserDefinedFunctions = 0x400,

    /// <summary>
    /// Indicates that RDBMS supports user defined types.
    /// </summary>
    UserDefinedTypes = 0x800,

    /// <summary>
    /// Indicates that RDBMS supports assertions.
    /// </summary>
    Assertions = 0x1000,

    /// <summary>
    /// Indicates that RDBMS supports character sets.
    /// </summary>
    CharacterSets = 0x2000,

    /// <summary>
    /// Indicates that RDBMS supports translations.
    /// </summary>
    Translations = 0x4000,

    /// <summary>
    /// Indicates that RDBMS supports partition functions.
    /// </summary>
    PartitionFunctions = 0x8000,

    /// <summary>
    /// Indicates that RDBMS supports partition schemes.
    /// </summary>
    PartitionSchemes = 0x10000,

    /// <summary>
    /// Indicates that RDBMS supports indexes.
    /// </summary>
    Indexes = 0x20000,
  }
}
