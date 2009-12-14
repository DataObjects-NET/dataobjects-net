// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.12.14

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Fulltext
{
  /// <summary>
  /// Fulltext indexed entity container.
  /// </summary>
  /// <typeparam name="T">Type of the indexed entity.</typeparam>
  public sealed class Document<T> 
    where T : class, IEntity
  {
    /// <summary>
    /// Gets the rank of the fulltext document.
    /// </summary>
    public float Rank { get; private set; }

    /// <summary>
    /// Gets the key of the fulltext document and <see cref="Target"/> entity.
    /// </summary>
    public Key Key { get; private set; }

    /// <summary>
    /// Gets the target entity.
    /// </summary>
    public T Target { get; private set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="key">The <see cref="Key"/> property value.</param>
    /// <param name="rank">The <see cref="Rank"/> property value.</param>
    /// <param name="target">The <see cref="Target"/> property value.</param>
    public Document(float rank, Key key, T target)
    {
      Rank = rank;
      Key = key;
      Target = target;
    }
  }
}