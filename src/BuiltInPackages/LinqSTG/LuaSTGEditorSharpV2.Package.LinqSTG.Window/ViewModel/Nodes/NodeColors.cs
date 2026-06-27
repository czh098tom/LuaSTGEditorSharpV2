using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public static class NodeColors
    {
        public static readonly Color Shoot = Color.FromArgb(0xFF, 0xA0, 0x6B, 0xC7);
        public static readonly Color Data = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
        public static readonly Color Movement = Color.FromArgb(0xFF, 0x1C, 0x1C, 0xDB);
        public static readonly Color Pattern = Color.FromArgb(0xFF, 0xDB, 0x1C, 0x1C);
        public static readonly Color Transformation = Color.FromArgb(0xFF, 0x1C, 0xDB, 0x1C);
        public static readonly Color PatternOperator = Color.FromArgb(0xFF, 0xDB, 0x81, 0x1C);
        public static readonly Color Operator = Color.FromArgb(0xFF, 0x6B, 0xA0, 0xC7);
    }
}
