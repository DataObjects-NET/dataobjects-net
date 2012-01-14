// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.24

namespace Xtensive.Orm
{
  /// <summary>
  /// Precondition operation contract. Preconditions change nothing,
  /// but ensure the upcoming operations can be performed safely.
  /// </summary>
  public interface IPrecondition : IOperation 
  {
  }
}