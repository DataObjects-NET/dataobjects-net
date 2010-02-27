// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Diagnostics;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Base abstract class for all <see cref="IOperation"/> implementors.
  /// </summary>
  [DebuggerDisplay("Description = {Description}")]
  [Serializable]
  public abstract class Operation : IOperation
  {
    /// <inheritdoc/>
    public abstract string Title { get; }

    /// <inheritdoc/>
    public virtual string Description { 
      get { return Title; }
    }

    /// <inheritdoc/>
    public abstract void Prepare(OperationExecutionContext context);

    /// <inheritdoc/>
    public abstract void Execute(OperationExecutionContext context);

    /// <inheritdoc/>
    public override string ToString()
    {
      return Description;
    }

    
    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Operation()
    {
    }
  }
}