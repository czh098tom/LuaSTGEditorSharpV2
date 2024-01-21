using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Services
{
    public class MainWindowLayoutSettings
    {
        //[JsonProperty]
        //public string LayoutXML { get; set; } = """
        //    <?xml version="1.0" encoding="utf-8"?>
        //    <LayoutRoot>
        //      <RootPanel Orientation="Horizontal">
        //        <LayoutPanel Orientation="Horizontal" DockWidth="0.6492713666797951*">
        //          <LayoutPanel Orientation="Horizontal">
        //            <LayoutAnchorablePaneGroup Orientation="Horizontal" DockWidth="0.3887662496934019*" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" FloatingLeft="543.4285714285714" FloatingTop="461.71428571428567">
        //              <LayoutAnchorablePane DockWidth="0.19436786136274126*" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" FloatingLeft="543.4285714285714" FloatingTop="461.71428571428567">
        //                <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="工具箱" IsSelected="True" ContentId="LuaSTGEditorSharpV2.Toolbox.ViewModel.ToolboxPageViewModel, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingLeft="543.4285714285714" FloatingTop="461.71428571428567" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" CanClose="False" LastActivationTimeStamp="01/21/2024 16:00:28" PreviousContainerId="b2e68f84-1319-4c5a-b830-d628a9e97a9e" PreviousContainerIndex="0" />
        //              </LayoutAnchorablePane>
        //            </LayoutAnchorablePaneGroup>
        //            <LayoutPanel Orientation="Vertical" DockWidth="1.611233750306598*">
        //              <LayoutDocumentPaneGroup Orientation="Horizontal" DockHeight="1.5506964690638159*">
        //                <LayoutDocumentPane>
        //                  <LayoutDocument Title="test.lstgxml" IsSelected="True" IsLastFocusedDocument="True" CanClose="True" LastActivationTimeStamp="01/21/2024 16:00:34" />
        //                </LayoutDocumentPane>
        //              </LayoutDocumentPaneGroup>
        //              <LayoutAnchorablePaneGroup Orientation="Horizontal" DockHeight="0.4493035309361841*" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" FloatingLeft="905.7142857142857" FloatingTop="522.2857142857142">
        //                <LayoutAnchorablePane DockWidth="0.19436786136274126*" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" FloatingLeft="905.7142857142857" FloatingTop="522.2857142857142">
        //                  <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="错误列表" IsSelected="True" ContentId="LuaSTGEditorSharpV2.Analyzer.ViewModel.ErrorListPageViewModel, LuaSTGEditorSharpV2.Analyzer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingLeft="905.7142857142857" FloatingTop="522.2857142857142" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" CanClose="False" LastActivationTimeStamp="01/21/2024 16:00:37" PreviousContainerId="b2e68f84-1319-4c5a-b830-d628a9e97a9e" PreviousContainerIndex="1" />
        //                  <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="输出" ContentId="LuaSTGEditorSharpV2.Debugging.ViewModel.DebugOutputPageViewModel, LuaSTGEditorSharpV2.Debugging, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingLeft="884" FloatingTop="606.8571428571428" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" CanClose="False" LastActivationTimeStamp="01/21/2024 16:00:36" PreviousContainerId="b2e68f84-1319-4c5a-b830-d628a9e97a9e" PreviousContainerIndex="1" />
        //                </LayoutAnchorablePane>
        //              </LayoutAnchorablePaneGroup>
        //            </LayoutPanel>
        //          </LayoutPanel>
        //        </LayoutPanel>
        //        <LayoutAnchorablePane Id="b2e68f84-1319-4c5a-b830-d628a9e97a9e" DockWidth="0.3507286333202049*" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" FloatingLeft="731.4285714285713">
        //          <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" Title="属性" IsSelected="True" ContentId="LuaSTGEditorSharpV2.PropertyView.PropertyPageViewModel, LuaSTGEditorSharpV2.PropertyView, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingLeft="731.4285714285713" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" CanClose="False" LastActivationTimeStamp="01/21/2024 16:00:35" />
        //        </LayoutAnchorablePane>
        //      </RootPanel>
        //      <TopSide />
        //      <RightSide />
        //      <LeftSide />
        //      <BottomSide />
        //      <FloatingWindows >
        //      <FloatingWindows />
        //      <Hidden />
        //    </LayoutRoot>
        //    """;

        [JsonProperty]
        public string LayoutXML { get; set; } = """
            <?xml version="1.0" encoding="utf-8"?>
            <LayoutRoot>
            </LayoutRoot>
            """;
    }
}
