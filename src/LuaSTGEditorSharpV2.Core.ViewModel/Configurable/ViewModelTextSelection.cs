using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LuaSTGEditorSharpV2.Core.Model;

namespace LuaSTGEditorSharpV2.Core.ViewModel.Configurable
{
	[Serializable]
	public record class ViewModelTextSelection(string Text, string ConditionOn, bool Inversed)
	{
		public bool ShouldAppend(NodeData source)
		{
			string property = source.GetProperty(ConditionOn, "false");
			bool isFalse = (
				(property.ToLower().Trim() == "false")||
				(String.IsNullOrEmpty(property))
			);
			return (!isFalse && !Inversed) || (isFalse && Inversed);
		}
	}
}
