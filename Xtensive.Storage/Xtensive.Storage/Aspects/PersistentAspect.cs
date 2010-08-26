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
#if NET40
  [SecuritySafeCritical]
#endif
  internal class PersistentAspect : Aspect,
    IAspectProvider
  {
    private const string HandlerMethodSuffix = "FieldValue";
    private static readonly ReplaceAutoProperty replacer;

    #region IAspectProvider Members

    public IEnumerable<AspectInstance> ProvideAspects(object targetElement)
    {
      var result = new List<AspectInstance>();
      var propertyInfo = (PropertyInfo) targetElement;
      var type = propertyInfo.DeclaringType;
      var fieldAttribute = propertyInfo.GetAttribute<FieldAttribute>(AttributeSearchOptions.InheritNone);
      var allFieldAttributes = propertyInfo.GetAttributes<FieldAttribute>(AttributeSearchOptions.InheritFromAllBase);
      const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
      if (allFieldAttributes != null && allFieldAttributes.Length > 0) {
        if (fieldAttribute == null)
          fieldAttribute = allFieldAttributes.First();
        else if (allFieldAttributes.Any(a => !a.IsCompatibleWith(fieldAttribute))) {
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
      }
      return result;
    }

    #endregion

    static PersistentAspect()
    {
      replacer = new ReplaceAutoProperty(HandlerMethodSuffix);
    }
  }
}