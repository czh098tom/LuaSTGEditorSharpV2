{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Object/DefineObject",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/objectdefine.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineObject",
			"Properties": {
				"identifier": "",
				"difficulty": "All"
			},
			"Children": [
				{
					"TypeUID": "ObjectInit",
					"Properties": {
						"parameters": "",
						"image": "leaf",
						"layer": "LAYER_ENEMY_BULLET",
						"group": "GROUP_ENEMY_BULLET",
						"hide": false,
						"bound": true,
						"navigate": false,
						"hp": 10,
						"collision": true
					}
				}
			]
		}
	],
	"Caption": {
		"Neutral": "Define object",
		"Localized": {
			"zh": "定义物体类型"
		}
	}
}