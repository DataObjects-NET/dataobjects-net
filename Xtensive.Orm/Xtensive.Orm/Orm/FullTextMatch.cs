// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm
{
  /// <summary>
  /// Single full-text search match result.
  /// </summary>
  /// <typeparam name="T">Type of the matched entity.</typeparam>
  public sealed class FullTextMatch<T> 
    where T : class, IEntity
  {
    /// <summary>
    /// Gets the rank of the full-text document.
    /// </summary>
    public double Rank { get; private set; }

    /// <summary>
    /// Gets the target entity.
    /// </summary>
    public T Entity { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="rank">The <see cref="Rank"/> property value.</param>
    /// <param name="target">The <see cref="Entity"/> property value.</param>
    internal FullTextMatch(double rank, T target)
    {
      Rank = rank;
      Entity = target;
    }
  }
}