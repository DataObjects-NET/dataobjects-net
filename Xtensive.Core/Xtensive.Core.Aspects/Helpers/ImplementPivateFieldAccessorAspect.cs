// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.25

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Collections;
using Xtensive.Core.Reflection;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// Private field accessors aspect - provides get and set accessors 
  /// for a set of private fields (<see cref="TargetFields"/>) of a type.
  /// </summary>
  [MulticastAttributeUsage(MulticastTargets.Class | MulticastTargets.Struct)]
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class ImplementPrivateFieldAccessorsAspect : LaosTypeLevelAspect
  {
    private readonly HashSet<string> targetFields;

    ///<summary>
    /// Gets a sequence of field names that should be provided with private accessors.
    ///</summary>
    public string[] TargetFields {
      get {
        return targetFields.ToArray();
      }
    }

    /// <inheritdoc/>
    public override bool CompileTimeValidate(Type type)
    {
      foreach (var fieldName in targetFields) {
        FieldInfo field = null;
        try {
          field = type.UnderlyingSystemType.GetField(fieldName,
            BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static |
            BindingFlags.DeclaredOnly);
        }
        catch (ArgumentNullException) {}
        if (field==null) {
          ErrorLog.Write(SeverityType.Error, Strings.AspectExRequiresToHave,
            GetType().GetShortName(), 
            type.GetShortName(), 
            field);
          return false;
        }
      }

      return true;
    }

    /// <inheritdoc/>
    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Core.Weaver");
      requirements.Tasks.Add("Xtensive.Core.Weaver.WeaverFactory");
      return requirements;
    }

    /// <summary>
    /// Applies this aspect to the specified <paramref name="field"/>.
    /// </summary>
    /// <param name="field">The field to apply the aspect to.</param>
    /// <returns>If it was the first application with the specified set of arguments, the newly created aspect;
    /// otherwise, <see langword="null" />.</returns>
    public static ImplementPrivateFieldAccessorsAspect ApplyOnce(FieldInfo field)
    {
      ArgumentValidator.EnsureArgumentNotNull(field, "field");

      return AppliedAspectSet.AddOrCombine(field, 
        new ImplementPrivateFieldAccessorsAspect(field.Name),
        (a1, a2) => a1.targetFields.UnionWith(a2.targetFields));
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetFields"><see cref="TargetFields"/> property value.</param>
    public ImplementPrivateFieldAccessorsAspect(IEnumerable<string> targetFields)
    {
      this.targetFields = new HashSet<string>(targetFields);
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="targetFields"><see cref="TargetFields"/> property value.</param>
    public ImplementPrivateFieldAccessorsAspect(params string[] targetFields)
    {
      this.targetFields = new HashSet<string>(targetFields);
    }
  }
}