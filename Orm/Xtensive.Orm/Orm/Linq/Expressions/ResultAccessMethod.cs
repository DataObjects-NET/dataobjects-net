// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.04.24


namespace Xtensive.Orm.Linq.Expressions
{
  [Serializable]
  internal enum ResultAccessMethod
  {
    All,
    First,
    FirstOrDefault,
    Single,
    SingleOrDefault
  }
}