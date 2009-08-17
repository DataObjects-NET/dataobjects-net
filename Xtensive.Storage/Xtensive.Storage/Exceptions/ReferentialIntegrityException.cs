// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Thrown on attempt to remove an object having
  /// reference with <see cref="OnRemoveAction.Deny"/>
  /// option pointing to it.
  /// </summary>
  [Serializable]
  public class ReferentialIntegrityException: StorageException
  {
    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ReferentialIntegrityException()
      : base(Strings.ExReferentialIntegrityViolation)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="message">Text of message.</param>
    public ReferentialIntegrityException(string message)
      : base(message)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ReferentialIntegrityException(Entity entity)
      : this(string.Format(
      Strings.ReferentialIntegrityViolationOnAttemptToRemoveXKeyY, entity.GetType().BaseType.GetFullName(), entity.Key))
    {
    }
  }
}