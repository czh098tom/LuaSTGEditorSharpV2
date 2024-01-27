using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Core.Settings;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace LuaSTGEditorSharpV2.Services
{
    public class FileDialogService : ISettingsProvider, ISettingsSavedOnClose
    {
        private static OpenFileDialog? ShowOpenFileDialog(string filter, Property<string> filePath)
        {
            var dialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                Filter = filter,
                Multiselect = true,
                InitialDirectory = filePath
            };
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                filePath.Setter(dialog.FileNames.FirstOrDefault(string.Empty));
                return dialog;
            }
            return null;
        }

        private FileDialogServiceSettings _settings = new();
        public object Settings
        {
            get => _settings;
            set => _settings = (value as FileDialogServiceSettings) ?? _settings;
        }

        public void RefreshSettings() { }

        public void SaveSettings()
        {
            HostedApplicationHelper.GetService<SettingsService>().SaveSettings(this);
        }

        public IReadOnlyList<string> ShowOpenFileCommandDialog()
        {
            var local = HostedApplicationHelper.GetService<LocalizationService>();
            if (ShowOpenFileDialog(local.GetString("openFileCommandDialog_fileExtension", typeof(FileDialogService).Assembly), 
                new Property<string>(() => _settings.OpenFilePath, v => _settings.OpenFilePath = v)) 
                is OpenFileDialog dialog)
            {
                return dialog.FileNames;
            }
            return [];
        }
    }
}
