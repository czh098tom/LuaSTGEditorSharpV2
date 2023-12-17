using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace LuaSTGEditorSharpV2.Core.Configurable
{
    public class Selection
    {
        [JsonProperty] public string Text { get; private set; } = string.Empty;
        [JsonProperty] public int? ConditionOn { get; private set; }
        [JsonProperty] public bool Inversed { get; private set; } = false;
        [JsonProperty] public bool DefaultValue { get; private set; } = true;
        [JsonProperty] public bool UseExistanceInsteadOfBoolValue { get; private set; } = false;

        public bool ShouldAppend(string?[] captureResult)
        {
            if (ConditionOn == null)
            {
                return !Inversed;
            }
            bool isTrue;
            if (!UseExistanceInsteadOfBoolValue)
            {
                string captured = DefaultValue.ToString();
                if (ConditionOn.Value < captureResult.Length && ConditionOn.Value > 0)
                {
                    captured = captureResult[ConditionOn.Value] ?? captured;
                }
                isTrue = captured.ToLower().Trim() == "true";
            }
            else
            {
                string captured = string.Empty;
                if (ConditionOn.Value < captureResult.Length && ConditionOn.Value > 0)
                {
                    captured = captureResult[ConditionOn.Value] ?? captured;
                }
                isTrue = !string.IsNullOrWhiteSpace(captured);
            }
            return isTrue ^ Inversed;
        }
    }
}
