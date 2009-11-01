// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.06

namespace Xtensive.Storage
{
  /// <summary>
  /// Enumerates possible storage build modes.
  /// <seealso cref="Domain.Build"/>
  /// <seealso cref="Domain"/>
  /// </summary>
  public enum DomainBuildMode
  {
    /// <summary>
    /// Default update mode.
    /// The same as <see cref="Perform"/>
    /// Value is <see langword="0x13"/>. 
    /// </summary>
    Default = 0x13,
    /// <summary>
    /// Database update should be skipped.
    /// Extracted database model will contain incomplete
    /// information about existing database (only a
    /// list of types will be extracted in this mode).
    /// Value is <see langword="0x0"/>. 
    /// </summary>
    Skip = 0x0,
    /// <summary>
    /// Database update should be skipped, but 
    /// database model should be extracted.
    /// Value is <see langword="0x1"/>. 
    /// </summary>
    SkipButExtract = 0x1,
    /// <summary>
    /// Database update should be skipped, but 
    /// database model should be extracted and compared
    /// to the new database model.
    /// Value is <see langword="0x3"/>. 
    /// </summary>
    SkipButExtractAndCompare = 0x3,
    /// <summary>
    /// Database update will be performed.
    /// All update types except row-level integrity checks
    /// are acceptable in this mode.
    /// Value is <see langword="0x13"/>. 
    /// </summary>
    Perform = 0x13,
    /// <summary>
    /// Database update will be performed.
    /// Instances with unknown types will not be removed
    /// in this mode (an exception will be thrown
    /// if such type exists in the database).
    /// Value is <see langword="0x33"/>. 
    /// </summary>
    PerformSafe = 0x33,
    /// <summary>
    /// Database update with row-level integrity checks
    /// will be performed.
    /// All update types are acceptable in this mode.
    /// Note that this update mode is the slowest one (all foreign
    /// keys are recreated in this mode), generally
    /// it should be used when you significantly change
    /// inheritance hierarchy of persistent types, for example,
    /// move a persistent type (which instances exist in the 
    /// database) to different node in the inheritance hierarchy
    /// (change its base type).
    /// One more case when it can be used is when
    /// <see cref="DomainBuildMode.Perform"/> update mode is
    /// incapable to resolve the problem (it may fail e.g.
    /// when an intermediate class is inserted into existing
    /// inheritance hierarchy, and database already contain instances
    /// of its descendants - in this case DataObjects.NET will try
    /// to update foreign key for identifying columns 
    /// of each descendant-related table, but since ancestor-related
    /// table contains no rows, foreign key violation will occur).
    /// Value is <see langword="0x53"/>. 
    /// </summary>
    PerformComplete = 0x53,
    /// <summary>
    /// Recreates all database structures. Database will
    /// contain no any instances after this type of update.
    /// Value is <see langword="0x113"/>. 
    /// </summary>
    Recreate = 0x113,
    /// <summary>
    /// <see cref="DomainUpdateIsBlockedException"/> will be 
    /// thrown on update attempt.
    /// Value is <see langword="0x1003"/>. 
    /// </summary>
    Block = 0x1003,
  }
}