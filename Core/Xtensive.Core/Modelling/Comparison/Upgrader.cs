// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.07

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Disposing;
using Xtensive.Serialization.Binary;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Resources;
using Xtensive.Reflection;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// <see cref="IUpgrader"/> implementation.
  /// </summary>
  public class Upgrader : IUpgrader
  {
    /// <summary>
    /// Node group comment (in action sequence).
    /// </summary>
    public readonly static string NodeGroupComment           = "{0}";
    /// <summary>
    /// Node collection group comment (in action sequence).
    /// </summary>
    public readonly static string NodeCollectionGroupComment = "{0}[]";
    /// <summary>
    /// Preconditions group comment (in action sequence).
    /// </summary>
    public readonly static string PreConditionsGroupComment  = "<PreConditions>";
    /// <summary>
    /// Renames group comment (in action sequence).
    /// </summary>
    public readonly static string RenamesGroupComment        = "<Renames>";
    /// <summary>
    /// Postconditions group comment (in action sequence).
    /// </summary>
    public readonly static string PostConditionsGroupComment = "<PostConditions>";
    /// <summary>
    /// Temporary name format string.
    /// </summary>
    public readonly static string TemporaryNameFormat        = "Temp_{0}";

    [ThreadStatic]
    private static Upgrader current;

    #region Properties: Current, Context, Difference, Hints, Stage, XxxModel, ...

    /// <summary>
    /// Gets the current comparer.
    /// </summary>
    public static Upgrader Current {
      get { return current; }
    }

    /// <summary>
    /// Gets the current upgrade context.
    /// </summary>
    protected internal UpgradeContext Context { get; internal set; }

    /// <summary>
    /// Gets the difference that is currently processed.
    /// </summary>
    protected Difference Difference { get; private set; }

    /// <summary>
    /// Gets the comparison and upgrade hints.
    /// </summary>
    protected HintSet Hints { get; private set; }

    /// <summary>
    /// Gets the current upgrade stage.
    /// </summary>
    protected internal UpgradeStage Stage { get; private set; }

    /// <summary>
    /// Gets the temporary renames.
    /// </summary>
    protected Dictionary<string, Node> TemporaryRenames { get; private set; }

    /// <summary>
    /// Gets the source model to compare.
    /// </summary>
    protected IModel SourceModel { get; private set; }

    /// <summary>
    /// Gets the target model to compare.
    /// </summary>
    protected IModel TargetModel { get; private set; }

    /// <summary>
    /// Gets the current model.
    /// </summary>
    protected IModel CurrentModel { get; private set; }

    /// <summary>
    /// Gets the comparer.
    /// </summary>
    protected IComparer Comparer { get; private set; }

    #endregion

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>hints.SourceModel</c> or <c>hints.TargetModel</c>
    /// is out of range.</exception>
    public ReadOnlyList<NodeAction> GetUpgradeSequence(Difference difference, HintSet hints)
    {
      return GetUpgradeSequence(difference, hints, new Comparer());
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentOutOfRangeException"><c>hints.SourceModel</c> or <c>hints.TargetModel</c>
    /// is out of range.</exception>
    /// <exception cref="InvalidOperationException">Upgrade sequence validation has failed.</exception>
    public ReadOnlyList<NodeAction> GetUpgradeSequence(Difference difference, HintSet hints, IComparer comparer)
    {
      ArgumentValidator.EnsureArgumentNotNull(hints, "hints");
      ArgumentValidator.EnsureArgumentNotNull(comparer, "comparer");
      if (difference == null)
        return new ReadOnlyList<NodeAction>(Enumerable.Empty<NodeAction>().ToList());

      TemporaryRenames = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
      SourceModel = (IModel) difference.Source;
      TargetModel = (IModel) difference.Target;
      Hints = hints ?? new HintSet(SourceModel, TargetModel);
      Comparer = comparer;
      if (Hints.SourceModel!=SourceModel)
        throw new ArgumentOutOfRangeException("hints.SourceModel");
      if (Hints.TargetModel!=TargetModel)
        throw new ArgumentOutOfRangeException("hints.TargetModel");
      CurrentModel = Cloner.Clone(SourceModel);
      Difference = difference;
      var previous = current;
      current = this;
      using (NullActionHandler.Instance.Activate()) {
        try {
          var actions = new GroupingNodeAction();

          ProcessStage(UpgradeStage.CleanupData, actions);
          ProcessStage(UpgradeStage.Prepare, actions);
          ProcessStage(UpgradeStage.TemporaryRename, actions);
          ProcessStage(UpgradeStage.Upgrade, actions);
          ProcessStage(UpgradeStage.CopyData, actions);
          ProcessStage(UpgradeStage.PostCopyData, actions);
          ProcessStage(UpgradeStage.Cleanup, actions);

          var validationHints = new HintSet(CurrentModel, TargetModel);
          Hints.OfType<IgnoreHint>().ForEach(validationHints.Add);
          var diff = comparer.Compare(CurrentModel, TargetModel, validationHints);
          if (diff!=null) {
            Log.InfoRegion(Strings.LogAutomaticUpgradeSequenceValidation);
            Log.Info(Strings.LogValidationFailed);
            Log.Info(Strings.LogItemFormat, Strings.Difference);
            Log.Info("{0}", diff);
            Log.Info(Strings.LogItemFormat+"\r\n{1}", Strings.UpgradeSequence, 
              new ActionSequence() {actions});
            Log.Info(Strings.LogItemFormat, Strings.ExpectedTargetModel);
            TargetModel.Dump();
            Log.Info(Strings.LogItemFormat, Strings.ActualTargetModel);
            CurrentModel.Dump();
            throw new InvalidOperationException(Strings.ExUpgradeSequenceValidationFailure);
          }
          return new ReadOnlyList<NodeAction>(actions.Actions, true);
        }
        finally {
          current = previous;
        }
      }
    }

    /// <summary>
    /// Generate actions for specific <see cref="UpgradeStage"/>.
    /// </summary>
    /// <param name="stage">The stage.</param>
    /// <param name="action">The parent action.</param>
    protected void ProcessStage(UpgradeStage stage, GroupingNodeAction action)
    {
      Stage = stage;
      if (stage==UpgradeStage.Upgrade) {
        UpdateHints();
        Difference = Comparer.Compare(CurrentModel, TargetModel, Hints);
      }
      var stageActions = Visit(Difference);
      if (stageActions!=null)
        action.Add(stageActions);
      Log.Info(string.Format("Stage {0} complete.", stage));
    }

    /// <summary>
    /// Visitor dispatcher.
    /// </summary>
    /// <param name="difference">The difference to visit.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected GroupingNodeAction Visit(Difference difference)
    {
      if (difference==null)
        return null;
      UpgradeContext ctx = null;
      using (CreateContext().Activate()) {
        ctx = Context;
        Context.Difference = difference;
        if (difference is NodeDifference)
          VisitNodeDifference((NodeDifference) difference);
        else if (difference is NodeCollectionDifference)
          VisitNodeCollectionDifference((NodeCollectionDifference) difference);
        else if (difference is ValueDifference)
          VisitValueDifference((ValueDifference) difference);
        else
          VisitUnknownDifference(difference);
      }
      return ctx.Actions;
    }

    /// <summary>
    /// Visits the node difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected virtual void VisitNodeDifference(NodeDifference difference)
    {
      var any = difference.Target ?? difference.Source;
      var isInverseOrderUpgrade =
        Stage==UpgradeStage.Prepare
        || Stage==UpgradeStage.Cleanup
        || Stage==UpgradeStage.TemporaryRename;

      using (OpenActionGroup(string.Format(NodeGroupComment, any.Name))) {
        if (isInverseOrderUpgrade) {
          ProcessProperties(difference);
          if (IsAllowedForCurrentStage(difference))
            ProcessMovement(difference);
        }
        else {
          if (IsAllowedForCurrentStage(difference))
            ProcessMovement(difference);
          ProcessProperties(difference);
        }
      }
    }

    /// <summary>
    /// Generates <see cref="NodeAction"/> according to <see cref="NodeDifference.MovementInfo"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    protected virtual void ProcessMovement(NodeDifference difference)
    {
      var source = difference.Source;
      var target = difference.Target;
      var isImmutable = Context.IsImmutable;
      var isChanged = (difference.MovementInfo & MovementInfo.Changed)!=0;
      var isCreated = (difference.MovementInfo & MovementInfo.Created)!=0;
      var isRemoved = (difference.MovementInfo & MovementInfo.Removed)!=0
        || (isImmutable && difference.HasChanges && !isCreated);
      
      var sc = StringComparer.OrdinalIgnoreCase;

      switch (Stage) {
      case UpgradeStage.CleanupData:
        if (difference.IsDataChanged) {
          Hints.GetHints<UpdateDataHint>(difference.Source)
            .Where(hint => sc.Compare(hint.SourceTablePath, difference.Source.Path) == 0)
            .ForEach(hint => AddAction(UpgradeActionType.Regular, 
              new DataAction {DataHint = hint}));
          Hints.GetHints<DeleteDataHint>(difference.Source)
            .Where(hint => !hint.PostCopy)
            .Where(hint => sc.Compare(hint.SourceTablePath, difference.Source.Path) == 0)
            .ForEach(hint => AddAction(UpgradeActionType.Regular, 
              new DataAction {DataHint = hint}));
        }
        break;
      case UpgradeStage.Prepare:
        if (isRemoved && !difference.IsRemoveOnCleanup) {
          if (!Context.IsRemoved || difference.IsDependentOnParent) {
            AddAction(UpgradeActionType.PreCondition,
              new RemoveNodeAction { Path = (source ?? target).Path });
          }
        }
        break;
      case UpgradeStage.Cleanup:
        if (!Context.IsRemoved || difference.IsDependentOnParent) {
          AddAction(UpgradeActionType.PreCondition,
            new RemoveNodeAction() {
              Path = (source ?? target).Path
            });
        }
        break;
      case UpgradeStage.TemporaryRename:
        RegisterTemporaryRename(source);
        var temporaryName = GetTemporaryName(source);
        AddAction(UpgradeActionType.Rename,
          new MoveNodeAction {
            Path = source.Path,
            Parent = source.Parent==null ? string.Empty : source.Parent.Path,
            Name = temporaryName,
            Index = null,
            NewPath = GetPathWithoutName(source) + temporaryName
          });
        break;
      case UpgradeStage.CopyData:
        Hints.GetHints<CopyDataHint>(difference.Source).Where(hint => sc.Compare(hint.SourceTablePath, difference.Source.Path) == 0)
          .ForEach(hint => AddAction(UpgradeActionType.Regular, new DataAction {DataHint = hint}));
        break;
      case UpgradeStage.PostCopyData:
        if (difference.IsDataChanged) {
          Hints.GetHints<DeleteDataHint>(difference.Source)
            .Where(hint => hint.PostCopy)
            .Where(hint => sc.Compare(hint.SourceTablePath, difference.Source.Path) == 0)
            .ForEach(hint => AddAction(UpgradeActionType.Regular,
              new DataAction {DataHint = hint}));
        }
        break;
      case UpgradeStage.Upgrade:
        if (target==null)
          break;
        if (isCreated) {
          var action = new CreateNodeAction {
            Path = target.Parent==null ? string.Empty : target.Parent.Path,
            Type = target.GetType(),
            Name = target.Name,
            Index = target.Nesting.IsNestedToCollection ? (int?) target.Index : null
          };
          AddAction(UpgradeActionType.PostCondition, action);
        }
        else if (isChanged) {
          var action = new MoveNodeAction {
            Path = source.Path,
            Parent = target.Parent==null ? string.Empty : target.Parent.Path,
            Name = target.Name,
            Index = target.Nesting.IsNestedToCollection ? (int?) source.Index : null,
            NewPath = target.Path
          };
          AddAction(UpgradeActionType.PostCondition, action);
        }
        break;
      }
    }

    /// <summary>
    /// Registers the temporary rename.
    /// </summary>
    /// <param name="source">The renamed node.</param>
    protected void RegisterTemporaryRename(Node source)
    {
      TemporaryRenames.Add(source.Path, CurrentModel.Resolve(source.Path, true) as Node);

      var children = source.PropertyAccessors.Values
        .Where(pa => !pa.IsImmutable).Select(pa=>pa.Getter.Invoke(source));
      foreach (var child in children) {
        var childNode = child as Node;
        if (childNode != null) {
          if (!TemporaryRenames.ContainsKey(childNode.Path)) {
            var currentModelChildNode = CurrentModel.Resolve(childNode.Path) as Node;
            if (currentModelChildNode!=null)
              TemporaryRenames.Add(childNode.Path, currentModelChildNode);
          }
          continue;
        }
        var childNodeCollection = child as NodeCollection;
        if (childNodeCollection != null)
          foreach (Node node in childNodeCollection)
            if (!TemporaryRenames.ContainsKey(node.Path)) {
              var currentModelChildNode = CurrentModel.Resolve(node.Path) as Node;
              if (currentModelChildNode!=null)
                TemporaryRenames.Add(node.Path, currentModelChildNode);
            }
      }
    }

    /// <summary>
    /// Process <see cref="NodeDifference.PropertyChanges"/> for specific <see cref="NodeDifference"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    protected virtual void ProcessProperties(NodeDifference difference)
    {
      var isImmutable = Context.IsImmutable;
      var isCleanup = Stage==UpgradeStage.Prepare || Stage==UpgradeStage.Cleanup;
      var any = difference.Target ?? difference.Source;

      IEnumerable<KeyValuePair<string, Difference>> propertyChanges = difference.PropertyChanges;
      if (isCleanup)
        propertyChanges = propertyChanges.Reverse();
      foreach (var pair in propertyChanges) {
        var accessor = any.PropertyAccessors[pair.Key];
        if (!isCleanup || !isImmutable || IsVolatile(difference, accessor))
          using (CreateContext().Activate()) {
            Context.Property = pair.Key;
            Context.IsImmutable = IsImmutable(difference, accessor);
            if ((difference.MovementInfo & MovementInfo.Removed) != 0)
              Context.IsRemoved = true;
            var dependencyRootType = GetDependencyRootType(difference, accessor);
            if (dependencyRootType!=null)
              Context.DependencyRootType = dependencyRootType;
            Visit(pair.Value);
          }
      }
    }

    /// <summary>
    /// Visits the node collection difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected virtual void VisitNodeCollectionDifference(NodeCollectionDifference difference)
    {
      using (OpenActionGroup(string.Format(NodeCollectionGroupComment, Context.Property))) {
        var itemChanges = difference.ItemChanges;
        if (Stage == UpgradeStage.Upgrade)
          itemChanges = itemChanges.Where(nodeDifference =>
            (nodeDifference.MovementInfo & MovementInfo.NameChanged)!=0)
            .Concat(itemChanges.Where(nodeDifference =>
              (nodeDifference.MovementInfo & MovementInfo.NameChanged)==0)).ToList();
        foreach (var newDifference in itemChanges)
          Visit(newDifference);
      }
    }

    /// <summary>
    /// Visits the value difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    protected virtual void VisitValueDifference(ValueDifference difference)
    {
      if (Stage!=UpgradeStage.Upgrade)
        return;

      var parentContext = Context.Parent;
      var targetNode = ((NodeDifference) parentContext.Difference).Target;
      if (targetNode==null)
        return;
      var action = new PropertyChangeAction() {
        Path = targetNode.Path
      };
      action.Properties.Add(parentContext.Property, PathNodeReference.Get(difference.Target));
      AddAction(UpgradeActionType.PostCondition, action);
    }

    /// <summary>
    /// Visits the unknown difference.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// A sequence of actions that must be performed to upgrade
    /// from <see cref="IDifference.Source"/> of the specified
    /// difference to its <see cref="IDifference.Target"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">Always thrown by this method.</exception>
    protected virtual void VisitUnknownDifference(Difference difference)
    {
      throw new NotSupportedException();
    }

    /// <summary>
    /// Determines whether the specified difference allowed for current <see cref="Stage"/>.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// <see langword="true"/> if the specified difference allowed
    /// for current <see cref="Stage"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><c>Stage</c> is out of range.</exception>
    protected virtual bool IsAllowedForCurrentStage(NodeDifference difference)
    {
      var isRemoved = (difference.MovementInfo & MovementInfo.Removed)!=0;
      var isCreated = (difference.MovementInfo & MovementInfo.Created)!=0;
      var isNameChanged = (difference.MovementInfo & MovementInfo.NameChanged)!=0;
      var isImmutable = Context.IsImmutable;
      var isRemoveOnPrepare =
        isRemoved && !difference.IsRemoveOnCleanup || (isImmutable && !isCreated);

      switch (Stage) {
      case UpgradeStage.CleanupData:
          return difference.IsDataChanged;
      case UpgradeStage.Prepare:
          return isRemoveOnPrepare;
      case UpgradeStage.TemporaryRename:
        return isNameChanged && IsCyclicRename(difference);
      case UpgradeStage.Upgrade:
        return !isRemoveOnPrepare && (isCreated || isNameChanged);
      case UpgradeStage.CopyData:
      case UpgradeStage.PostCopyData:
        return difference.IsDataChanged;
      case UpgradeStage.Cleanup:
        return isRemoved;
      default:
        throw new ArgumentOutOfRangeException("Stage");
      }
    }

    /// <summary>
    /// Determines whether is cycle rename detected.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <returns>
    /// <see langword="true"/> if is cycle rename exists; otherwise, <see langword="false"/>.
    /// </returns>
    protected virtual bool IsCyclicRename(NodeDifference difference)
    {
      if (!difference.IsNameChanged || difference.Source==null || difference.Target==null)
        return false;
      var source = difference.Source;
      var target = difference.Target;
      if (!source.Nesting.IsNestedToCollection)
        return false;
      var collection = source.Parent.GetProperty(source.Nesting.PropertyName) as NodeCollection;
      return collection.Contains(target.Name)
        || Hints.OfType<RenameHint>()
          .Any(renameHint => renameHint.TargetPath==source.Path);
    }

    /// <summary>
    /// Determines whether specified property is immutable.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="accessor">The property accessor.</param>
    /// <returns><see langword="true"/> if th specified property is immutable; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Returns <paramref name="accessor"/>.<see cref="PropertyAccessor.IsImmutable"/>
    /// by default.
    /// </remarks>
    protected virtual bool IsImmutable(Difference difference, PropertyAccessor accessor)
    {
      return accessor.IsImmutable;
    }

    /// <summary>
    /// Determines whether specified property is mutable.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="accessor">The property accessor.</param>
    /// <returns><see langword="true"/> if th specified property is mutable; 
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// Returns <paramref name="accessor"/>.<see cref="PropertyAccessor.IsVolatile"/>
    /// by default.
    /// </remarks>
    protected virtual bool IsVolatile(Difference difference, PropertyAccessor accessor)
    {
      return accessor.IsVolatile;
    }

    /// <summary>
    /// Gets the type of the dependency root object for the specified property.
    /// </summary>
    /// <param name="difference">The difference.</param>
    /// <param name="accessor">The property accessor.</param>
    /// <returns>The type of the dependency root object for the specified property;
    /// <see langword="null" />, if none.</returns>
    /// <remarks>
    /// Returns <paramref name="accessor"/>.<see cref="PropertyAccessor.DependencyRootType"/>
    /// by default.
    /// </remarks>
    protected virtual Type GetDependencyRootType(Difference difference, PropertyAccessor accessor)
    {
      return accessor.DependencyRootType;
    }

    /// <summary>
    /// Gets the name of the temporary for the renaming node in case of conflict
    /// (mutual rename).
    /// </summary>
    /// <returns>Temporary node name.</returns>
    protected virtual string GetTemporaryName(Node node)
    {
      var currentNode = CurrentModel.Resolve(node.Path, true) as Node;
      var currentCollection = currentNode.Parent.GetProperty(currentNode.Nesting.PropertyName) as NodeCollection;
      if (currentCollection==null)
        return node.Name;
      
      var tempName = string.Format(TemporaryNameFormat, node.Name);
      var counter = 0;
      while (currentCollection.Contains(tempName))
        tempName = string.Format(TemporaryNameFormat, node.Name + ++counter);

      return tempName;
    }

    /// <summary>
    /// Gets the node path without node name.
    /// </summary>
    /// <param name="node">The node.</param>
    /// <returns>Path.</returns>
    protected static string GetPathWithoutName(Node node)
    {
      var path = node.Path;
      return !path.Contains(Node.PathDelimiter) 
        ? string.Empty 
        : path.Substring(0, path.LastIndexOf(Node.PathDelimiter));
    }

    #region Helper methods

    /// <summary>
    /// Creates new upgrade context.
    /// </summary>
    /// <returns>Newly created <see cref="UpgradeContext"/> instance.</returns>
    protected virtual UpgradeContext CreateContext()
    {
      return new UpgradeContext();
    }

    /// <summary>
    /// Creates the new action group.
    /// </summary>
    /// <param name="comment">The action group comment.</param>
    /// <returns>A disposable deactivating the group.</returns>
    protected IDisposable OpenActionGroup(string comment)
    {
      var oldActions = new {
        Context.PreConditions, 
        Context.Actions, 
        Context.Renames,
        Context.PostConditions
      };
      Context.PreConditions = new GroupingNodeAction {
        Comment = PreConditionsGroupComment
      };
      Context.Actions = new GroupingNodeAction {
        Comment = comment
      };
      Context.Renames = new GroupingNodeAction {
        Comment = RenamesGroupComment
      };
      Context.PostConditions = new GroupingNodeAction {
        Comment = PostConditionsGroupComment
      };
      try {
        return new Disposable( 
          (isDisposing) => {
            var renames = Context.Renames;
            var postConditions = Context.PostConditions;
            var actions = Context.GetMergeActions();
            Context.PreConditions = oldActions.PreConditions;
            Context.Actions = oldActions.Actions;
            Context.Renames = oldActions.Renames;
            Context.PostConditions = oldActions.PostConditions;
            if (actions!=null && actions.Actions.Count!=0)
              Context.Actions.Add(actions);
            renames.Execute(CurrentModel);
            postConditions.Execute(CurrentModel);
          });
      }
      catch {
        Context.Actions = oldActions.Actions;
        Context.Renames = oldActions.Renames;
        Context.PreConditions = oldActions.PreConditions;
        Context.PostConditions = oldActions.PostConditions;
        throw;
      }
    }

    /// <summary>
    /// Appends the specified action to the action sequence that is building now.
    /// </summary>
    /// <param name="actionType">Type of the action.</param>
    /// <param name="action">The action to append.</param>
    /// <exception cref="ArgumentOutOfRangeException"><c>actionType</c> is out of range.</exception>
    /// <exception cref="InvalidOperationException">Invalid <c>Context.DependencyRootType</c>.</exception>
    protected void AddAction(UpgradeActionType actionType, NodeAction action)
    {
      action.Difference = Context.Difference;
      var dependencyRootType = Context.DependencyRootType;
      if (dependencyRootType==null || actionType==UpgradeActionType.Regular) {
        action.Execute(CurrentModel);
        Context.Actions.Add(action);
      }
      else if (actionType==UpgradeActionType.Rename) {
        var parentDifference = Context.Difference.Parent;
        EnumerableUtils.Unfold(Context, c => c.Parent)
          .Where(c => c.Parent.Difference==parentDifference)
          .Take(1)
          .ForEach(c => c.Renames.Add(action));
      }
      else {
        foreach (var ctx in EnumerableUtils.Unfold(Context, c => c.Parent)) {
          var difference = ctx.Difference;
          var any = difference.Target ?? difference.Source;
          var type = any==null ? typeof (object) : any.GetType();
          if (type==dependencyRootType) {
            switch (actionType) {
            case UpgradeActionType.PreCondition:
              action.Execute(CurrentModel);
              ctx.PreConditions.Add(action);
              break;
            case UpgradeActionType.PostCondition:
              ctx.PostConditions.Add(action);
              break;
            default:
              throw new ArgumentOutOfRangeException("actionType");
            }
            return;
          }
        }
        throw new InvalidOperationException(string.Format(
          Strings.ExDifferenceRelatedToXTypeIsNotFoundOnTheUpgradeContextStack, 
          dependencyRootType.GetShortName()));
      }
    }

    private void UpdateHints()
    {
      var originalHints = Hints;
      Hints = new HintSet(CurrentModel, TargetModel);

      // Process RenameHints
      foreach (var renameHint in originalHints.OfType<RenameHint>()) {
        Node sourceNode;
        if (renameHint!=null && TemporaryRenames.TryGetValue(renameHint.SourcePath, out sourceNode))
          Hints.Add(new RenameHint(sourceNode.Path, renameHint.TargetPath));
        else
          Hints.Add(renameHint);
      }

      // Process CopyDataHints
      foreach (var copyDataHint in originalHints.OfType<CopyDataHint>()) {
        var sourceTablePath = GetActualPath(copyDataHint.SourceTablePath);
        var identities = copyDataHint.Identities.Select(pair => 
          new IdentityPair(GetActualPath(pair.Source), pair.Target, pair.IsIdentifiedByConstant))
          .ToList();
        var copiedColumns = copyDataHint.CopiedColumns.Select(pair =>
          new Pair<string>(GetActualPath(pair.First), pair.Second))
          .ToList();
        var newCopyDataHint = new CopyDataHint(sourceTablePath, identities, copiedColumns);
        Hints.Add(newCopyDataHint);
      }

      // Process DeleteDataHints
      foreach (var deleteDataHint in originalHints.OfType<DeleteDataHint>()) {
        if (!deleteDataHint.PostCopy)
          continue; // It's not necessary to copy this hint
        var sourceTablePath = GetActualPath(deleteDataHint.SourceTablePath);
        var identities = deleteDataHint.Identities.Select(pair => 
          new IdentityPair(GetActualPath(pair.Source), pair.Target, pair.IsIdentifiedByConstant))
          .ToList();
        var newDeleteDataHint = new DeleteDataHint(sourceTablePath, identities, true);
        Hints.Add(newDeleteDataHint);
      }

      // Process IgnoreHints
      foreach (var ignoreHint in originalHints.OfType<IgnoreHint>())
        Hints.Add(ignoreHint);
    }

    private string GetActualPath(string oldNodePath)
    {
      Node actualNode;
      return TemporaryRenames.TryGetValue(oldNodePath, out actualNode) 
        ? actualNode.Path : oldNodePath;
    }

    #endregion
  }
}