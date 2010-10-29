// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.13

using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Security;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using Xtensive.Aspects;
using Xtensive.Reflection;
using Xtensive.Orm.Validation;
using Xtensive.Orm.Resources;

namespace Xtensive.Orm
{
  [Serializable]
  [MulticastAttributeUsage(MulticastTargets.Property, AllowMultiple = false, Inheritance = MulticastInheritance.Multicast, 
    TargetMemberAttributes = 
      MulticastAttributes.AnyVisibility | 
      MulticastAttributes.Instance | 
      MulticastAttributes.NonAbstract | 
      MulticastAttributes.Managed)]
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
#if NET40
  [SecuritySafeCritical]
#endif
  internal class PersistentAspect : Aspect,
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
      var type = propertyInfo.DeclaringType;
      // TODO: check integrity of AssociationAttribute instances in same manner
      var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>();
      var allFieldAttributes = propertyInfo.GetAttributes<FieldAttribute>(AttributeSearchOptions.InheritFromAllBase);
      const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      if (allFieldAttributes != null && allFieldAttributes.Length > 0) {
        if (fieldAttribute == null)
          fieldAttribute = allFieldAttributes.First();
        if (allFieldAttributes.Any(a => !a.IsCompatibleWith(fieldAttribute))) {
          var typeName = type.GetShortName();
          var propName = propertyInfo.GetShortName(false);
          var interfaceName = type.GetInterfaces()
            .First(i => typeof (IEntity).IsAssignableFrom(i)&& i.GetProperty(propName, bindingFlags) != null)
            .GetShortName();
          ErrorLog.Write(
            SeverityType.Error, 
            "\"{0}.{1}\" has [Field] incompatible with \"{2}.{1}\"", 
            typeName, 
            propName, 
            interfaceName);
        }
      }
      if (fieldAttribute == null)
        return result;

      var associationAttribute = propertyInfo.GetAttribute<AssociationAttribute>();
      var associationAttributes = propertyInfo.GetAttributes<AssociationAttribute>(AttributeSearchOptions.InheritFromAllBase); 
      if (associationAttributes.Length > 1) {
        if (associationAttribute == null)
          associationAttribute = associationAttributes.First();
        if (associationAttributes.Any(a => !a.IsCompatibleWith(associationAttribute))) {
          var typeName = type.GetShortName();
          var propName = propertyInfo.GetShortName(false);
          var interfaceName = type.GetInterfaces()
            .First(i => typeof (IEntity).IsAssignableFrom(i)&& i.GetProperty(propName, bindingFlags) != null)
            .GetShortName();
          ErrorLog.Write(
            SeverityType.Error, 
            "\"{0}.{1}\" has [Association] incompatible with \"{2}.{1}\"", 
            typeName, 
            propName, 
            interfaceName);
        }
      }

      var keyAttribute = propertyInfo.GetAttribute<KeyAttribute>();
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
          propertyInfo.GetAttributes<PropertyConstraintAspect>();
        if (!(constraints == null || constraints.Length == 0))
          result.Add(new AspectInstance(setter, transactional));
      }
      return result;
    }

    #endregion

    static PersistentAspect()
    {
      replacer = new ReplaceAutoProperty(HandlerMethodSuffix);
      transactional = new TransactionalAttribute(TransactionalBehavior.Auto);
    }
  }
}