// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using Xtensive.Core.Aspects;

namespace Xtensive.Core.Weaver
{
  /// <summary>
  /// Creates the weavers defined by the 'Xtensive.Core.Weaver' plug-in.
  /// </summary>
  public class PlugIn : PostSharp.AspectWeaver.PlugIn
  {
    public PlugIn()
      : base(Priorities.User)
    {
    }

    protected override void Initialize()
    {
      AddAspectWeaverFactory<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
      AddAspectWeaverFactory<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
      AddAspectWeaverFactory<NotSupportedAttribute, NotSupportedWeaver>();
      AddAspectWeaverFactory<ImplementConstructor, ImplementConstructorWeaver>();
      AddAspectWeaverFactory<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
    }
  }
}
