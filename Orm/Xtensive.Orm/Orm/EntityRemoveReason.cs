// Copyright (C) 2018-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Kudelin
// Created:    2018.10.12

using System.Text;

namespace Xtensive.Orm
{
  /// <summary>
  /// Entity remove reason
  /// </summary>
  public enum EntityRemoveReason
  {
    /// <summary>
    /// Remove caused by other reasons
    /// </summary>
    Other = 0,

    /// <summary>
    /// Remove caused by user code
    /// </summary>
    User = 1,

    /// <summary>
    /// Remove caused by association
    /// </summary>
    Association = 2
  }
}