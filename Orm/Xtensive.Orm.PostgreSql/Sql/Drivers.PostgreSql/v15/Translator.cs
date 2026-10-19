// Copyright (C) 2026 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

using System;
using Xtensive.Sql.Compiler;
using Xtensive.Sql.Ddl;
using Xtensive.Sql.Model;

namespace Xtensive.Sql.Drivers.PostgreSql.v15
{
  internal class Translator : v14.Translator
  {
    public override void Translate(IOutput output, UniqueNullTreatment nullValuesTreatment)
    {
      // explicitly declare unique null treatment
      _ = output.AppendSpaceIfNecessary()
        .Append(TryResolveUnspecifiedNullTreatment(nullValuesTreatment) switch {
          UniqueNullTreatment.Distinct => "NULLS DISTINCT",
          UniqueNullTreatment.NotDistinct => "NULLS NOT DISTINCT",
          _ => throw new NotSupportedException() // should be impossible
        });
    }

    private UniqueNullTreatment TryResolveUnspecifiedNullTreatment(UniqueNullTreatment nullValuesTreatment)
    {
      return nullValuesTreatment switch {
        UniqueNullTreatment.Unspecified when Driver.ServerInfo.Index.NullDistinctByDefault == true => UniqueNullTreatment.Distinct,
        UniqueNullTreatment.Unspecified when Driver.ServerInfo.Index.NullDistinctByDefault == false => UniqueNullTreatment.NotDistinct,
        UniqueNullTreatment.Unspecified when !Driver.ServerInfo.Index.NullDistinctByDefault.HasValue => throw new NotSupportedException(),
        _ => nullValuesTreatment
      };
    }


    // Constructors

    public Translator(SqlDriver driver)
      : base(driver)
    {
    }
  }
}