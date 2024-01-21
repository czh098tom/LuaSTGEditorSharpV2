using LuaSTGEditorSharpV2.PropertyView;
using LuaSTGEditorSharpV2.Toolbox.ViewModel;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;

namespace LuaSTGEditorSharpV2.Util
{
    public class LayoutUpdateStrategy : ILayoutUpdateStrategy
    {
        public void AfterInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableShown)
        {
        }

        public void AfterInsertDocument(LayoutRoot layout, LayoutDocument documentShown)
        {
        }

        public bool BeforeInsertAnchorable(LayoutRoot layout, LayoutAnchorable anchorableToShow, ILayoutContainer destinationContainer)
        {
            var type = anchorableToShow?.Content?.GetType();
            if (type == typeof(PropertyPageViewModel))
            {
                var group = new LayoutAnchorGroup();
                group.Children.Add(anchorableToShow);
                layout.RightSide.Children.Add(group);
                return true;
            }
            else if (type == typeof(ToolboxPageViewModel))
            {
                var group = new LayoutAnchorGroup();
                group.Children.Add(anchorableToShow);
                layout.LeftSide.Children.Add(group);
                return true;
            }
            else if (type == typeof(Debugging.ViewModel.DebugOutputPageViewModel)
                || type == typeof(Analyzer.ViewModel.ErrorListPageViewModel))
            {
                LayoutAnchorGroup layoutAnchorGroup;
                if (layout.BottomSide.ChildrenCount > 0)
                {
                    layoutAnchorGroup = layout.BottomSide.Children[0];
                }
                else
                {
                    layoutAnchorGroup = new LayoutAnchorGroup();
                    layout.BottomSide.Children.Add(layoutAnchorGroup);
                }
                layoutAnchorGroup.Children.Add(anchorableToShow);
                return true;
            }
            return false;
        }

        public bool BeforeInsertDocument(LayoutRoot layout, LayoutDocument documentToShow, ILayoutContainer destinationContainer)
        {
            return false;
        }
    }
}
