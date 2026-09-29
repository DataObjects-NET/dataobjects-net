// Copyright (C) 2008-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alex Yakunin
// Created:    2008.11.07

using System;
using System.Collections.Generic;
using Xtensive.Orm.Validation;

namespace Xtensive.Orm
{
  public partial class Session 
  {
    /// <summary>
    /// Gets the current validation context.
    /// </summary>
    /// <exception cref="InvalidOperationException">Can not get validation context: There is no active transaction.</exception>
    internal ValidationContext ValidationContext { get; private set; }

    /// <summary>
    /// Validates all instances registered in <see cref="ValidationContext"/>
    /// of current <see cref="Session"/>.
    /// </summary>
    public void Validate() => ValidationContext.Validate(ValidationReason.UserRequest);

    /// <summary>
    /// Validates all registered entities similar to <see cref="Validate"/> method
    /// and returns all validation errors.
    /// </summary>
    /// <returns>List errors occurred during validation.</returns>
    public IList<EntityErrorInfo> ValidateAndGetErrors() => ValidationContext.ValidateAndGetErrors();
  }
}