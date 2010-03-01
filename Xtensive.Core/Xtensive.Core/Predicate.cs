// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.24

using System;

namespace Xtensive.Core
{
  // public delegate bool Predicate(); // Disabled, because leads to confusion with other Predicates

  // public delegate bool Predicate<T>(T a); // Exists in System namespace

  public delegate bool Predicate<T1, T2>(T1 a1, T2 a2);

  public delegate bool Predicate<T1, T2, T3>(T1 a1, T2 a2, T3 a3);

  public delegate bool Predicate<T1, T2, T3, T4>(T1 a1, T2 a2, T3 a3, T4 a4);
}