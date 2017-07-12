﻿// Copyright (C) 2017 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kulakov
// Created:    2017.06.12


namespace Xtensive.Orm.Upgrade
{
  public interface IFulltextCatalogResolver
  {
    bool IsEnabled { get; }

    string Resolve();
  }
}
