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
        [JsonProperty]
        public string LayoutXML { get; set; } = """
            <?xml version="1.0" encoding="utf-8"?>
            <LayoutRoot>
              <RootPanel Orientation="Horizontal">
                <LayoutAnchorablePane DockWidth="0.3446238676644347*" DockMinWidth="100" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428">
                  <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" IsSelected="True" ContentId="LuaSTGEditorSharpV2.Toolbox.ViewModel.ToolboxPageViewModel, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" CanClose="False" LastActivationTimeStamp="01/23/2024 22:04:50" />
                </LayoutAnchorablePane>
                <LayoutPanel Orientation="Vertical" DockWidth="1.6553761323355651*">
                  <LayoutPanel Orientation="Horizontal" DockHeight="1.4516092151146174*">
                    <LayoutPanel Orientation="Vertical" DockWidth="1.549057054189544*">
                      <LayoutDocumentPaneGroup Orientation="Horizontal">
                        <LayoutDocumentPane>
                          <LayoutDocument IsSelected="True" IsLastFocusedDocument="True" CanClose="True" LastActivationTimeStamp="01/23/2024 22:04:50" />
                        </LayoutDocumentPane>
                      </LayoutDocumentPaneGroup>
                    </LayoutPanel>
                  </LayoutPanel>
                  <LayoutAnchorablePane DockHeight="0.5483907848853826*">
                    <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" IsSelected="True" ContentId="LuaSTGEditorSharpV2.Analyzer.ViewModel.ErrorListPageViewModel, LuaSTGEditorSharpV2.Analyzer, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingLeft="732.5714285714286" FloatingTop="597.1428571428571" FloatingWidth="1464.5714285714284" FloatingHeight="208.57142857142856" CanClose="False" LastActivationTimeStamp="01/23/2024 22:04:53" />
                    <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" ContentId="LuaSTGEditorSharpV2.Debugging.ViewModel.DebugOutputPageViewModel, LuaSTGEditorSharpV2.Debugging, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" CanClose="False" LastActivationTimeStamp="01/23/2024 22:04:50" />
                  </LayoutAnchorablePane>
                </LayoutPanel>
                <LayoutAnchorablePane DockWidth="0.4509429458104558*" DockMinWidth="100" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" FloatingLeft="731.4285714285713">
                  <LayoutAnchorable AutoHideMinWidth="100" AutoHideMinHeight="100" IsSelected="True" ContentId="LuaSTGEditorSharpV2.PropertyView.PropertyPageViewModel, LuaSTGEditorSharpV2.PropertyView, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" FloatingLeft="731.4285714285713" FloatingWidth="731.4285714285713" FloatingHeight="774.8571428571428" CanClose="False" LastActivationTimeStamp="01/23/2024 22:04:50" />
                </LayoutAnchorablePane>
              </RootPanel>
              <TopSide />
              <RightSide />
              <LeftSide />
              <BottomSide />
              <FloatingWindows />
              <Hidden />
            </LayoutRoot>
            """;
    }
}
