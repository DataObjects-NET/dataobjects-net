// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.27

using System.Resources;
using PostSharp.Extensibility;
using Xtensive.Core.Weaver.Resources;

namespace Xtensive.Core.Weaver
{
  internal static class WeaverMessageSource
  {
    public static readonly MessageSource Instance = new MessageSource("Xtensive.Core.Weaver", Strings.ResourceManager);
  }
}