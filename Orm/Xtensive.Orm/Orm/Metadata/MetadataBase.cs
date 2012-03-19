// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.14

using System;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Building;
using Xtensive.Orm.Model;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Abstract base class for any metadata type.
  /// </summary>
  [Serializable]
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

    private void EnsureIsWritable()
    {
      if (Upgrade.UpgradeContext.Current==null)
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