// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.07.02

namespace Xtensive.Storage.Model
{
  public enum Multiplicity
  {
    ZeroToOne = 1,

    ZeroToMany = 2,

    OneToOne = 3,

    OneToMany = 4,

    ManyToOne = 5,

    ManyToMany = 6,
  }
}