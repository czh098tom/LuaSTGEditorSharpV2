using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.Core;
using LuaSTGEditorSharpV2.Core.Settings;
using LuaSTGEditorSharpV2.Core.Services;
using LuaSTGEditorSharpV2.Core.Command.Factory;
using LuaSTGEditorSharpV2.Core.Editor;

namespace LuaSTGEditorSharpV2.Services
{
    [Inject(ServiceLifetime.Singleton)]
    public class ActiveDocumentService(ILogger<ActiveDocumentService> logger, LocalizationService localizationService, EditorNodeFactory editorNodeFactory) 
        : ISettingsProvider
    {
        private readonly ILogger<ActiveDocumentService> _logger = logger;

        private ActiveDocumentServiceSettings _settings = new();
        public object Settings
        {
            get => _settings;
            set => _settings = (value as ActiveDocumentServiceSettings) ?? _settings;
        }

        private readonly List<EditorDocument> _activeDocuments = [];
        public IReadOnlyList<EditorDocument> ActiveDocuments => _activeDocuments;

        private readonly List<EditorDocument?> _unsavedUntitledDocuments = [];

        public void RefreshSettings() { }

        public EditorDocument? Open(string path)
        {
            try
            {
                var plainDoc = DocumentModel.CreateFromFile(path);
                if (plainDoc == null) return null;
                var doc = new EditorDocument(plainDoc, editorNodeFactory);
                _activeDocuments.Add(doc);
                return doc;
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                _logger.LogError("Open documnent from {path} failed", path);
            }
            return null;
        }

        public EditorDocument CreateBlank()
        {
            var idx = FindUnoccupiedUntitledFileIndex();
            var plainDoc = DocumentModel.CreateEmpty(GetUntitledFileName(idx));
            var doc = new EditorDocument(plainDoc, editorNodeFactory);
            _activeDocuments.Add(doc);
            InsertUntitledFile(doc, idx);
            return doc;
        }

        public void MarkAsSaved(EditorDocument document)
        {
            var idx = _unsavedUntitledDocuments.FindIndex(m => m == document);
            if (idx < 0) return;
            _unsavedUntitledDocuments[idx] = null;
        }

        public void Close(EditorDocument document)
        {
            _activeDocuments.Remove(document);
        }

        private void InsertUntitledFile(EditorDocument document, int index)
        {
            if (index >= _unsavedUntitledDocuments.Count)
            {
                _unsavedUntitledDocuments.Add(document);
            }
            else
            {
                _unsavedUntitledDocuments[index] = document;
            }
        }

        private string GetUntitledFileName(int index)
        {
            if (string.IsNullOrEmpty(_settings.CustomizedUntitledName))
            {
                var str = localizationService.GetString("untitled_file_default_name", typeof(ActiveDocumentService).Assembly);
                return $"{str} {index}";
            }
            else
            {
                return $"{_settings.CustomizedUntitledName} {index}";
            }
        }

        private int FindUnoccupiedUntitledFileIndex()
        {
            for (int i = 0; i < _unsavedUntitledDocuments.Count; i++)
            {
                if (_unsavedUntitledDocuments[i] == null)
                {
                    return i;
                }
            }
            return _unsavedUntitledDocuments.Count;
        }
    }
}
