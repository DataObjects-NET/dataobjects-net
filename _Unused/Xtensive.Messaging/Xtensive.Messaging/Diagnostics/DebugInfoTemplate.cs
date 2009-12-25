// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.10.19

using System;
using Xtensive.Core;
using Xtensive.Messaging.Resources;

namespace Xtensive.Messaging.Diagnostics
{
  /// <summary>
  /// Template (base class) for any debug information providers.
  /// </summary>
  /// <typeparam name="T">Should always be the type of descendant.</typeparam>
  public class DebugInfoTemplate<T>
  {
    private static readonly Type syncRoot = typeof(DebugInfoTemplate<T>);
    private static bool isOperableDefined;
    private static bool isOperable;
    private static readonly Random random = new Random();

    // Properties

    protected static object SyncRoot
    {
      get { return syncRoot; }
    }

    protected static bool IsOperableDefined
    {
      get { return isOperableDefined; }
    }

    public static bool IsOperable
    {
      get
      {
        if (isOperableDefined)
          return isOperable;
        lock (syncRoot)
        {
          isOperable = Core.Diagnostics.DebugInfo.IsUnitTestSessionRunning;
          isOperableDefined = true;
          return isOperable;
        }
      }
      set
      {
        lock (syncRoot)
        {
          if (isOperableDefined)
            Exceptions.AlreadyInitialized("IsOperable");
          isOperable = value;
          isOperableDefined = true;
        }
      }
    }

    // Methods

    protected static void EnsureOperable()
    {
      if (!IsOperable)
        throw new NotSupportedException(Strings.ExDebugInfoIsNotAvailable);
    }

    protected static double Random
    {
      get
      {
        lock (SyncRoot)
        {
          return random.NextDouble();
        }
      }
    }
  }
}