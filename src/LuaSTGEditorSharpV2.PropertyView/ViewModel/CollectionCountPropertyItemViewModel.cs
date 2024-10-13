using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.DependencyInjection;

using CommunityToolkit.Mvvm.Input;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.PropertyView.ViewModel
{
    public class CollectionCountPropertyItemViewModel : BasicPropertyItemViewModel
    {
        private readonly ICommand _increase;
        public ICommand Increase => _increase;

        private readonly ICommand _decrease;
        public ICommand Decrease => _decrease;

        public CollectionCountPropertyItemViewModel(NodeData nodeData, LocalServiceParam localServiceParam,
            string? key, ViewModelProviderServiceProvider viewModelProviderServiceProvider)
            : base(nodeData, localServiceParam, key, viewModelProviderServiceProvider)
        {
            _increase = new RelayCommand(() =>
            {
                if (int.TryParse(Value, out var count))
                {
                    Value = (count + 1).ToString();
                }
            });
            _decrease = new RelayCommand(() =>
            {
                if (int.TryParse(Value, out var count))
                {
                    Value = (count - 1).ToString();
                }
            });
        }
    }

    [Inject(ServiceLifetime.Singleton)]
    public class CollectionCountPropertyItemViewModelFactory(ViewModelProviderServiceProvider viewModelProviderServiceProvider) 
        : IBasicPropertyItemViewModelFactory<CollectionCountPropertyItemViewModel>
    {
        public CollectionCountPropertyItemViewModel Create(NodeData nodeData, LocalServiceParam localServiceParam, string? key)
        {
            return new CollectionCountPropertyItemViewModel(nodeData, localServiceParam, key, viewModelProviderServiceProvider);
        }
    }
}
