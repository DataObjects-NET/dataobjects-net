// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.01.23

using System.Resources;
using PostSharp.Extensibility;
using Xtensive.Aspects.Resources;

namespace Xtensive.Aspects
{
  internal sealed class AspectsMessageDispenser : IMessageDispenser
  {
    private readonly ResourceManager strings;

    public string GetMessage(string key)
    {
      return strings.GetString(key);
    }


    // Constructors

    public AspectsMessageDispenser()
    {
      strings = Strings.ResourceManager;
    }
  }
}