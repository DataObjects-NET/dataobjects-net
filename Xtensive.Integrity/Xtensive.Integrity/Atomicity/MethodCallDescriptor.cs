// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.11.23

using System;
using System.Diagnostics;
using System.Reflection;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Integrity.Atomicity
{
  /// <summary>
  /// Describes method call operation (without arguments).
  /// </summary>
  [Serializable]
  public struct MethodCallDescriptor : IEquatable<MethodCallDescriptor>
  {
    /// <summary>
    /// Gets the method target.
    /// </summary>    
    public object Target { get; private set; }

    /// <summary>
    /// Gets the method.
    /// </summary>    
    public MethodBase Method { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this call descriptor is valid.
    /// </summary>
    public bool IsValid {
      [DebuggerStepThrough]
      get {
        if (Method==null)
          return false;
        return Method.IsStatic ^ Target!=null;
      }
    }

    /// <inheritdoc/>
    public override string ToString()
    {
      if (!IsValid)
        return "Invalid CallDescriptor";
      else {
        if (Target==null)
          return String.Format("static {0}::{1}", Method.DeclaringType.Name, Method.Name);
        else 
          return String.Format("\"{2}\".{0}::{1}", Method.DeclaringType.Name, Method.Name, Target);
      }
    }

    #region Equals & GetHashCode

    public bool Equals(MethodCallDescriptor methodCallDescriptor)
    {
      return Equals(Target, methodCallDescriptor.Target) && Equals(Method, methodCallDescriptor.Method);
    }

    public override bool Equals(object obj)
    {
      if (!(obj is MethodCallDescriptor)) return false;
      return Equals((MethodCallDescriptor) obj);
    }

    public override int GetHashCode()
    {
      return 
        (Target!=null ? Target.GetHashCode() : 0) + 
          29*(Method!=null ? Method.GetHashCode() : 0);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="target">The target.</param>
    /// <param name="method">The method.</param>
    public MethodCallDescriptor(object target, MethodBase method)
      : this()
    {
      this.Target = target;
      this.Method = method;
      if (!IsValid) {
        this.Method = null;
        this.Target = null;
      }
    }
  }
}