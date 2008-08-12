// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using System.Diagnostics;
using System.Reflection;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Describes method call operation (without arguments).
  /// </summary>
  [Serializable]
  public struct MethodCallDescriptor : IEquatable<MethodCallDescriptor>
  {
    private object target;
    private MethodBase method;

    public object Target {
      [DebuggerStepThrough]
      get { return target; }
    }

    public MethodBase Method {
      [DebuggerStepThrough]
      get { return method; }
    }

    public bool IsValid {
      [DebuggerStepThrough]
      get {
        if (method==null)
          return false;
        return method.IsStatic ^ target!=null;
      }
    }

    public override string ToString()
    {
      if (!IsValid)
        return "Invalid CallDescriptor";
      else {
        if (target==null)
          return String.Format("static {0}::{1}", method.DeclaringType.Name, method.Name);
        else 
          return String.Format("\"{2}\".{0}::{1}", method.DeclaringType.Name, method.Name, target);
      }
    }

    #region Equals & GetHashCode

    public bool Equals(MethodCallDescriptor methodCallDescriptor)
    {
      return Equals(target, methodCallDescriptor.target) && Equals(method, methodCallDescriptor.method);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is MethodCallDescriptor)) return false;
      return Equals((MethodCallDescriptor) obj);
    }

    public override int GetHashCode()
    {
      return 
        (target!=null ? target.GetHashCode() : 0) + 
          29*(method!=null ? method.GetHashCode() : 0);
    }

    #endregion

    // Constructors

    public MethodCallDescriptor(object target, MethodBase method)
    {
      this.target = target;
      this.method = method;
      if (!IsValid) {
        this.method = null;
        this.target = null;
      }
    }
  }
}