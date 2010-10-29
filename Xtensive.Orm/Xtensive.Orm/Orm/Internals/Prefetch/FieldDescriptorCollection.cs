// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.19

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Orm.Internals.Prefetch
{
  /// <summary>
  /// Collection of <see cref="PrefetchFieldDescriptor"/>.
  /// </summary>
  [Serializable]
  public sealed class FieldDescriptorCollection : IEnumerable<PrefetchFieldDescriptor>
  {
    private readonly List<PrefetchFieldDescriptor> descriptors;

    /// <summary>
    /// Gets the count of elements.
    /// </summary>
    public int Count { get { return descriptors.Count; } }

    /// <summary>
    /// Gets the <see cref="PrefetchFieldDescriptor"/> with the specified index.
    /// </summary>
    public PrefetchFieldDescriptor this[int i] {get { return descriptors[i]; } }

    /// <inheritdoc/>
    public IEnumerator<PrefetchFieldDescriptor> GetEnumerator()
    {
      return descriptors.GetEnumerator();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptors">The descriptors.</param>
    public FieldDescriptorCollection(IEnumerable<PrefetchFieldDescriptor> descriptors)
    {
      ArgumentValidator.EnsureArgumentNotNull(descriptors, "descriptors");
      var asCollection = descriptors as ICollection<PrefetchFieldDescriptor>;
      if (asCollection != null)
        this.descriptors = new List<PrefetchFieldDescriptor>(asCollection.Count);
      else {
        var asArray = descriptors as PrefetchFieldDescriptor[];
        this.descriptors = asArray != null
          ? new List<PrefetchFieldDescriptor>(asArray.Length)
          : new List<PrefetchFieldDescriptor>();
      }
      this.descriptors.AddRange(descriptors);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="descriptors">The descriptors.</param>
    public FieldDescriptorCollection(params PrefetchFieldDescriptor[] descriptors)
      : this((IEnumerable<PrefetchFieldDescriptor>) descriptors)
    {}
  }
}