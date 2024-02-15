using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaSTGEditorSharpV2.Core
{
    internal class ServiceInfo
    {
        public required string Name { get; init; }
        public required string ShortName { get; init; }
        public required Type ServiceProviderType { get; init; }
        public required Type ServiceInstanceType { get; init; }
        public required Delegate RegisterFunction { get; init; }
        public required string ServiceInstancePrimaryKeyName { get; init; }

        public Type? SettingsType { get; init; }
        public Delegate? SettingsReplacementFunction { get; init; }

        [MemberNotNullWhen(true, nameof(SettingsType))]
        [MemberNotNullWhen(true, nameof(SettingsReplacementFunction))]
        public bool HasSettings => SettingsType != null && SettingsReplacementFunction != null;
    }
}
