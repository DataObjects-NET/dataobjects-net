// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Text;
using Xtensive.Internals.DocTemplates;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="Node"/> movement information.
  /// </summary>
  [Flags]
  public enum MovementInfo
  {
    /// <summary>
    /// The source node is not changed.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The source node is changed.
    /// </summary>
    Changed = 
      Created | 
      Removed | 
      Copied | 
      NameChanged | 
      IndexChanged | 
      ParentRelocated |
      ParentChanged,

    /// <summary>
    /// The source node is relocated.
    /// Implies any action leading to update references to it.
    /// </summary>
    Relocated = 
      Copied | 
      IndexChanged | 
      ParentChanged | 
      ParentRelocated,

    /// <summary>
    /// The target node is newly created.
    /// </summary>
    Created = 1,

    /// <summary>
    /// The source node is removed.
    /// </summary>
    Removed = 2,

    /// <summary>
    /// The source node is copied.
    /// </summary>
    Copied = 4,

    /// <summary>
    /// Source node <see cref="Node.Name"/> is changed.
    /// Always <see langword="false" /> for <see cref="IUnnamedNode"/>s.
    /// </summary>
    NameChanged = 8,

    /// <summary>
    /// Source node <see cref="Node.Index"/> is changed.
    /// Always <see langword="false" /> for nodes nested into
    /// <see cref="IUnorderedNodeCollection"/>.
    /// </summary>
    IndexChanged = 16,

    /// <summary>
    /// Direct source node <see cref="Node.Parent"/> is changed,
    /// i.e. the node was moved to a different parent node.
    /// Parent's renaming isn't considered as parent change.
    /// </summary>
    ParentChanged = 32,

    /// <summary>
    /// Direct or indirect source node <see cref="Node.Parent"/> is <see cref="Relocated"/>.
    /// </summary>
    ParentRelocated = 64,
  }
}