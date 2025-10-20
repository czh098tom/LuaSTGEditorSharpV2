using Stubble.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core.CodeGenerator.Configurable.Mustache
{
    public static class CodeGenerationContextExtension
    {
        public static StringBuilder RenderIndentedTemplate(this CodeGenerationContext context, string template, MustacheCodeGenerationHash captureResult)
		{
			return context.ApplyIndented(context.GetIndented(), StaticStubbleRenderer.Render(template, captureResult));
		}
    }
}
