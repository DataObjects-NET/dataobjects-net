// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.14

using System;
using Xtensive.Core;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Building;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Metadata
{
  /// <summary>
  /// Abstract base class for any metadata type.
  /// </summary>
  public abstract class MetadataBase : Entity
  {
    #region Event handlers

    /// <exception cref="Exception">Object is read-only.</exception>
    protected override void  OnSettingFieldValue(FieldInfo field, object value)
    {
      EnsureIsWritable();
      base.OnSettingFieldValue(field, value);
    }

    /// <exception cref="Exception">Object is read-only.</exception>
    protected override void OnRemove()
    {
      EnsureIsWritable();
      base.OnRemove();
    }

    #endregion

    /// <summary>
    /// Ensures the entity is writable.
    /// </summary>
    /// <exception cref="Exception">Object is read-only.</exception>
    [Infrastructure]
    protected void EnsureIsWritable()
    {
      if (BuildingContext.Current==null)
        throw Exceptions.ObjectIsReadOnly(null);
    }


    // Constructors

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="id">The identifier.</param>
    protected MetadataBase(int id)
      : base(id)
    {
      EnsureIsWritable();
    }
    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="name">The identifier.</param>
    protected MetadataBase(string name)
      : base(name)
    {
      EnsureIsWritable();
    }
  }
}