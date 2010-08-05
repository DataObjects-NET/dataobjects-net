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
    private static readonly ReadOnlyList<IOperation> EmptyOperations = 
      new ReadOnlyList<IOperation>(new List<IOperation>());
    private static readonly ReadOnlyList<IPrecondition> EmptyPreconditions = 
      new ReadOnlyList<IPrecondition>(new List<IPrecondition>());

    private ReadOnlyDictionary<string, Key> identifiedEntities = EmptyIdentifiedEntities;
    private ReadOnlyList<IPrecondition> preconditions = EmptyPreconditions;
    private ReadOnlyList<IOperation> nestedOperations = EmptyOperations;
    private ReadOnlyList<IOperation> undoOperations = EmptyOperations;

    /// <inheritdoc/>
    public abstract string Title { get; }

    /// <inheritdoc/>
    public virtual string Description { 
      get { return Title; }
    }

    /// <inheritdoc/>
    public OperationType Type { get; internal set; }

    /// <inheritdoc/>
    public IOperation OuterOperation { get; internal set; }

    /// <inheritdoc/>
    public IOperation OutermostOperation {
      get { return OuterOperation==null ? this : OuterOperation.OutermostOperation; }
    }

    /// <inheritdoc/>
    public bool IsNested {
      get { return OuterOperation!=null; }
    }

    /// <inheritdoc/>
    public bool IsOutermost {
      get { return OuterOperation==null; }
    }

    /// <inheritdoc/>
    public ReadOnlyList<IPrecondition> Preconditions {
      get { return preconditions; }
      internal set { preconditions = value; }
    }

    /// <inheritdoc/>
    public ReadOnlyList<IOperation> NestedOperations {
      get { return nestedOperations; }
      internal set { nestedOperations = value; }
    }

    /// <inheritdoc/>
    public ReadOnlyList<IOperation> UndoOperations {
      get { return undoOperations; }
      internal set { undoOperations = value; }
    }

    /// <inheritdoc/>
    public ReadOnlyDictionary<string, Key> IdentifiedEntities {
      get { return identifiedEntities; }
      set { identifiedEntities = value; }
    }

    /// <inheritdoc/>
    public void Prepare(OperationExecutionContext context)
    {
      foreach (var precondition in Preconditions)
        precondition.Prepare(context);
      PrepareSelf(context);
      foreach (var operation in NestedOperations)
        operation.Prepare(context);
    }

    /// <inheritdoc/>
    public void Execute(OperationExecutionContext context)
    {
      foreach (var precondition in Preconditions)
        precondition.Execute(context);
      ExecuteSelf(context);
      foreach (var operation in NestedOperations)
        operation.Execute(context);
    }

    /// <inheritdoc/>
    public IOperation Clone(bool withIdentifiedEntities)
    {
      var clone = CloneSelf(null);
      clone.Type = Type;
      if (Preconditions.Count!=0)
        clone.Preconditions = new ReadOnlyList<IPrecondition>(
          Preconditions.Select(p => (IPrecondition) p.Clone(false)).ToList());
      if (IdentifiedEntities.Count!=0 && withIdentifiedEntities)
        clone.IdentifiedEntities = IdentifiedEntities;
      return clone;
    }

    /// <summary>
    /// Prepares the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    protected abstract void PrepareSelf(OperationExecutionContext context);

    /// <summary>
    /// Executes the operation itself.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    protected abstract void ExecuteSelf(OperationExecutionContext context);

    /// <summary>
    /// Clones the operation itself.
    /// </summary>
    protected abstract Operation CloneSelf(Operation clone);

    /// <inheritdoc/>
    public override string ToString()
    {
      // Shouldn't be moved to resources
      return Description
        + (IdentifiedEntities.Count==0 ? string.Empty : Environment.NewLine + FormatIdentifiedEntities())
        + (Preconditions.Count==0 ? string.Empty : Environment.NewLine + FormatOperations("Preconditions:", Preconditions))
        + (NestedOperations.Count==0 ? string.Empty : Environment.NewLine + FormatOperations("Nested operations:", NestedOperations))
        + (UndoOperations.Count==0 ? string.Empty : Environment.NewLine + FormatOperations("Undo operations:", UndoOperations));
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

    [DebuggerStepThrough]
    private string FormatOperations(string title, IEnumerable<IOperation> operations)
    {
      // Shouldn't be moved to resources
      return "  {0}:\r\n".FormatWith(title) +
        operations.ToDelimitedString(Environment.NewLine).ToString().Indent(4);
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