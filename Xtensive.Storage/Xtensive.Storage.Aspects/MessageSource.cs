// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.09.27

using System.Resources;
using PostSharp.Extensibility;
using Xtensive.Storage.Aspects.Resources;

namespace Xtensive.Storage.Aspects
{
  internal static class AspectsMessageSource
  {
    public static readonly MessageSource Instance = new MessageSource("Xtensive.Storage.Aspects", Strings.ResourceManager);
  }
}