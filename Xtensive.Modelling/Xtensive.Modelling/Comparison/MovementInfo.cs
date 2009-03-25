// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="Node"/> movement information.
  /// </summary>
  [Serializable]
  public class MovementInfo
  {
    /// <summary>
    /// Gets a value indicating whether <see cref="Node.Name"/> is changed.
    /// Always <see langword="false" /> for <see cref="IUnnamedNode"/>s.
    /// </summary>
    public bool IsNameChanged { get; set; }

    /// <summary>
    /// Gets a value indicating whether <see cref="Node.Name"/> is changed.
    /// Always <see langword="false" /> for nodes nested into
    /// <see cref="IUnorderedNodeCollection"/>.
    /// </summary>
    public bool IsIndexChanged { get; set; }

    /// <summary>
    /// Gets a value indicating whether direct <see cref="Node.Parent"/> is changed,
    /// i.e. the node was moved to a different parent node.
    /// Parent's renaming isn't considered as parent change.
    /// </summary>
    public bool IsParentChanged { get; set; }

    /// <summary>
    /// Gets a value indicating whether direct or inherited <see cref="Node.Parent"/> is changed,
    /// i.e. the node or one of its parent was moved to a different parent node.
    /// </summary>
    public bool IsAnyParentChanged { get; set; }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MovementInfo()
    {
    }
  }
}