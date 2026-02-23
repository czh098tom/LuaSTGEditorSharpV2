using CommunityToolkit.Mvvm.Input;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Net.Mime.MediaTypeNames;

namespace LuaSTGEditorSharpV2.Debugging.ViewModel
{
    public class DebugOutputPageViewModel : AnchorableViewModelBase
    {
        public override string I18NTitleKey => "panel_output_title";

        public string? LastBuildingAppendedText
        {
            get => _lastBuildingAppendedText;
            set
            {
                _lastBuildingAppendedText = value;
                RaisePropertyChanged();
            }
        }
        private string? _lastBuildingAppendedText = null;

        public string? LastDebugAppendedText
        {
            get => _lastDebugAppendedText;
            set
            {
                _lastDebugAppendedText = value;
                RaisePropertyChanged();
            }
        }
        private string? _lastDebugAppendedText = null;

        public int SelectedOutputSourceIndex
        {
            get => _selectedOutputSourceIndex;
            set
            {
                _selectedOutputSourceIndex = value;
                SetSelectedOutputSource(value);
                RaisePropertyChanged();
            }
        }
        private int _selectedOutputSourceIndex = 0;

        public bool BuildingTextIsVisible
        {
            get => _buildingTextIsVisible;
            set
            {
                _buildingTextIsVisible = value;
                RaisePropertyChanged();
            }
        }
        private bool _buildingTextIsVisible = true;

        public bool DebugTextIsVisible
        {
            get => _debugTextIsVisible;
            set
            {
                _debugTextIsVisible = value;
                RaisePropertyChanged();
            }
        }
        private bool _debugTextIsVisible = false;

        public EventArgs ClearStream
        {
            get => _clearStream;
            set
            {
                _clearStream = value;
                RaisePropertyChanged();
            }
        }
        private EventArgs _clearStream = EventArgs.Empty;

        public ObservableCollection<string> OutputSourceNames { get; } = [];

        public ICommand ClearOutputCommand
        {
            get => _clearOutputCommand;
            set
            {
                _clearOutputCommand = value;
                RaisePropertyChanged();
            }
        }
        private ICommand _clearOutputCommand;

        public IOutputLogWriter OutputLogWriter => _outputLogWriter;
        private readonly IOutputLogWriter _outputLogWriter;

        public DebugOutputPageViewModel(IServiceProvider serviceProvider, LocalizationService localization) : base(serviceProvider)
        {
            OutputSourceNames.Add(localization.GetString("panel_output_source_box_build", typeof(DebugOutputPageViewModel).Assembly));
            OutputSourceNames.Add(localization.GetString("panel_output_source_box_debug", typeof(DebugOutputPageViewModel).Assembly));
            _clearOutputCommand = new RelayCommand(() =>
            {
                ClearStream = new();
            });
            _outputLogWriter = new OutputLogWriterImpl(this);
        }

        public void SetSelectedOutput(string name)
        {
            if (name == "build")
            {
                SelectedOutputSourceIndex = 0;
            }
            else if (name == "debug")
            {
                SelectedOutputSourceIndex = 1;
            }
        }

        private void SetSelectedOutputSource(int index)
        {
            BuildingTextIsVisible = index == 0;
            DebugTextIsVisible = index == 1;
        }

        public void AppendBuildOutput(string text)
        {
            LastBuildingAppendedText = text;
        }

        public void AppendDebugOutput(string text)
        {
            LastDebugAppendedText = text;
        }

        private class OutputLogWriterImpl(DebugOutputPageViewModel viewModel) : IOutputLogWriter
        {
            public void WriteLine(string target, string text)
            {
                if (target == "build")
                {
                    viewModel.AppendBuildOutput(text);
                    viewModel.AppendBuildOutput("\n");
                }
                else if (target == "debug")
                {
                    viewModel.AppendDebugOutput(text);
                    viewModel.AppendBuildOutput("\n");
                }
            }
        }
    }
}
