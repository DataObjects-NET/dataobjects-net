// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

namespace Xtensive.Core.Links
{
  public abstract class Operation<TArg> : ILockable
  {
    private bool isLocked;
    private string linkedPropertyName;
    private string propertyName;

    protected LinkType ProtectedLinkType = LinkType.Unknown;

    public LinkType LinkType
    {
      get { return ProtectedLinkType; }
      set
      {
        if (IsLocked)
          throw new InstanceIsLockedException();
        if (ProtectedLinkType==LinkType.Unknown)
          ProtectedLinkType = value;
        else
          throw Exceptions.AlreadyInitialized("LinkType");
      }
    }

    public string LinkedPropertyName
    {
      get { return linkedPropertyName; }
      set
      {
        if (IsLocked)
          throw new InstanceIsLockedException();
        if (linkedPropertyName!=null)
          throw Exceptions.AlreadyInitialized("LinkedPropertyName");
        linkedPropertyName = value;
      }
    }

    public string PropertyName
    {
      get { return propertyName; }
      set
      {
        if (IsLocked)
          throw new InstanceIsLockedException();
        if (propertyName!=null)
          throw Exceptions.AlreadyInitialized("PropertyName");
        propertyName = value;
      }
    }

    public abstract void ExecuteCall(ref TArg arg, ref ExecutionStage executionStage);

    internal abstract void ExecuteOperation(ref TArg arg);

    public void Execute(ref TArg arg)
    {
      bool newOperationStarted = ExecutionContextSwitcher.Current.CurrentContext.BeginOperation();
      try {
        ExecuteOperation(ref arg);
      }
      finally {
        if (newOperationStarted)
          ExecutionContextSwitcher.Current.CurrentContext.EndOperation();
      }
    }



    public bool IsLocked
    {
      get { return isLocked; }
    }

    public void Lock()
    {
      Lock(true);
    }

    public virtual void Lock(bool recursive)
    {
      isLocked = true;
    }



    protected Operation()
    {
    }
  }
}