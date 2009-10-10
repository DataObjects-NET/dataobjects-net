// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.10.08

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Internals
{
  /// <summary>
  /// Saves a strong reference to an object.
  /// </summary>
  [Serializable]
  public class StrongReferenceContainer
  {
    private StrongReferenceContainer root;
    private readonly object reference;
    private StrongReferenceContainer nextJoinedContainer;
    private StrongReferenceContainer lastJoinedContainer;

    /// <summary>
    /// Joins this instance with <paramref name="other"/>, 
    /// if <paramref name="other"/> is not <see langword="null" /> and is the root of other containers' chain.
    /// </summary>
    /// <param name="other">The other container.</param>
    public void JoinIfPossible(StrongReferenceContainer other)
    {
      if (other != null && other.IsRoot)
        Join(other);
    }

    /// <summary>
    /// Joins this instance with <paramref name="other"/>.
    /// </summary>
    /// <param name="other">The other container.</param>
    public void Join(StrongReferenceContainer other)
    {
      if (other == null)
        return;
      if (!IsRoot) {
        root.Join(other);
        return;
      }
      if (!other.IsRoot)
        throw new InvalidOperationException();
      other.root = this;
      if (lastJoinedContainer == null)
        nextJoinedContainer = other;
      else if (lastJoinedContainer.nextJoinedContainer != null)
        throw new InvalidOperationException();
      else
        lastJoinedContainer.nextJoinedContainer = other;
      lastJoinedContainer = other.lastJoinedContainer ?? other;
    }

    private bool IsRoot { get { return root==null; } }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="reference">The reference to be saved.</param>
    public StrongReferenceContainer(object reference)
    {
      this.reference = reference;
    }
  }
}