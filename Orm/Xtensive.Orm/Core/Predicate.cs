// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2007.05.24

namespace Xtensive.Core
{
  // public delegate bool Predicate(); // Disabled, because leads to confusion with other Predicates

  // public delegate bool Predicate<T>(T a); // Exists in System namespace

  /// <summary>
  /// A delegate returning boolean and accepting two arguments.
  /// </summary>
  public delegate bool Predicate<T1, T2>(T1 a1, T2 a2);
}