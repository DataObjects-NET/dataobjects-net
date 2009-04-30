// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Building;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Persistent descriptor of an assembly with registered persistent types.
  /// Used for schema upgrade purposes.
  /// </summary>
  [SystemType(TypeId = 2)]
  [HierarchyRoot("Name")]
  public class Assembly : Entity
  {
    /// <summary>
    /// Gets or sets the name of the assembly.
    /// </summary>
    /// <value>The name of the assembly.</value>
    [Field(Length = 1024)]
    public string Name { get; private set; }

    /// <summary>
    /// Gets or sets the assembly version.
    /// </summary>
    [Field(Length = 64)]
    public string Version { get; set; }

    #region Event handlers

    /// <exception cref="Exception">Object is read-only.</exception>
    protected override void  OnSettingFieldValue(FieldInfo field, object value)
    {
      if (BuildingContext.Current==null)
        throw Exceptions.ObjectIsReadOnly(null);
      base.OnSettingFieldValue(field, value);
    }

    /// <exception cref="Exception">Object is read-only.</exception>
    protected override void OnRemove()
    {
      if (BuildingContext.Current==null)
        throw Exceptions.ObjectIsReadOnly(null);
      base.OnRemove();
    }

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
      return string.Format(Strings.MetadataAssemblyFormat, Name, Version);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The assembly name.</param>
    /// <exception cref="Exception">Object is read-only.</exception>
    public Assembly(string name) 
      : base(Tuple.Create(name))
    {
      if (BuildingContext.Current==null)
        throw Exceptions.ObjectIsReadOnly(null);
    }
  }
}