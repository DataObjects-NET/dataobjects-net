// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;

namespace Xtensive.Storage
{
  public abstract class Persistent : SessionBound
  {
    private Dictionary<FieldInfo, IFieldHandler> fieldHandlers;

    /// <summary>
    /// Gets the type of this instance.
    /// </summary>
    [DebuggerHidden]
    public abstract TypeInfo Type { get; }

    [DebuggerHidden]
    protected abstract internal Tuple Tuple { get; }

    internal Dictionary<FieldInfo, IFieldHandler> FieldHandlers
    {
      get
      {
        if (fieldHandlers == null)
          fieldHandlers = new Dictionary<FieldInfo, IFieldHandler>();
        return fieldHandlers;
      }
    }

    public T  GetProperty<T>(string name)
    {
      FieldInfo field = Type.Fields[name];
      if (field.UnderlyingProperty!=null)
        return (T) field.UnderlyingProperty.GetValue(this, null);
      return GetValue<T>(name);
    }

    public void SetProperty<T>(string name, T value)
    {
      FieldInfo field = Type.Fields[name];
      if (field.UnderlyingProperty!=null)
        field.UnderlyingProperty.SetValue(this, value, null);
      else
        SetValue(name, value);
    }

    public object this[string name]
    {
      get { return GetProperty<object>(name); }
      set { SetProperty(name, value); }
    }

    protected T GetValue<T>(string name)
    {
      FieldInfo field = Type.Fields[name];
      OnGettingValue(field);
      T result = field.GetAccessor<T>().GetValue(this, field);
      OnGet(field);
      return result;
    }

    protected void SetValue<T>(string name, T value)
    {
      FieldInfo field = Type.Fields[name];
      OnSettingValue(field);
      field.GetAccessor<T>().SetValue(this, field, value);
      OnSetValue(field);
    }

    protected internal virtual void OnCreating()
    {
    }

    protected internal virtual void OnCreated(Type constructedType)
    {
    }

    protected internal virtual void OnGettingValue(FieldInfo fieldInfo)
    {
    }

    protected internal virtual void OnGet(FieldInfo fieldInfo)
    {
    }

    protected internal virtual void OnSettingValue(FieldInfo fieldInfo)
    {
    }

    protected internal virtual void OnSetValue(FieldInfo fieldInfo)
    {
    }


    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="Persistent"/> class.
    /// </summary>
    protected Persistent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Persistent"/> class.
    /// </summary>
    protected Persistent(EntityData data) : this()
    {
    }
  }
}