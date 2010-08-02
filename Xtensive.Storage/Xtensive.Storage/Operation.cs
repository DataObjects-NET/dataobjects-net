// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Storage.Operations;

namespace Xtensive.Storage
{
  /// <summary>
  /// Base abstract class for all <see cref="IOperation"/> implementors.
  /// </summary>
  [DebuggerDisplay("Description = {Description}")]
  [Serializable]
  public abstract class Operation : IOperation
  {
    private static readonly ReadOnlyDictionary<string, Key> EmptyIdentifiedEntities = 
      new ReadOnlyDictionary<string, Key>(new Dictionary<string, Key>());

    private ReadOnlyDictionary<string, Key> identifiedEntities = EmptyIdentifiedEntities;

    /// <inheritdoc/>
    public abstract string Title { get; }

    /// <inheritdoc/>
    public virtual string Description { 
      get { return Title; }
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, Key> IdentifiedEntities {
      get { return identifiedEntities; }
      set {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        if (identifiedEntities!=EmptyIdentifiedEntities)
          Exceptions.AlreadyInitialized("identifiedEntities");
        identifiedEntities = value;
      }
    }

    /// <inheritdoc/>
    public abstract void Prepare(OperationExecutionContext context);

    /// <inheritdoc/>
    public abstract void Execute(OperationExecutionContext context);

    /// <inheritdoc/>
    public override string ToString()
    {
      return Description + (IdentifiedEntities.Count==0 ? string.Empty : Environment.NewLine + FormatIdentifiedEntities());
    }

    [DebuggerStepThrough]
    private string FormatIdentifiedEntities()
    {
      // Shouldn't be moved to resources
      return "  Identified entities:\r\n" + (
        from pair in IdentifiedEntities
        orderby pair.Key
        select "    {0}: {1}".FormatWith(pair.Key, pair.Value)
        ).ToDelimitedString(Environment.NewLine);
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