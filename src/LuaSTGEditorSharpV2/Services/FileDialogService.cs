using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Microsoft.Extensions.DependencyInjection;

using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Core.Settings;

using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;

namespace LuaSTGEditorSharpV2.Services
{
    [Inject(ServiceLifetime.Singleton)]
    public class FileDialogService(SettingsService settingsService, LocalizationService localization) 
        : ISettingsProvider, ISettingsSavedOnClose
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
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath.Setter(Path.GetDirectoryName(dialog.FileNames.FirstOrDefault(string.Empty)) ?? string.Empty);
                return dialog;
            }
            return null;
        }

        private static SaveFileDialog? ShowSaveFileDialog(string fileName, string filter, Property<string> filePath)
        {
            var dialog = new SaveFileDialog()
            {
                CheckPathExists = true,
                FileName = fileName,
                Filter = filter,
                InitialDirectory = filePath
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath.Setter(Path.GetDirectoryName(dialog.FileName) ?? string.Empty);
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
            settingsService.SaveSettings(this);
        }

        public IReadOnlyList<string> ShowOpenFileCommandDialog()
        {
            if (ShowOpenFileDialog(localization.GetString("fileDialog_openFileExtension", typeof(FileDialogService).Assembly), 
                new Property<string>(() => _settings.OpenFilePath, v => _settings.OpenFilePath = v)) 
                is OpenFileDialog dialog)
            {
                return dialog.FileNames;
            }
            return [];
        }

        public string? ShowSaveAsFileCommandDialog(string fileName)
        {
            if (ShowSaveFileDialog(fileName,
                localization.GetString("fileDialog_saveFileExtension", typeof(FileDialogService).Assembly),
                new Property<string>(() => _settings.SaveFilePath, v => _settings.SaveFilePath = v))
                is SaveFileDialog dialog)
            {
                return dialog.FileName;
            }
            return null;
        }
    }
}
