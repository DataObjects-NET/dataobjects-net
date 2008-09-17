// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.01

using System;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Disposable;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Atomicity;
using Xtensive.Integrity.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Integrity.Relations
{
  public abstract class OneToOneRelationManager<TMaster, TSlave, TVariator>
    where TMaster: class
    where TSlave:  class
  {
    private static Action<TMaster, TSlave> setMasterProperty;
    private static Action<TSlave, TMaster> setSlaveProperty;
    private static Func<TMaster, TSlave> getMasterProperty;
    private static Func<TSlave, TMaster> getSlaveProperty;
    private static bool UndoEnabled;

    public static bool Initialize(string masterPropertyName, string slavePropertyName)
    {
      bool requiresUndo = !typeof (IAtomicityAware).IsAssignableFrom(typeof (TMaster));
      return Initialize(masterPropertyName, slavePropertyName, requiresUndo);
    }

    public static bool Initialize(string masterPropertyName, string slavePropertyName, bool undoEnabled)
    {
      try {
        setMasterProperty = DelegateHelper.CreateSetMemberDelegate<TMaster, TSlave>(masterPropertyName);
        getMasterProperty = DelegateHelper.CreateGetMemberDelegate<TMaster, TSlave>(masterPropertyName);
        setSlaveProperty = DelegateHelper.CreateSetMemberDelegate<TSlave, TMaster>(slavePropertyName);
        getSlaveProperty = DelegateHelper.CreateGetMemberDelegate<TSlave, TMaster>(slavePropertyName);
        UndoEnabled = undoEnabled;
        return true;
      }
      catch {
        return false;
      }
    }

    public static void SetMaster(TMaster master, TSlave value, Action<TMaster, TSlave> actualSetMasterProperty)
    {
      var oldValue = getMasterProperty(master);
      if (oldValue==value)
        return;
      IDisposable scope;
      var context = AcquireContext(master, value, out scope);
      try {
        switch (context.Stage) {
        case OneToOneRelationSyncStage.Default:
        case OneToOneRelationSyncStage.MasterSetterInvoked:
          // Passing further control to slave back
          if (value!=null) {
            context.OldSlave = oldValue;
            PrepareForCall(context, OneToOneRelationSyncStage.SlaveSetterInvoked, value, master);
            setSlaveProperty(value, master);
          }
          else {
            PrepareForCall(context, OneToOneRelationSyncStage.OldSlaveSetterInvoked, oldValue, null);
            setSlaveProperty(oldValue, null); // oldValue!=null here
          }
          // Slaves are updated. Updating ourselves.
          actualSetMasterProperty(master, value);
          break;
        default:
          throw Log.Error(new InvalidOperationException(String.Format(Strings.ExIncorrectStageValue, context.Stage)));
        }
      }
      catch {
        if (UndoEnabled) try {
          actualSetMasterProperty(master, oldValue);
        }
        catch (Exception e) {
          Log.Error(e, Strings.LogRelationSyncUndoError);
        }
        throw;
      } 
      finally {
        scope.DisposeSafely();
      }
    }

    public static void SetSlave(TSlave slave, TMaster value, Action<TSlave, TMaster> actualSetSlaveProperty)
    {
      var oldValue = getSlaveProperty(slave);
      if (oldValue==value)
        return;
      IDisposable scope;
      var context = AcquireContext(slave, value, out scope);
      try {
        switch (context.Stage) {
        case OneToOneRelationSyncStage.Default:
          // Pasing control to master
          if (value!=null) {
            PrepareForCall(context, OneToOneRelationSyncStage.MasterSetterInvoked, value, slave);
            setMasterProperty(value, slave);
          }
          else {
            PrepareForCall(context, OneToOneRelationSyncStage.MasterSetterInvoked, oldValue, null);
            setMasterProperty(oldValue, null);
          }
          break;
        case OneToOneRelationSyncStage.SlaveSetterInvoked:
          // Pasing control to old slave, if possible
          if (context.OldSlave!=null) {
            PrepareForCall(context, OneToOneRelationSyncStage.OldSlaveSetterInvoked, context.OldSlave, null);
            setSlaveProperty(context.OldSlave, null);
          }
          // Old slaves is updated. Updating ourselves.
          actualSetSlaveProperty(slave, value);
          break;
        case OneToOneRelationSyncStage.OldSlaveSetterInvoked:
          // Nothing is updated. Updating ourselves.
          actualSetSlaveProperty(slave, value);
          break;
        default:
          throw Log.Error(new InvalidOperationException(String.Format(Strings.ExIncorrectStageValue, context.Stage)));
        }
      }
      catch {
        if (UndoEnabled) try {
          actualSetSlaveProperty(slave, oldValue);
        }
        catch (Exception e) {
          Log.Error(e, Strings.LogRelationSyncUndoError);
        }
        throw;
      }
      finally {
        scope.DisposeSafely();
      }
    }

    #region Private \ internal methods

    private static OneToOneRelationSyncContext<TMaster, TSlave, TVariator> AcquireContext(
      object sceduledCallTarget, object sceduledCallValue, out IDisposable scope)
    {
      scope = null;
      var context = RelationSyncScope<OneToOneRelationSyncContext<TMaster, TSlave, TVariator>>.CurrentContext;
      if (context==null || 
        context.SceduledCallTarget!=sceduledCallTarget || 
          context.SceduledCallValue!=sceduledCallValue) {
        context = new OneToOneRelationSyncContext<TMaster, TSlave, TVariator>();
        scope = new RelationSyncScope<OneToOneRelationSyncContext<TMaster, TSlave, TVariator>>(context);
        context.SceduledCallTarget = sceduledCallTarget;
        context.SceduledCallValue  = sceduledCallValue;
      }
      return context;
    }

    private static void PrepareForCall(OneToOneRelationSyncContext<TMaster, TSlave, TVariator> context, 
      OneToOneRelationSyncStage stage, object sceduledCallTarget, object sceduledCallValue)
    {
      context.Stage = stage;
      context.SceduledCallTarget = sceduledCallTarget;
      context.SceduledCallValue  = sceduledCallValue;
    }

    #endregion


    // Type initializer

    static OneToOneRelationManager()
    {
      // Let's ensure static constructor of TVariator is invoked
      Type variatorType = typeof (TVariator);
      FieldInfo fi = variatorType.GetField("Initialized", BindingFlags.Public | BindingFlags.Static);
      if (fi!=null)
        fi.GetValue(null);
    }
  }
}