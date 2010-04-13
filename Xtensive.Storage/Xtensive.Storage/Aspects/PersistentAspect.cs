// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.13

using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects;
using Xtensive.Core.Reflection;
using Xtensive.Integrity.Aspects;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Property, AllowMultiple = false, Inheritance = MulticastInheritance.Multicast, 
    TargetMemberAttributes = 
      MulticastAttributes.AnyVisibility | 
      MulticastAttributes.Instance | 
      MulticastAttributes.NonAbstract | 
      MulticastAttributes.Managed)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
  public class PersistentAspect : Aspect,
    IAspectProvider
  {
    private const string HandlerMethodSuffix = "FieldValue";
    private static readonly ReplaceAutoProperty replacer;
    private static readonly TransactionalAttribute transactional;

    #region IAspectProvider Members

    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      var propertyInfo = (PropertyInfo) targetElement;
      var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(AttributeSearchOptions.InheritFromAllBase);
      if (fieldAttribute == null)
        return result;

      var type = propertyInfo.DeclaringType;
      var keyAttribute = propertyInfo.GetAttribute<KeyAttribute>(AttributeSearchOptions.InheritNone);
      var getter = propertyInfo.GetGetMethod(true);
      var setter = propertyInfo.GetSetMethod(true);
      if (getter != null)
        result.Add(new AspectInstance(getter, replacer));
      if (setter != null) {
        if (keyAttribute != null) {
          string errorMessage = string.Format(Strings.ExKeyFieldXInTypeYShouldNotHaveSetAccessor, propertyInfo.Name, type.Name);
          var notSupportedAspect = new NotSupportedAttribute(errorMessage);
          result.Add(new AspectInstance(setter, notSupportedAspect));
        }
        result.Add(new AspectInstance(setter, replacer));

        // If there are constraints, we must "wrap" setter into transaction
        var constraints = 
          propertyInfo.GetAttributes<PropertyConstraintAspect>(AttributeSearchOptions.InheritNone);
        if (!(constraints == null || constraints.Length == 0))
          result.Add(new AspectInstance(setter, transactional));
      }
      return result;
    }

    #endregion

    static PersistentAspect()
    {
      replacer = new ReplaceAutoProperty(HandlerMethodSuffix);
      transactional = new TransactionalAttribute();
    }
  }
}