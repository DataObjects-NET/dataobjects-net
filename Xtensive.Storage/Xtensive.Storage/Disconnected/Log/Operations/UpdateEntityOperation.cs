// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected.Log.Operations
{
  [Serializable]
  public class UpdateEntityOperation : IUpdateEntityOperation
  {
    public Key Key { get; private set; }
    public EntityOperationType Type { get; private set; }
    public FieldInfo FieldInfo { get; private set; }
    public object Value { get; private set; }

    public void Prepare(PrefetchContext prefetchContext)
    {
      prefetchContext.Register(Key);
    }

    public void Execute(Session session)
    {
      var entity = Query.Single(session, Key);
      var setter = DelegateHelper.CreateDelegate<Action<Entity>>(
        this, 
        typeof (UpdateEntityOperation), 
        "ExecuteSetValue", 
        FieldInfo.ValueType);
      setter.Invoke(entity);
    }

    private void ExecuteSetValue<T>(Entity entity)
    {
      entity.SetFieldValue(FieldInfo, (T)Value);
    }

    
    // Constructors

    public UpdateEntityOperation(Key key, FieldInfo fieldInfo, object value)
    {
      Key = key;
      Type = EntityOperationType.Update;
      FieldInfo = fieldInfo;
      Value = value;
    }
  }
}