// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.01

using System;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  /// <summary>
  /// Thrown on attempt to remove an object having
  /// reference with <see cref="ReferentialAction.Restrict"/>
  /// option pointing to it.
  /// </summary>
  [Serializable]
  public class ReferentialIntegrityException: Exception
  {
    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ReferentialIntegrityException()
      : base("Referential integrity violation.") {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="text">Text of message.</param>
    public ReferentialIntegrityException(string text)
      : base(text) {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public ReferentialIntegrityException(Entity entity)
      : this(String.Format("Referential integrity violation on attempt to remove {0}, Key={1}.", entity.GetType().BaseType.GetFullName(), entity.Key)) {}
  }
}