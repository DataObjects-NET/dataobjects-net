// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2010.04.07

using PostSharp.Sdk.AspectWeaver;

// ReSharper disable UnusedMember.Global

namespace Xtensive.Aspects.Weaver
{
  public sealed class PlugIn : AspectWeaverPlugIn
  {
    protected override void Initialize()
    {
      if (!new AspectValidator(Project).IsValid)
        return;

      BindAspectWeaver<ReplaceAutoProperty, ReplaceAutoPropertyWeaver>();
      BindAspectWeaver<ImplementConstructorEpilogue, ConstructorEpilogueWeaver>();
      BindAspectWeaver<NotSupportedAttribute, NotSupportedWeaver>();
      BindAspectWeaver<ImplementConstructor, ImplementConstructorWeaver>();
      BindAspectWeaver<ImplementFactoryMethod, ImplementFactoryMethodWeaver>();
    }


    // Constructors

    public PlugIn()
      : base(StandardPriorities.User)
    {
    }
   
  }
}