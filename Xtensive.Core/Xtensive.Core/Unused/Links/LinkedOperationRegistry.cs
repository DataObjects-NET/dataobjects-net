// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2007.06.04

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using Xtensive.Core.Resources;

namespace Xtensive.Core.Links
{
  public sealed class LinkedOperationRegistry
  {
    private HybridDictionary operations = new HybridDictionary();

    public static object GetKey(Type operationType, string associatedPropertyName, string operationName)
    {
      return GetKey(operationType, associatedPropertyName) +
        (operationName==null ? "" : (":" + operationName));
    }

    public static object GetKey(MemberInfo operationType, string associatedPropertyName)
    {
      return operationType.Name + ":" + associatedPropertyName;
    }

    public void Register<TArg>(Operation<TArg> operation)
    {
      Register(operation, null);
    }

    public void Register<TArg>(Operation<TArg> operation, string operationName)
    {
      ArgumentValidator.EnsureArgumentNotNull(operation, "operation");

      if (!operation.IsLocked)
        throw new InvalidOperationException(Strings.ExOperationMustBeLocked);

      if (operation.LinkType==LinkType.Unknown)
        throw new InvalidOperationException(Strings.ExOperationLinkTypeUndefined);

      if (operation.PropertyName==null)
        throw new InvalidOperationException(Strings.ExOperationPropertyNameUndefined);

      Type currentType = operation.GetType();
      foreach (Type type in currentType.GetInterfaces()) {
        operations[GetKey(type, operation.PropertyName, operationName)] = operation;
        if (operationName!=null)
          operations[GetKey(type, operation.PropertyName, null)] = operation;
      }

      while (typeof (Operation<TArg>).IsAssignableFrom(currentType)) {
        operations[GetKey(currentType, operation.PropertyName, operationName)] = operation;
        if (operationName!=null)
          operations[GetKey(currentType, operation.PropertyName, null)] = operation;
        currentType = currentType.BaseType;
      }
    }

    public TOperation Find<TOperation>(object key)
    {
      return (TOperation)operations[key];
    }

    // Constructor.

    public LinkedOperationRegistry()
    {
      operations = new HybridDictionary();
    }

    public LinkedOperationRegistry(LinkedOperationRegistry prototype)
    {
      operations = new HybridDictionary();
      if (prototype==null)
        return;
      foreach (DictionaryEntry entry in prototype.operations) {
        operations.Add(entry.Key, entry.Value);
      }
    }
  }
}