using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostSharp.Sdk.Extensibility;

namespace Xtensive.Aspects.Weaver.ExtensionBase
{
  public abstract class ValidatorStage
  {
    public abstract bool Validate(Project project);
  }
}
