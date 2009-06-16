// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.10.01

using System;

namespace Xtensive.Storage
{
  /// <summary>
  /// Defines <see cref="MappingAttribute.MappingName">mapping name</see> for persistent type 
  /// (i.e. name of the table this class is mapped to).
  /// </summary>
  /// <example>In following example order's data will be stored in "MyOrderTable" table.
  /// <code>
  /// [Entity(MappingName = "OrdersTable")]
  /// public class Order : Document
  /// {
  ///   ...
  /// }
  /// </code>
  /// </example>
  /// <seealso cref="MappingAttribute.MappingName">MappingName property</seealso>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public class EntityAttribute : MappingAttribute
  {
    // Constructors

    /// <inheritdoc/>
    public EntityAttribute()
    {
    }

    /// <inheritdoc/>
    public EntityAttribute(string mappingName)
      : base(mappingName)
    {
    }
  }
}