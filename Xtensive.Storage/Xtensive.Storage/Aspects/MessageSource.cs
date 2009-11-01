// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.06.11

using System.Resources;
using PostSharp.Extensibility;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Aspects
{
  internal static class AspectsMessageSource
  {
    public static readonly MessageSource Instance = new MessageSource("Xtensive.Storage", Strings.ResourceManager);
  }
}