// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.09.25

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Custom attribute that, when applied to a method defined in the current assembly, inserts a piece
  /// of code before and after the body of these methods. This custom attribute can be multicasted
  /// (see <see cref="PostSharp.Extensibility.MulticastAttribute" />).
  /// </summary>
  /// <remarks>
  /// This custom attribute is useful to implement "boundary" functionalities like tracing (writing a 
  /// line to log) or transactions (automatically start a transaction at entry and commit or rollback
  /// at exit).
  /// </remarks>
  [MulticastAttributeUsage(
    MulticastTargets.Constructor | MulticastTargets.Method, 
    AllowMultiple = true,
    TargetMemberAttributes = 
      MulticastAttributes.Managed | 
      MulticastAttributes.NonAbstract | 
      MulticastAttributes.AnyScope | 
      MulticastAttributes.AnyVisibility)] 
  [AttributeUsage(
    AttributeTargets.Event | 
    AttributeTargets.Property | 
    AttributeTargets.Method | 
    AttributeTargets.Constructor, 
    AllowMultiple = true, Inherited = false)]
  [Serializable]
  public abstract class ImplementFastMethodBoundaryAspect : LaosMethodLevelAspect
  {
    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      AspectHelper.AddStandardRequirements(requirements);
      return requirements;
    }

    /// <summary>
    /// Method executed <b>before</b> the body of methods to which this aspect is applied.
    /// </summary>
    /// <param name="instance">Current object instance on which this attribute is applied.</param>
    /// <remarks>
    /// If the aspect is applied to a constructor, the current method is invoked
    /// after the <b>this</b> pointer has been initialized, that is, after
    /// the base constructor has been called.
    /// </remarks>
    public abstract object OnEntry(object instance);

    /// <summary>
    /// Method executed <b>after</b> the body of methods to which this aspect is applied,
    /// even when the method exists with an exception (this method is invoked from
    /// the <b>finally</b> block).
    /// </summary>
    /// <param name="instance">Current object instance on which this attribute is applied.</param>
    /// <param name="onEntryResult">Result of <see cref="OnEntry"/> method call.</param>
    public abstract void OnExit(object instance, object onEntryResult);

    /// <summary>
    /// Method executed <b>after</b> the body of methods to which this aspect is applied,
    /// but only when the method succesfully returns (i.e. when no exception flies out
    /// the method.).
    /// </summary>
    /// <param name="instance">Current object instance on which this attribute is applied.</param>
    /// <param name="onEntryResult">Result of <see cref="OnEntry"/> method call.</param>
    public abstract void OnSuccess(object instance, object onEntryResult);

    /// <summary>
    /// Method executed when the body of methods to which this aspect is applied throws an exception.
    /// </summary>
    /// <returns>An exception will be thrown when result is <see langword="true"/>.</returns>
    /// <param name="instance">Current object instance on which this attribute is applied.</param>
    /// <param name="e">Throwed exception.</param>
    public abstract bool OnError(object instance, Exception e);
  }
}