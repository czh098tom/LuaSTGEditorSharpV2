using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using Microsoft.Extensions.Logging;
using LuaSTGEditorSharpV2.Core;

namespace LuaSTGEditorSharpV2.Services
{
    public class ActiveDocumentService(ILogger<ActiveDocumentService> logger)
    {
        private readonly ILogger<ActiveDocumentService> _logger = logger;

        private readonly List<EditingDocumentModel> _activeDocuments = [];
        public IReadOnlyList<EditingDocumentModel> ActiveDocuments => _activeDocuments;

        public EditingDocumentModel? Open(string path)
        {
            try
            {
                var plainDoc = DocumentModel.CreateFromFile(path);
                if (plainDoc == null) return null;
                var doc = new EditingDocumentModel(plainDoc);
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
    }
}
