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


    /// <summary>
    /// Gets the title of the operation.
    /// </summary>
    public abstract string Title { get; }


    /// <summary>
    /// Gets the description of the operation.
    /// </summary>
    public virtual string Description { 
      get { return Title; }
    }


    /// <summary>
    /// Gets the type of the operation.
    /// </summary>
    public OperationType Type { get; internal set; }


    /// <summary>
    /// Gets the list of preconditions.
    /// </summary>
    public ReadOnlyList<IOperation> PrecedingOperations {
      get { return precedingOperations; }
      internal set { precedingOperations = value; }
    }


    /// <summary>
    /// Gets the list of nested operations.
    /// </summary>
    public ReadOnlyList<IOperation> FollowingOperations {
      get { return followingOperations; }
      internal set { followingOperations = value; }
    }


    /// <summary>
    /// Gets the list of undo operations.
    /// </summary>
    public ReadOnlyList<IOperation> UndoOperations {
      get { return undoOperations; }
      internal set { undoOperations = value; }
    }


    /// <summary>
    /// Gets or sets the identified entities.
    /// Value of this property can be assigned just once.
    /// </summary>
    public ReadOnlyDictionary<string, Key> IdentifiedEntities {
      get { return identifiedEntities; }
      set { identifiedEntities = value; }
    }


    /// <summary>
    /// Prepares the operation using specified execution context.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    public void Prepare(OperationExecutionContext context)
    {
      foreach (var operation in PrecedingOperations)
        operation.Prepare(context);
      PrepareSelf(context);
      foreach (var operation in FollowingOperations)
        operation.Prepare(context);
    }


    /// <summary>
    /// Executes the operation using specified execution context.
    /// </summary>
    /// <param name="context">The operation execution context.</param>
    public void Execute(OperationExecutionContext context)
    {
      foreach (var operation in PrecedingOperations)
        operation.Execute(context);
      ExecuteSelf(context);
      foreach (var operation in FollowingOperations)
        operation.Execute(context);
    }


    /// <summary>
    /// Clones the operation, <see cref="PrecedingOperations"/>,
    /// <see cref="FollowingOperations"/> and <see cref="UndoOperations"/>.
    /// </summary>
    /// <param name="withIdentifiedEntities">if set to <see langword="true"/>
    /// 	<see cref="IdentifiedEntities"/>
    /// must be cloned as well.</param>
    /// <returns>
    /// Clone of the current operation.
    /// </returns>
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


    /// <summary>
    /// Returns a <see cref="System.String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="System.String"/> that represents this instance.
    /// </returns>
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