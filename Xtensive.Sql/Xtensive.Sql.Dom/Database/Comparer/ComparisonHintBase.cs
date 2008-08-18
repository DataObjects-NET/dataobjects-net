// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.18

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for comparison hints.
  /// </summary>
  public abstract class ComparisonHintBase
  {
    private readonly Type type;
    private readonly string name;

    /// <summary>
    /// Type of hint target.
    /// </summary>
    public Type Type
    {
      get { return type; }
    }

    /// <summary>
    /// Name of hint target.
    /// </summary>
    public string Name
    {
      get { return name; }
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected ComparisonHintBase(Type type, string name)
    {
      this.type = type;
      this.name = name;
    }
  }
}