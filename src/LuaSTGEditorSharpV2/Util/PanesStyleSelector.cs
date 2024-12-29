using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Util
{
    class PanesStyleSelector : StyleSelector
    {
        public Style? ToolStyle
        {
            get;
            set;
        }

        public Style? FileStyle
        {
            get;
            set;
        }

        public override Style? SelectStyle(object item, DependencyObject container)
        {
            if (item is AnchorableViewModelBase)
                return ToolStyle;

            if (item is DockingViewModelBase)
                return FileStyle;

            return base.SelectStyle(item, container);
        }
    }
}
