// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.03.25

using System;
using System.Text;
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
    /// Gets or sets a value indicating whether the target node is created.
    /// </summary>
    public bool IsCreated { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the source node is removed.
    /// </summary>
    public bool IsRemoved { get; set; }

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

    /// <summary>
    /// Gets a value indicating whether node wasn't moved, created or deleted.
    /// </summary>
    public bool IsUnchanged { 
      get {
        return !(IsCreated || IsRemoved || IsIndexChanged || IsNameChanged || IsParentChanged);
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      var sb = new StringBuilder();
      if (IsCreated)
        sb.Append(", created");
      if (IsRemoved)
        sb.Append(", removed");
      if (IsNameChanged)
        sb.Append(", renamed");
      if (IsIndexChanged)
        sb.Append(", moved");
      if (IsParentChanged)
        sb.Append(", parent changed");
      
      if (sb.Length==0)
        return string.Empty;
      else
        return sb.ToString(2, sb.Length - 2);
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    public MovementInfo()
    {
    }
  }
}