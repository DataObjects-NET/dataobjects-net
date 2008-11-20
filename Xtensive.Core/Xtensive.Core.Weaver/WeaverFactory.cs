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
    /// <inheritdoc/>
    public LaosAspectWeaver CreateAspectWeaver(ILaosAspect aspect)
    {
      var privateFieldAccessorsAspect = aspect as PrivateFieldAccessorsAspect;
      var autoPropertyReplacementAspect = aspect as AutoPropertyReplacementAspect;
      var constructorEpilogueAspect = aspect as ConstructorEpilogueAspect;
      var reprocessMethodBoundaryAspect = aspect as ReprocessMethodBoundaryAspect;
      var notSupportedMethodAspect = aspect as NotSupportedMethodAspect;
      var declareConstructorAspect = aspect as DeclareConstructorAspect;
      var buildConstructorAspect = aspect as ImplementProtectedConstructorBodyAspect;
      var buildConstructorAccessorAspect = aspect as ImplementProtectedConstructorAccessorAspect;


      // Trying PrivateFieldAccessorsWeaver
      if (privateFieldAccessorsAspect!=null)
        return new PrivateFieldAccessorsWeaver(privateFieldAccessorsAspect.TargetFields);

      // Trying AutoPropertyReplacementWeaver
      if (autoPropertyReplacementAspect!=null)
        return new AutoPropertyReplacementWeaver(
          Project.Module.Cache.GetType(autoPropertyReplacementAspect.HandlerType),
          autoPropertyReplacementAspect.HandlerMethodSuffix);

      // Trying ConstructorEpilogueWeaver
      if (constructorEpilogueAspect!=null)
        return new ConstructorEpilogueWeaver(
          Project.Module.Cache.GetType(constructorEpilogueAspect.HandlerType), constructorEpilogueAspect.HandlerMethodName);

      // Trying DeclareConstructorAspect
      if (declareConstructorAspect!=null)
        return new DeclareProtectedConstructorAspectWeaver(declareConstructorAspect.Aspect.TargetType,
          declareConstructorAspect.Aspect.ParameterTypes.Select(t => Project.Module.Cache.GetType(t)).ToArray());

      // Trying ImplementProtectedConstructorBodyAspect
      if (buildConstructorAspect != null)
        return new ImplementProtectedConstructorBodyWeaver(buildConstructorAspect.Aspect.TargetType,
          buildConstructorAspect.Aspect.ParameterTypes.Select(t => Project.Module.Cache.GetType(t)).ToArray());

      // Trying ProtectedConstructorAccessorWeaver
      if (buildConstructorAccessorAspect != null) {
        return new ProtectedConstructorAccessorWeaver(buildConstructorAccessorAspect.Aspect.TargetType,
          buildConstructorAccessorAspect.Aspect.ParameterTypes.Select(t => Project.Module.Cache.GetType(t)).ToArray());
      }

      if (reprocessMethodBoundaryAspect != null)
        return new ReprocessMethodBoundaryWeaver();

      if (notSupportedMethodAspect != null)
        return new NotSupportedMethodAspectWeaver();

      return null;
    }
  }
}
