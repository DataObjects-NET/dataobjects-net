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
using Xtensive.Collections;

using Xtensive.Orm.Operations;

namespace Xtensive.Orm
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

    private ReadOnlyDictionary<string, Key> identifiedEntities = EmptyIdentifiedEntities;
    private ReadOnlyList<IOperation> precedingOperations = EmptyOperations;
    private ReadOnlyList<IOperation> followingOperations = EmptyOperations;
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
    public ReadOnlyList<IOperation> PrecedingOperations {
      get { return precedingOperations; }
      internal set { precedingOperations = value; }
    }

    /// <inheritdoc/>
    public ReadOnlyList<IOperation> FollowingOperations {
      get { return followingOperations; }
      internal set { followingOperations = value; }
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
      foreach (var operation in PrecedingOperations)
        operation.Prepare(context);
      PrepareSelf(context);
      foreach (var operation in FollowingOperations)
        operation.Prepare(context);
    }

    /// <inheritdoc/>
    public void Execute(OperationExecutionContext context)
    {
      foreach (var operation in PrecedingOperations)
        operation.Execute(context);
      ExecuteSelf(context);
      foreach (var operation in FollowingOperations)
        operation.Execute(context);
    }

    /// <inheritdoc/>
    public IOperation Clone(bool withIdentifiedEntities)
    {
      var clone = CloneSelf(null);
      clone.Type = Type;
      if (PrecedingOperations.Count != 0) {
        var preconditions = (
          from o in PrecedingOperations
          where o is IPrecondition
          select o.Clone(false)
          ).ToList();
        if (preconditions.Count != 0)
          clone.PrecedingOperations = new ReadOnlyList<IOperation>(preconditions);
      }
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
        + (PrecedingOperations.Count==0 ? string.Empty : Environment.NewLine + FormatOperations("Preceding nested operation:", PrecedingOperations))
        + (FollowingOperations.Count==0 ? string.Empty : Environment.NewLine + FormatOperations("Following nested operations:", FollowingOperations))
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
    /// Initializes a new instance of this class.
    /// </summary>
    protected Operation()
    {
    }
  }
}