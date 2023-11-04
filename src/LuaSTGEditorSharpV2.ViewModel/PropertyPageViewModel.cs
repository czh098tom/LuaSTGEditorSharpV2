using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.ViewModel
{
    public class PropertyPageViewModel : BaseViewModel
    {
        public NodeData? Source { get; private set; }

        public ObservableCollection<PropertyViewModel> Properties { get; set; } = new();

        public void LoadProperties(IReadOnlyList<PropertyViewModel> viewModels, NodeData source)
        {
            Source = source;
            Properties.Clear();
            for (int i = 0; i < viewModels.Count; i++)
            {
                Properties.Add(viewModels[i]);
            }
        }
    }
}
