// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.01.06

namespace Xtensive.Integrity.Tests
{
  public interface ISessionBound
  {
    Session Session { get; }
  }
}