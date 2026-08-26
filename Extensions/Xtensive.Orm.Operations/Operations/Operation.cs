// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.11.19

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Xtensive.Core;
using Xtensive.Orm.Operations.Interfaces;
using Xtensive.Orm.Operations.Serialization.Json;

namespace Xtensive.Orm.Operations
{
  /// <summary>
  /// Base abstract class for all <see cref="IOperation"/> implementors.
  /// </summary>
  [DebuggerDisplay("Description = {Description}")]
  [Serializable]
  [JsonDerivedType(typeof(EntitiesRemoveOperation),      nameof(EntitiesRemoveOperation))]
  [JsonDerivedType(typeof(EntityCreateOperation),        nameof(EntityCreateOperation))]
  [JsonDerivedType(typeof(EntityFieldSetOperation),      nameof(EntityFieldSetOperation))]
  [JsonDerivedType(typeof(EntityInitializeOperation),    nameof(EntityInitializeOperation))]
  [JsonDerivedType(typeof(EntitySetClearOperation),      nameof(EntitySetClearOperation))]
  [JsonDerivedType(typeof(EntitySetItemAddOperation),    nameof(EntitySetItemAddOperation))]
  [JsonDerivedType(typeof(EntitySetItemRemoveOperation), nameof(EntitySetItemRemoveOperation))]
  [JsonDerivedType(typeof(KeyGenerateOperation),         nameof(KeyGenerateOperation))]
  [JsonDerivedType(typeof(MethodCallOperation),          nameof(MethodCallOperation))]
  [JsonDerivedType(typeof(ValidateVersionOperation),     nameof(ValidateVersionOperation))]
  public abstract class Operation : IOperation, IExecutableOperation
  {
    private static readonly IReadOnlyDictionary<string, Key> EmptyIdentifiedEntities = new Dictionary<string, Key>().AsReadOnly();

    [JsonIgnore]
    private IReadOnlyDictionary<string, Key> identifiedEntities = EmptyIdentifiedEntities;
    [JsonIgnore]
    private IReadOnlyList<IOperation> precedingOperations = Array.Empty<IOperation>();
    [JsonIgnore]
    private IReadOnlyList<IOperation> followingOperations = Array.Empty<IOperation>();
    [JsonIgnore]
    private IReadOnlyList<IOperation> undoOperations = Array.Empty<IOperation>();

    /// <inheritdoc/>
    [JsonIgnore]
    public abstract string Title { get; }

    /// <inheritdoc/>
    [JsonIgnore]
    public virtual string Description {
      get { return Title; }
    }

    /// <inheritdoc/>
    [JsonInclude]
    public OperationType Type { get; internal set; }

    /// <inheritdoc/>
    [JsonInclude]
    [JsonConverter(typeof(CollectionOfOperationsConverter))]
    public IReadOnlyList<IOperation> PrecedingOperations {
      get { return precedingOperations; }
      internal set { precedingOperations = (value.Count == 0) ? Array.Empty<IOperation>() : value; }
    }

    /// <inheritdoc/>
    [JsonInclude]
    [JsonConverter(typeof(CollectionOfOperationsConverter))]
    public IReadOnlyList<IOperation> FollowingOperations {
      get { return followingOperations; }
      internal set { followingOperations = (value.Count == 0) ? Array.Empty<IOperation>(): value ; }
    }

    /// <inheritdoc/>
    [JsonInclude]
    [JsonConverter(typeof(CollectionOfOperationsConverter))]
    public IReadOnlyList<IOperation> UndoOperations {
      get { return undoOperations; }
      internal set { undoOperations = (value.Count == 0) ? Array.Empty<IOperation>() : value; }
    }

    /// <inheritdoc/>
    [JsonInclude]
    [JsonConverter(typeof(CollectionOfIdentifiedEntitiesConverter))]
    public IReadOnlyDictionary<string, Key> IdentifiedEntities {
      get { return identifiedEntities; }
      set { identifiedEntities = value.Count==0 ? EmptyIdentifiedEntities : value; }
    }

    /// <inheritdoc/>
    public void Prepare(OperationExecutionContext context)
    {
      foreach (var operation in PrecedingOperations.Cast<IExecutableOperation>()) {
        if (operation is not null)
          operation.Prepare(context);
      }
      PrepareSelf(context);
      foreach (var operation in FollowingOperations.Cast<IExecutableOperation>()) {
        if (operation is not null)
          operation.Prepare(context);
      }
    }

    /// <inheritdoc/>
    public void Execute(OperationExecutionContext context)
    {
      foreach (var operation in PrecedingOperations.Cast<IExecutableOperation>()) {
        if (operation is not null)
          operation.Execute(context);
      }
      ExecuteSelf(context);
      foreach (var operation in FollowingOperations.Cast<IExecutableOperation>()) {
        if (operation is not null)
          operation.Execute(context);
      }
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
          clone.PrecedingOperations = preconditions.AsReadOnly();
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
      return "  Identified entities:" + Environment.NewLine + (
        from pair in IdentifiedEntities
        orderby pair.Key
        select $"    {pair.Key}: {pair.Value}"
        ).ToDelimitedString(Environment.NewLine);
    }

    [DebuggerStepThrough]
    private string FormatOperations(string title, IEnumerable<IOperation> operations)
    {
      // Shouldn't be moved to resources
      return $"  {title}:" + Environment.NewLine +
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