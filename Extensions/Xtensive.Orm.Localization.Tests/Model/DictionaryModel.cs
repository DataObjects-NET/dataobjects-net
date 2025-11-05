// Copyright (C) 2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System.Globalization;
using Xtensive.Orm.Model;
using Xtensive.Orm.Upgrade;

namespace Xtensive.Orm.Localization.Tests.CustomTypeModel
{
  [HierarchyRoot(InheritanceSchema = InheritanceSchema.ConcreteTable)]
  public abstract class AbstractDictionary : Entity
  {
    [Key, Field]
    public int Id { get; private set; }

    [Field(Nullable = false, Length = 64)]
    public string Identifier { get; set; }

    // abstract property
    public abstract string Name { get; set; }

    protected AbstractDictionary(Session session) : base(session) { }
  }

  public abstract class AbstractLocalizableDictionary<T, TT> : AbstractDictionary, ILocalizable<TT>
      where T : AbstractLocalizableDictionary<T, TT>
      where TT : AbstractDictionaryLocalization<T, TT>
  {
    public override string Name { get => Localizations.Current.Name; set => Localizations.Current.Name = value; }

    [Field]
    public LocalizationSet<TT> Localizations { get; private set; }

    protected AbstractLocalizableDictionary(Session session) : base(session) { }
  }

  public abstract class AbstractDictionaryLocalization<T, TT> : Localization<T>
    where TT : AbstractDictionaryLocalization<T, TT>
    where T : AbstractLocalizableDictionary<T, TT>
  {
    [Field(Nullable = false, Length = 512)]
    public string Name { get; set; }

    protected AbstractDictionaryLocalization(Session session, CultureInfo culture, T target) : base(session, culture, target) { }
  }

  public class Country : AbstractLocalizableDictionary<Country, CountryLocalization>, ILocalizable<CountryLocalization>
  {
    public Country(Session session)
      : base(session)
    {
    }
  }

  [HierarchyRoot]
  public class CountryLocalization : AbstractDictionaryLocalization<Country, CountryLocalization>
  {
    public CountryLocalization(Session session, CultureInfo culture, Country target)
      : base(session, culture, target)
    {
    }
  }

  namespace Upgrade
  {
    public class CustomUpgradeHandler : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      public override void OnBeforeExecuteActions(UpgradeActionSequence actions) => base.OnBeforeExecuteActions(actions);
    }
  }
}
