﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuaSTGEditorSharpV2.Core.Model;
using LuaSTGEditorSharpV2.ViewModel;

namespace LuaSTGEditorSharpV2.Core.Command
{
    public class EditPropertyCommand : CommandBase
    {
        public static CommandBase? CreateEditCommandOnDemand(NodeData node, string? propertyName, string afterEdit)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }
            else
            {
                if (node.HasProperty(propertyName))
                {
                    return new EditPropertyCommand(node, propertyName, afterEdit);
                }
                else
                {
                    return new AddPropertyCommand(node, propertyName, afterEdit);
                }
            }
        }

        public NodeData Node { get; private set; }
        public string PropertyName { get; private set; }
        public string AfterEdit { get; private set; }

        string? _beforeEdit;

        public EditPropertyCommand(NodeData node, string propertyName, string afterEdit)
        {
            Node = node;
            PropertyName = propertyName;
            AfterEdit = afterEdit;
        }

        protected override void DoExecute(LocalServiceParam param)
        {
            _beforeEdit = Node.Properties[PropertyName];
            Node.Properties[PropertyName] = AfterEdit;
            HostedApplicationHelper
                .GetService<ViewModelProviderServiceProvider>()
                .UpdateViewModelDataRecursive(Node, param);
        }

        protected override void RevertExecution(LocalServiceParam param)
        {
            if (_beforeEdit == null) throw new InvalidOperationException("Command has not been executed yet.");
            Node.Properties[PropertyName] = _beforeEdit;
            HostedApplicationHelper
                .GetService<ViewModelProviderServiceProvider>()
                .UpdateViewModelDataRecursive(Node, param);
        }
    }
}
