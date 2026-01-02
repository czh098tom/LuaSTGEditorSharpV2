using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

using Newtonsoft.Json;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.NodeProfile.WPF.ViewModel
{
    public class NodeProfilePageViewModel : AnchorableViewModelBase
    {
        private static readonly string BRICK = "pack://application:,,,/LuaSTGEditorSharpV2.Resources.Shared;component/images/editor/brick.png";
        private static readonly string BRICKS = "pack://application:,,,/LuaSTGEditorSharpV2.Resources.Shared;component/images/editor/bricks.png";

        private static readonly JsonSerializerSettings SETTINGS = new()
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        };

        public override string I18NTitleKey => "panel_nodeProfile_title";

        public ObservableCollection<TreeNodeViewModel> Tree { get; } = [];

        public TreeNodeViewModel? Current
        {
            get => _current;
            set
            {
                _current = value;
                Content = value?.Content;
            }
        }
        private TreeNodeViewModel? _current;

        public ContentViewModel? Content
        {
            get => _content;
            set
            {
                _content = value;
                RaisePropertyChanged();
            }
        }
        private ContentViewModel? _content;

        public NodeProfilePageViewModel(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            Refresh();
        }

        public void Refresh()
        {
            var gen = ServiceProvider.GetRequiredService<NodeProfileGenerator>();
            var profile = gen.CreateProfile();
            foreach (var p in profile)
            {
                var vmNode = new TreeNodeViewModel(BRICKS, p.Name);
                foreach (var s in p.Profiles)
                {
                    var vmService = new TreeNodeViewModel(BRICK, s.Name, new(JsonConvert.SerializeObject(s.Data, SETTINGS)));
                    vmNode.Children.Add(vmService);
                }
                Tree.Add(vmNode);
            }
        }
    }
}
