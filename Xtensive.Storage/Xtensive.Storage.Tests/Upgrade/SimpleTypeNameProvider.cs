// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.30

using System;
using System.Diagnostics;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Tests.Upgrade
{
  [Serializable]
  public class SimpleTypeNameProvider : ITypeNameProvider
  {
    public string GetTypeName(Type type)
    {
      return type.GetShortName();
    }
  }
}