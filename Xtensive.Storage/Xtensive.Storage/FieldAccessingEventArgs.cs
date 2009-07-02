// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.07.01

using System;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Argument of handlers for events which are raising when 
  /// a <see cref="Persistent"/>'s field is accessing.
  /// </summary>
  [Serializable]
  public class FieldAccessingEventArgs : EventArgs
  {
    /// <summary>
    /// The key of <see cref="Entity"/> which owns the accessed field.
    /// </summary>
    public readonly Key EntityKey;

    /// <summary>
    /// The accessed field.
    /// </summary>
    public readonly FieldInfo Field;

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="entityKey">The value of the <see cref="EntityKey"/> property.</param>
    /// <param name="field">The value of the <see cref="Field"/> property.</param>
    public FieldAccessingEventArgs(Key entityKey, FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(entityKey, "entityKey");
      ArgumentValidator.EnsureArgumentNotNull(field, "field");
      EntityKey = entityKey;
      Field = field;
    }
  }
}