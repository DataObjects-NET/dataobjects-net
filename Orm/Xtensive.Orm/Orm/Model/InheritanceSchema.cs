// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2007.11.14

namespace Xtensive.Orm.Model
{
  /// <summary>
  /// Enumerates all supported 'class to tables mapping' schemes.
  /// </summary>
  /// <remarks>See M.Fowler - "Patterns of Enterprise Application Architecture".</remarks>
  public enum InheritanceSchema
  {
    /// <summary>
    /// Is equal to <see cref="ClassTable"/>.
    /// </summary>
    Default = ClassTable,
    /// <summary>
    /// One table per class in the inheritance structure. Inherited properties are stored in the parent class.
    /// </summary>
    ClassTable = 0,
    /// <summary>
    /// Maps all fields of all classes of an inheritance structure into a single table.
    /// </summary>
    SingleTable = 1,
    /// <summary>
    /// One table for each concrete class in the inheritance hierarchy. 
    /// Inherited properties are duplicated in the descendant tables.
    /// </summary>
    ConcreteTable = 2
  }
}