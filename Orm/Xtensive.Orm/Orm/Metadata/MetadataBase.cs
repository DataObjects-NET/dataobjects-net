// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.05.14

using System;
using Xtensive.Core;

using Xtensive.Orm.Model;

namespace Xtensive.Orm.Metadata
{
  /// <summary>
  /// Abstract base class for any metadata type.
  /// </summary>
  [Serializable]
  public abstract class MetadataBase : Entity
  {
    /// <exception cref="Exception">Object is read-only.</exception>
    protected override void OnSettingFieldValue(FieldInfo field, object value)
    {
      ThrowObjectIsReadOnly();
    }

    /// <exception cref="Exception">Object is read-only.</exception>
    protected override void OnRemove()
    {
      ThrowObjectIsReadOnly();
    }

    private void ThrowObjectIsReadOnly()
    {
      throw Exceptions.ObjectIsReadOnly(null);
    }


    // Constructors

    internal MetadataBase(Session session, EntityState state)
      : base(session, state)
    {
    }
  }
}