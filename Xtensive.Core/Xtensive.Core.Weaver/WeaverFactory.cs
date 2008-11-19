// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.23

using System.Linq;
using PostSharp.Extensibility;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects.Helpers;
using Xtensive.Core.Aspects.Helpers.Internals;

namespace Xtensive.Core.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Core.Weaver' plug-in.
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
      var privateFieldAccessorsAspect = aspect as PrivateFieldAccessorsAspect;
      var autoPropertyReplacementAspect = aspect as AutoPropertyReplacementAspect;
      var constructorEpilogueAspect = aspect as ConstructorEpilogueAspect;
      var protectedConstructorAccessorAspect = aspect as ProtectedConstructorAccessorAspect;
      var reprocessMethodBoundaryAspect = aspect as ReprocessMethodBoundaryAspect;
      var notSupportedMethodAspect = aspect as NotSupportedMethodAspect;
      var declareConstructorAspect = aspect as DeclareConstructorAspect;
      var buildConstructorAspect = aspect as ImplementProtectedConstructorBodyAspect;


      // Trying ImplementPrivateFieldAccessorsWeaver
      if (privateFieldAccessorsAspect!=null)
        return new ImplementPrivateFieldAccessorsWeaver(privateFieldAccessorsAspect.TargetFields);

      // Trying ImplementAutoPropertyReplacementWeaver
      if (autoPropertyReplacementAspect!=null)
        return new ImplementAutoPropertyReplacementWeaver(
          Project.Module.Cache.GetType(autoPropertyReplacementAspect.HandlerType),
          autoPropertyReplacementAspect.HandlerMethodSuffix);

      // Trying ImplementConstructorEpilogueWeaver
      if (constructorEpilogueAspect!=null)
        return new ImplementConstructorEpilogueWeaver(
          Project.Module.Cache.GetType(constructorEpilogueAspect.HandlerType), constructorEpilogueAspect.HandlerMethodName);

      // Trying DeclareConstructorAspect
      if (declareConstructorAspect!=null)
        return new DeclareConstructorAspectWeaver(declareConstructorAspect.ProtectedConstructorAspect.TargetType,
          declareConstructorAspect.ProtectedConstructorAspect.ParameterTypes.Select(t => Project.Module.Cache.GetType(t)).ToArray());

      // Trying ImplementProtectedConstructorBodyAspect
      if (buildConstructorAspect != null)
        return new BuildConstructorAspectWeaver(buildConstructorAspect.ProtectedConstructorAspect.TargetType,
          buildConstructorAspect.ProtectedConstructorAspect.ParameterTypes.Select(t => Project.Module.Cache.GetType(t)).ToArray());

      // Trying ImplementProtectedConstructorAccessorWeaver
      if (protectedConstructorAccessorAspect!=null) {
        return new ImplementProtectedConstructorAccessorWeaver(
          protectedConstructorAccessorAspect.ParameterTypes
            .Select(t => Project.Module.Cache.GetType(t)).ToArray());
      }

      if (reprocessMethodBoundaryAspect != null)
        return new ReprocessMethodBoundaryAspectWeaver();

      if (notSupportedMethodAspect != null)
        return new NotSupportedMethodAspectWeaver();

      return null;
    }
  }
}
