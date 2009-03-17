// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using System.Diagnostics;
using Xtensive.Core.Helpers;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Indexing.Storage.Model
{
  /// <summary>
  /// Describes a single foreign key.
  /// </summary>
  [Serializable]
  public class ForeignKeyInfo: Node<ForeignKeyInfo, PrimaryIndexInfo>
  {
    private string referencedIndexName;
    private ReferentialAction onRemove;
    private ReferentialAction onUpdate;

    /// <summary>
    /// Gets or sets the name of the referenced index.
    /// </summary>
    /// <value>The name of the referenced index.</value>
    public string ReferencedIndexName
    {
      [DebuggerStepThrough]
      get{return referencedIndexName;}
      [DebuggerStepThrough]
      set
      {
        this.EnsureNotLocked();
        referencedIndexName = value;
      }
    }

    /// <summary>
    /// Gets or sets the action on remove.
    /// </summary>
    /// <value>The on remove.</value>
    public ReferentialAction OnRemove
    {
      get { return onRemove; }
      set
      {
        this.EnsureNotLocked();
        onRemove = value;
      }
    }

    /// <summary>
    /// Gets or sets the action on update .
    /// </summary>
    /// <value>The on update.</value>
    public ReferentialAction OnUpdate
    {
      get { return onUpdate; }
      set
      {
        this.EnsureNotLocked();
        onUpdate = value;
      }
    }

    /// <inheritdoc/>
    protected override NodeCollection<ForeignKeyInfo, PrimaryIndexInfo> GetParentNodeCollection()
    {
      return Parent==null ? null : Parent.ForeignKeys;
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="primaryIndex">Index of the primary.</param>
    /// <param name="name">The name.</param>
    public ForeignKeyInfo(PrimaryIndexInfo primaryIndex, string name)
      :base(primaryIndex, name)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="primaryIndex">Index of the primary.</param>
    /// <param name="name">The name.</param>
    /// <param name="referencedIndexName">Name of the referenced index.</param>
    public ForeignKeyInfo(PrimaryIndexInfo primaryIndex, string name, string referencedIndexName)
      : base(primaryIndex, name)
    {
      this.referencedIndexName = referencedIndexName;
    }
  }
}