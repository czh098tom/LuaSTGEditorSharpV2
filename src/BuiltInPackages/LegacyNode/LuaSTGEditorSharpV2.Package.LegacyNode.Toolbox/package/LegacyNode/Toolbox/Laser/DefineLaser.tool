{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Laser/DefineLaser",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/lasercreate.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineLaser",
			"Properties": {
				"identifier": "\"\"",
				"difficulty": "\"All\""
			},
			"Children": [
				{
					"TypeUID": "LaserInit",
					"Properties": {
						"parameters": "",
						"color": "COLOR_RED",
						"style": 1,
						"head_length": 32,
						"body_length": 32,
						"tail_length": 32,
						"width": 8,
						"node_size": 0,
						"head_size": 0
					}
				}
			]
		}
	],
	"Caption": {
		"Neutral": "Define laser",
		"Localized": {
			"zh": "定义激光"
		}
	}
}