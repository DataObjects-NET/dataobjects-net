// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.06.02

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Providers
{
  public abstract class HandlerProvider
  {
    private TypeBasedDictionary providerCache;
    private static Assembly storageAssebmbly = typeof(HandlerProvider).Assembly;
    private static MethodInfo setValueInfo = typeof(TypeBasedDictionary).GetMethod("SetValue");

    public T GetHandler<T>()
    {
      Delegate constructorDelegate;
      if (null == (constructorDelegate = providerCache.GetValue<T, Delegate>()))
        return default(T);
      return ((Func<T>)constructorDelegate)();
    }

    public HandlerProvider()
    {
      providerCache.Initialize();
      Assembly assembly = GetType().Assembly;
      foreach (Type type in assembly.GetTypes()) {
        if (type.IsAbstract || type.IsNotPublic)
          continue;
        Type baseType = type.BaseType;
        while (baseType != null) {
          if (baseType.Assembly == storageAssebmbly) {
            setValueInfo.MakeGenericMethod(new Type[] {baseType, typeof (Delegate)})
              .Invoke(providerCache,
                      new object[]
                        {
                          DelegateHelper.CreateClassConstructorDelegate(
                            type,
                            typeof (Func<>).MakeGenericType(baseType))
                        });
            baseType = null;
            continue;
          }
          baseType = baseType.BaseType;
        }
      }
    }
  }
}