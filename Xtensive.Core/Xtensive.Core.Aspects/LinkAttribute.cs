// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Anton U. Rogozhin
// Created:    2007.07.26

using System;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Laos;
using Xtensive.Core.Aspects.Internals;
using Xtensive.Core.Links;

namespace Xtensive.Core.Aspects
{
  [MulticastAttributeUsage(MulticastTargets.Field)]
  [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
  [Serializable]
  public sealed class LinkAttribute : CompoundAspect
  {
    private string ownerProperty, dependentProperty;
    private LinkType linkType;
    
    public string OwnerProperty
    {
      get { return ownerProperty; }
    }

    public string DependentProperty
    {
      get { return dependentProperty; }
    }

    public LinkType LinkType
    {
      get { return linkType; }
    }

    public override void ProvideAspects(object element, LaosReflectionAspectCollection collection)
    {
      FieldInfo field = (FieldInfo)element;
      if ((field.Attributes & (FieldAttributes.Private | FieldAttributes.FamORAssem)) != 0) {
        ImplementPrivateFieldAccessorAspect ipfaa = ImplementPrivateFieldAccessorAspect.FindOrCreate(field);
        collection.AddAspect(field.DeclaringType, ipfaa);
      }
      ImplementLinkOwnerAspect iloa = ImplementLinkOwnerAspect.FindOrCreate(field.DeclaringType);
      bool bNewIloa = iloa.Links.Count == 0;
      iloa.Links[field] = this;
      if (bNewIloa) {
        collection.AddAspect(field.DeclaringType, iloa);
        if (typeof(ILinkOwner).IsAssignableFrom(field.DeclaringType)) {
          // TODO: Improve code below
          MethodInfo operationsMethod = field.DeclaringType.GetInterfaceMap(typeof(ILinkOwner)).TargetMethods[0];
          collection.AddAspect(operationsMethod, new ImplementLinkOwnerOperationsAspect(iloa));
        }
      }
    }

    public override PostSharpRequirements GetPostSharpRequirements()
    {
      PostSharpRequirements requirements = base.GetPostSharpRequirements();
      requirements.PlugIns.Add("Xtensive.Core.Weaver");
      requirements.Tasks.Add("Xtensive.Core.Weaver.WeaverFactory");
      return requirements;
    }


    // Constructors 
    
    ///<summary>
    ///</summary>
    ///<param name="ownerProperty"></param>
    ///<param name="dependentProperty"></param>
    ///<param name="linkType"></param>
    public LinkAttribute(string ownerProperty, string dependentProperty, LinkType linkType)
    {
      this.ownerProperty = ownerProperty;
      this.dependentProperty = dependentProperty;
      this.linkType = linkType;
    }
  }
}