// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using System;
using System.Diagnostics;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected
{
  public interface IUpdateEntityOperation : IEntityOperation
  {
    FieldInfo FieldInfo { get; }
    object Value { get; }
  }
}