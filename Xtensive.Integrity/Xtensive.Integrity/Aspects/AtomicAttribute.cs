// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Ilyin
// Created:    2007.07.16

using System;
using System.Diagnostics;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Aspects.Internals;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Resources;
using Xtensive.Core.Helpers;
using Xtensive.Core.Aspects;

namespace Xtensive.Integrity.Aspects
{
  /// <summary>
  /// Provides undo\redo features for methods it is applied on
  /// by <see cref="AtomicityContextBase"/> activation.
  /// Ensures results of method call will be undone in case
  /// of exception.
  /// </summary>
  // [MulticastAttributeUsage(MulticastTargets.Property | MulticastTargets.Method)]
  [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class AtomicAttribute : OnMethodBoundaryAspect, 
    ILaosWeavableAspect
  {
    public string     UndoMethodName { get; set; }
    public MethodInfo UndoMethod     { get; private set; }

    int ILaosWeavableAspect.AspectPriority { get { return (int)IntegrityAspectPriority.Atomic; } }
    
    public override bool CompileTimeValidate(MethodBase method)
    {
      if (!AspectHelper.ValidateContextBoundMethod<AtomicityContextBase>(this, method))
        return false;

      // Method is not constructor, so cast is always valid
      var methodInfo = (MethodInfo) method;
      Type type = method.DeclaringType;

      // Ensure type implements IAtomicityAware
      if (!AspectHelper.ValidateBaseType(this, SeverityType.Error, 
        type, true, typeof(IAtomicityAware)))
        return false;
                  
      if (methodInfo.IsSpecialName && methodInfo.Name.StartsWith(WellKnown.GetterPrefix)) {
        string expectedPropertyName = methodInfo.Name.Remove(0, WellKnown.GetterPrefix.Length);

        // This is getter; let's check if it is explicitely marked as [Atomic]
        PropertyInfo propertyInfo = methodInfo.DeclaringType.UnderlyingSystemType.GetProperty(expectedPropertyName, 
          BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (propertyInfo!=null && propertyInfo.GetAttribute<AtomicAttribute>(false)!=null)
          // Property itself is marked as [Atomic]
          return false;

        // Property getter is marked as [Atomic]
        ErrorLog.Write(SeverityType.Warning, AspectMessageType.AspectPossiblyMissapplied,
          AspectHelper.FormatType(GetType()),
          AspectHelper.FormatMember(methodInfo.DeclaringType, methodInfo));
      }

      // Ensure undo method exists (if specified)
      if (!String.IsNullOrEmpty(UndoMethodName)) {
        if (!AspectHelper.ValidateMethod(this, SeverityType.Error,
          type.UnderlyingSystemType, true,
          BindingFlags.Instance | 
            BindingFlags.Static | 
              BindingFlags.Public | 
                BindingFlags.NonPublic,
          typeof (void), UndoMethodName, new[] {typeof (IUndoDescriptor)},
          out methodInfo))
          return false;
      }

      return true;
    }

    [DebuggerStepThrough]
    public override void OnEntry(MethodExecutionEventArgs eventArgs)
    {
      var instance = (IAtomicityAware)eventArgs.Instance;
      var tag      = new AtomicAttributeTag();
      tag.atomicityScope = (AtomicityScope)instance.ActivateContext();
      AtomicityContextBase atomicityContext = AtomicityScope.CurrentContext;
      IRedoDescriptor redoDescriptor = RedoScope.CurrentDescriptor;
      IUndoDescriptor undoDescriptor = UndoScope.CurrentDescriptor;
      bool bRedoIsBlocked = redoDescriptor!=null;
      bool bUndoIsBlocked = !(undoDescriptor==null || undoDescriptor is IGroupUndoDescriptor);
      if (bRedoIsBlocked && bUndoIsBlocked)
        return;

      if (!bRedoIsBlocked && (atomicityContext.Options & AtomicityContextOptions.Redoable)!=0) {
        redoDescriptor = atomicityContext.OperationDescriptorFactory.CreateRedoDescriptor();
        redoDescriptor.CallDescriptor = new MethodCallDescriptor(eventArgs.Instance, eventArgs.Method);
        redoDescriptor.Arguments = eventArgs.GetArguments();
        tag.RedoScope = new RedoScope(redoDescriptor);
      }

      if (!bUndoIsBlocked && (atomicityContext.Options & AtomicityContextOptions.Undoable)!=0) {
        if (UndoMethod==null) {
          undoDescriptor = atomicityContext.OperationDescriptorFactory.CreateGroupUndoDescriptor();
        }
        else {
          undoDescriptor = atomicityContext.OperationDescriptorFactory.CreateUndoDescriptor();
          undoDescriptor.CallDescriptor = new MethodCallDescriptor(eventArgs.Instance, UndoMethod);
        }
        GroupUndoDescriptor groupUndoDescriptor = UndoScope.CurrentDescriptor as GroupUndoDescriptor;
        if (groupUndoDescriptor!=null) {
          undoDescriptor.Group = groupUndoDescriptor;
          groupUndoDescriptor.Operations.Add(undoDescriptor);
        }
        tag.UndoScope = new UndoScope(undoDescriptor);
      }

      if (tag.RedoScope!=null && tag.UndoScope!=null) {
        redoDescriptor.OppositeDescriptor = undoDescriptor;
        undoDescriptor.OppositeDescriptor = redoDescriptor;
      }

      eventArgs.MethodExecutionTag = tag;
    }

    [DebuggerStepThrough]
    public override void OnExit(MethodExecutionEventArgs eventArgs)
    {
      AtomicAttributeTag tag = eventArgs.MethodExecutionTag as AtomicAttributeTag;
      if (tag==null)
        return;

      IUndoDescriptor undoDescriptor = tag.UndoScope!=null ? tag.UndoScope.Descriptor : null;
      if (undoDescriptor is IGroupUndoDescriptor)
        undoDescriptor.Complete();
      bool isUndone = false;
      if (eventArgs.Exception!=null && undoDescriptor!=null && undoDescriptor.IsCompleted) {
        Log.Debug("Undoing: {0}.", undoDescriptor);
        using (UndoScope.CreateBlockingScope()) {
          try {
            undoDescriptor.Invoke();
            isUndone = true;
          }
          catch (Exception e) {
            Log.Error(e, Strings.LogUndoError, undoDescriptor);
          }
        }
      }

      if (tag.UndoScope!=null) {
        undoDescriptor = tag.UndoScope.Descriptor;
        undoDescriptor.Finalize(isUndone);
      }
      if (tag.RedoScope!=null) {
        IRedoDescriptor redoDescriptor = tag.RedoScope.Descriptor;
        redoDescriptor.Finalize(isUndone);
        // Logging the RedoDescriptor
        IOperationLog operationLog = AtomicityScope.CurrentContext.OperationLog;
        if (!redoDescriptor.IsUndone && operationLog!=null)
          operationLog.Append(redoDescriptor);
      }
      tag.UndoScope.DisposeSafely();
      tag.RedoScope.DisposeSafely();
      tag.atomicityScope.DisposeSafely();
    }


    // Constructors

    public AtomicAttribute()
      : this(null)
    {
    }

    public AtomicAttribute(string undoMethodName)
    {
      UndoMethodName = undoMethodName;
    }
  }
}