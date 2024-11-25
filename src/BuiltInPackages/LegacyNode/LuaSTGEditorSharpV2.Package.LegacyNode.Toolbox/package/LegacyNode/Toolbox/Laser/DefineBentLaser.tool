{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Laser/DefineBentLaser",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/laserbentdefine.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineBentLaser",
			"Properties": {
				"identifier": "\"\"",
				"difficulty": "\"All\""
			},
			"Children": [
				{
					"TypeUID": "BentLaserInit",
					"Properties": {
						"parameters": "",
						"color": "COLOR_RED",
						"length": 32,
						"width": 8,
						"sampling": 4,
						"node_size": 0
					}
				}
			]
		}
	],
	"Caption": {
		"Neutral": "Define bentlaser",
		"Localized": {
			"zh": "定义曲光"
		}
	}
}