// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexis Kochetov
// Created:    2009.10.22

using Xtensive.Storage.Model;

namespace Xtensive.Storage.Disconnected.Log
{
  public interface IEntitySetOperation : IEntityOperation
  {
    Key TargetKey { get; }
    FieldInfo FieldInfo { get; }
  }
}