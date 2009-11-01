// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.23

using System;
using System.Collections.Generic;
using PostSharp.CodeModel;
using PostSharp.CodeModel.Helpers;
using PostSharp.Extensibility;
using PostSharp.Extensibility.Tasks;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Storage.Aspects;

namespace Xtensive.Storage.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Storage.Weaver' plug-in.
  /// </summary>
  public class WeaverFactory : Task, ILaosAspectWeaverFactory
  {
    /// <summary>
    /// Called by PostSharp Laos to get the weaver of a given aspect.
    /// If the current plug-in does not know this aspect, it should return <b>null</b>.
    /// </summary>
    /// <param name="aspect">The aspect requiring a weaver.</param>
    /// <returns>A weaver (<see cref="LaosAspectWeaver"/>), or <b>null</b> if the <paramref name="aspect"/>
    /// is not recognized by the current factory.</returns>
    public LaosAspectWeaver CreateAspectWeaver(ILaosAspect aspect)
    {
      ImplementConstructorAspect constructorAspect = aspect as ImplementConstructorAspect;
      ImplementConstructorEpilogueAspect constructorEpilogueAspect = aspect as ImplementConstructorEpilogueAspect;
      ImplementAutoPropertyAspect autoPropertyAspect = aspect as ImplementAutoPropertyAspect;
      ConstructorDelegateAspect constructorDelegateAspect = aspect as ConstructorDelegateAspect;

      ITypeSignature[] parameterTypeSignatures = null;
      if (constructorAspect != null || constructorDelegateAspect != null) {
        Type[] parameterTypes = constructorAspect != null
                                  ? constructorAspect.ParameterTypes
                                  : constructorDelegateAspect.ParameterTypes;
        parameterTypeSignatures = new ITypeSignature[parameterTypes.Length];
        for (int i = 0; i < parameterTypes.Length; i++)
          parameterTypeSignatures[i] = Project.Module.Cache.GetType(parameterTypes[i]);
      }

      if (constructorAspect != null)
        return new ImplementConstructorWeaver(
          Project.Module.Cache.GetType(constructorAspect.BaseType),
          parameterTypeSignatures
          );

      if (constructorEpilogueAspect != null)
        return new ImplementConstructorEpilogueWeaver(
          Project.Module.Cache.GetType(constructorEpilogueAspect.BaseType));
      
      if (autoPropertyAspect != null)
        return new ImplementAutoPropertyWeaver(
          Project.Module.Cache.GetType(autoPropertyAspect.BaseType));

      if (constructorDelegateAspect != null)
        return new ConstructorDelegateWeaver(
          Project.Module.Cache.GetType(constructorDelegateAspect.BaseType),
          parameterTypeSignatures,
          Project.Module.Cache.GetType(constructorDelegateAspect.DelegateType, 
            BindingOptions.RequireGenericInstance));
      
      return null;
    }
  }
}
