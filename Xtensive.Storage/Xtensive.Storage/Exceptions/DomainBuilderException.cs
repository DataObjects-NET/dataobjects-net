// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.09.18

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Describes various errors detected during <see cref="Domain"/>.<see cref="Domain.Build"/> execution.
  /// </summary>
  [Serializable]
  public class DomainBuilderException: Exception
  {
    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public DomainBuilderException(string message)
      : base(message)
    {
    }
  }
}