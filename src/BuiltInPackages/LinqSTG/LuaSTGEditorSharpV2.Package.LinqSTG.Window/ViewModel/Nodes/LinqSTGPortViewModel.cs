using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public class LinqSTGPortViewModel(Color portColor) : PortViewModel()
    {
        public Color PortColor { get; protected init; } = portColor;
    }
}
