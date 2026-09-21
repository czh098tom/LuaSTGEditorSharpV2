using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public record RepeaterKey(string ID, string Total)
    {
        public static RepeaterKey Default { get; } = new("ID", "Total");

        public Repeater GetRepeater(Parameter dict)
        {
            var id = dict.Floats.GetValueOrDefault(ID, 0f);
            var total = dict.Floats.GetValueOrDefault(Total, 0f);
            return new Repeater(NumericConversion.ToInt32(id), NumericConversion.ToInt32(total));
        }
    }
}
