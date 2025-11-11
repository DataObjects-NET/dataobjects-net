// Copyright (C) 2022-2025 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2022.06.14

using System;


namespace Xtensive.Orm.Internals
{
  /// <summary>
  /// Base type for <see cref="SessionBound"/> registries.
  /// </summary>
  public abstract class SessionBoundRegistry : SessionBound
  {
    private bool registrationsDisabled;

    internal Core.Disposable DisableRegistrations()
    {
      if (registrationsDisabled)
        return null; // already disabled
      registrationsDisabled = true;
      return new Core.Disposable((disposing) => registrationsDisabled = false);
    }

    protected void EnsureRegistrationsAllowed()
    {
      if (changesDisabled) {
        throw new InvalidOperationException(
          string.Format(Strings.ExPersistentChangesAreNotAllowedForThisEvent, Session.Guid));
      }
    }

    public SessionBoundRegistry(Session session)
      : base(session)
    {
      registrationsDisabled = false;
    }
  }
}