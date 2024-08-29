﻿// Copyright (C) 2007-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Dmitri Maximov
// Created:    2007.08.03

namespace Xtensive.Orm.Configuration.Options
{
  /// <summary>
  /// Defines options where name is key of collection
  /// </summary>
  internal interface INamedOptionsCollectionElement
  {
    string Name { get; set; }
  }
}
