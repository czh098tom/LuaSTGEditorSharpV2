using DynamicData;
using NodeNetwork.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LuaSTGEditorSharpV2.Package.LinqSTG.Windows.ViewModel.Nodes
{
    public class LinqSTGNodeViewModel : NodeViewModel
    {
        public static readonly Color DefaultTitleColor = Color.FromArgb(0xFF, 0x6B, 0xA0, 0xC7);

        private readonly Dictionary<string, NodeEndpointEditorViewModel> editorDict = [];
        private readonly Dictionary<string, NodeInputViewModel> inputDict = [];
        private readonly Dictionary<string, NodeOutputViewModel> outputDict = [];

        public IReadOnlyDictionary<string, NodeEndpointEditorViewModel> EditorDict => editorDict;
        public IReadOnlyDictionary<string, NodeInputViewModel> InputDict => inputDict;
        public IReadOnlyDictionary<string, NodeOutputViewModel> OutputDict => outputDict;

        public Color TitleColor { get; protected init; } = DefaultTitleColor;

        public virtual string NodeType
        {
            get
            {
                var name = GetType().Name;
                return name.EndsWith("Node", StringComparison.Ordinal) ? name[..^4] : name;
            }
        }

        public void AddOutput(string name, NodeOutputViewModel output)
        {
            if (output is null)
            {
                throw new ArgumentNullException(nameof(output), "Output cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Output name cannot be null or whitespace.", nameof(name));
            }
            outputDict.Add(name, output);
            Outputs.Add(output);
        }

        public void AddInput(string name, NodeInputViewModel input)
        {
            if (input is null)
            {
                throw new ArgumentNullException(nameof(input), "Input cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Input name cannot be null or whitespace.", nameof(name));
            }
            inputDict.Add(name, input);
            Inputs.Add(input);
        }

        public void AddEditor(string name, NodeEndpointEditorViewModel editor)
        {
            if (editor is null)
            {
                throw new ArgumentNullException(nameof(editor), "Editor cannot be null.");
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Editor name cannot be null or whitespace.", nameof(name));
            }
            editorDict.Add(name, editor);
        }
    }
}
