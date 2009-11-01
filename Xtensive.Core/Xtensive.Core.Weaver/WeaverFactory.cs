// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.10.23

using PostSharp.Extensibility;
using PostSharp.Laos;
using PostSharp.Laos.Weaver;
using Xtensive.Core.Aspects;
using Xtensive.Core.Aspects.Internals;

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
      ImplementPrivateFieldAccessorAspect fieldAccessorAspect = aspect as ImplementPrivateFieldAccessorAspect;
      if (fieldAccessorAspect != null)
        return new ImplementPrivateFieldAccessorWeaver(fieldAccessorAspect.Fields);
      
      return null;
    }
  }

}
