using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using Newtonsoft.Json;

using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.ServiceBridge.CodeGenerator.ViewModel
{
    [DisplayName("")]
    public class CodeGenerationServiceSettingsViewModel : BaseViewModel
    {
        [JsonProperty("indention_string")]
        private string _indentionString = string.Empty;
        public string IndentionString 
        {
            get => _indentionString;
            set
            {
                _indentionString = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty("indent_on_blank_line")]
        private bool _indentOnBlankLine;
        public bool IndentOnBlankLine 
        {
            get => _indentOnBlankLine;
            set
            {
                _indentOnBlankLine = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty("skip_blank_line")]
        private bool _skipBlankLine;
        public bool SkipBlankLine 
        {
            get => _skipBlankLine;
            set
            {
                _skipBlankLine = value;
                RaisePropertyChanged();
            }
        }

        [JsonProperty("line_obfuscated")]
        private bool _lineObfuscated;
        public bool LineObfuscated 
        {
            get => _lineObfuscated;
            set
            {
                _lineObfuscated = value;
                RaisePropertyChanged();
            } 
        }
    }
}
