// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.01.11

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Defines root type of hierarchy of persistent types.
  /// </summary>
  /// <remarks>
  /// <para>
  /// All entities in your model can be divided into one or more persistent hierarchies. 
  /// Persistent hierarchy is a set of entities, that are inherited from one entity class(hierarchy root) 
  /// and have the same key structure. Hierarchy root entity should be marked by this attribute.
  /// </para>
  /// <para>
  /// Persistent hierarchies can use different inheritance schemes, e.g. all instances of hierarchy can be 
  /// stored in a single table or different tables should be crated for each entity class. Inheritance schema
  /// can be specified in <see cref="InheritanceSchema"/> property.
  /// </para>
  /// </remarks>
  /// <example>In following example two persistent type hierarchies are declared.
  /// Inheritance schema is specified for documents hierarchy.
  /// <code>
  /// [HerarchyRoot]
  /// public class Product : Entity  { ... }
  /// 
  /// [HerarchyRoot(InheritanceSchema = InheritaceSchema.ClassTable)
  /// public class Document : Entity  { ... }
  /// 
  /// public class Invoice : Document { ... }
  /// </code>
  /// </example>
  [Serializable]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class HierarchyRootAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the inheritance schema for this hierarchy.
    /// </summary>
    /// <remarks>
    /// Persistent hierarchies can use diffirent inheritance schemas, e.g. all instances of hierarchy can be 
    /// stored in a single table or different tables should be crated for each entity class.
    /// </remarks>
    public InheritanceSchema InheritanceSchema { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether key should include TypeId field.
    /// </summary>
    /// <remarks>
    /// TypeId can be included into entity Key for some specific optimization purposes.
    /// Default value is <see langword="false" />.
    /// </remarks>
    public bool IncludeTypeId { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public HierarchyRootAttribute()
      : this(InheritanceSchema.Default)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="schema">The inheritance schema for the hierarchy.</param>
    public HierarchyRootAttribute(InheritanceSchema schema)
    {
      InheritanceSchema = schema;
    }
  }
}