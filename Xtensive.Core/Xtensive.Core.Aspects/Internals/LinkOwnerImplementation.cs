// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.27

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Links;
using Xtensive.Core.Links.LinkedReference;
using Xtensive.Core.Reflection;
using Xtensive.Core.Aspects.Resources;
using Xtensive.Core.Links.LinkedSet;
using FieldInfo=System.Reflection.FieldInfo;
using MethodInfo=System.Reflection.MethodInfo;

namespace Xtensive.Core.Aspects.Internals
{
  public class LinkOwnerImplementation: ILinkOwner
  {
    private Type type;
    private LinkedOperationRegistry operations;

    public LinkedOperationRegistry Operations
    {
      get { return operations; }
    }

    private void RegisterOperations(FieldInfo field, LinkAttribute linkAttribute)
    {
      Type objectType = typeof (object);
      Type linkedSetType = typeof(LinkedSet<,>);
      Type linkedReferenceType = typeof(LinkedReference<,>);

      Type fieldType = field.FieldType;
      while (fieldType!=objectType) {
        if (!fieldType.IsGenericType) {
          fieldType = fieldType.BaseType;
          continue;
        }
        Type genericFieldType = fieldType.GetGenericTypeDefinition();
        if (genericFieldType==linkedSetType) {
          // LinkedSet field
          Type ownerType = fieldType.GetGenericArguments()[0];
          MethodInfo createGetMemberDelegateMethod = typeof(DelegateHelper).GetMethod("CreateGetMemberDelegate", BindingFlags.Public | BindingFlags.Static);
          createGetMemberDelegateMethod = createGetMemberDelegateMethod.MakeGenericMethod(ownerType, field.FieldType);
          MethodInfo exportOperationsMethod = fieldType.GetMethod("ExportOperations", BindingFlags.Public | BindingFlags.Static);
          exportOperationsMethod.Invoke(null, new object[] { Operations, 
            linkAttribute.OwnerProperty, linkAttribute.DependentProperty, linkAttribute.LinkType,
            createGetMemberDelegateMethod.Invoke(null, new object[] {field.Name}) });
          return;
        }
        if (genericFieldType == linkedReferenceType)
        {
          // LinkedReference field
          Type ownerType = fieldType.GetGenericArguments()[0];
          MethodInfo createGetMemberDelegateMethod = typeof(DelegateHelper).GetMethod("CreateGetMemberDelegate", BindingFlags.Public | BindingFlags.Static);
          createGetMemberDelegateMethod = createGetMemberDelegateMethod.MakeGenericMethod(ownerType, field.FieldType);
          MethodInfo exportOperationsMethod = fieldType.GetMethod("ExportOperations", BindingFlags.Public | BindingFlags.Static);
          exportOperationsMethod.Invoke(null, new object[] { Operations, 
            linkAttribute.OwnerProperty, linkAttribute.DependentProperty, linkAttribute.LinkType,
            createGetMemberDelegateMethod.Invoke(null, new object[] {field.Name}) });
          return;
        }
        fieldType = fieldType.BaseType;
      }
      throw new InvalidOperationException(Strings.ExUnknownLinkedFieldType);
    }


    // Constructors

    public LinkOwnerImplementation(Type type, Dictionary<FieldInfo, LinkAttribute> links, LinkOwnerImplementation baseImplementation)
    {
      this.type = type;
      this.operations = new LinkedOperationRegistry(baseImplementation!=null ? baseImplementation.Operations : null);
      foreach (KeyValuePair<FieldInfo, LinkAttribute> p in links)
        RegisterOperations(p.Key, p.Value);
    }
  }
}