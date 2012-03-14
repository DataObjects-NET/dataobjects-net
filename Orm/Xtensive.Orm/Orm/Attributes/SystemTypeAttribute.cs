// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.12.24

using System;
using Xtensive.Internals.DocTemplates;
using Xtensive.Orm.Model;

namespace Xtensive.Orm
{
  /// <summary>
  /// Marks persistent type as a system type.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, 
    AllowMultiple = false, Inherited = false)]
  internal sealed class SystemTypeAttribute : StorageAttribute
  {
    /// <summary>
    /// Type identifier to preserve for it.
    /// </summary>
    public new int TypeId { get; set; }

    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public SystemTypeAttribute()
    {
      TypeId = TypeInfo.NoTypeId;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="typeId">The type identifier.</param>
    public SystemTypeAttribute(int typeId)
    {
      TypeId = typeId;
    }
  }
}