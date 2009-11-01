// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.27

using System.Resources;
using PostSharp.Extensibility;
using Xtensive.Core.Aspects.Resources;

namespace Xtensive.Core.Aspects
{
  internal static class AspectsMessageSource
  {
    public static readonly MessageSource Instance = new MessageSource("Xtensive.Core.Aspects", Strings.ResourceManager);
  }
}