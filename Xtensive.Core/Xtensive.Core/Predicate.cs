// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.24

using System;

namespace Xtensive.Core
{
  // public delegate bool Predicate(); // Disabled, because leads to confusion with other Predicates

  // public delegate bool Predicate<A>(A a); // Exists in System namespace

  public delegate bool Predicate<A1, A2>(A1 a1, A2 a2);

  public delegate bool Predicate<A1, A2, A3>(A1 a1, A2 a2, A3 a3);

  public delegate bool Predicate<A1, A2, A3, A4>(A1 a1, A2 a2, A3 a3, A4 a4);
}