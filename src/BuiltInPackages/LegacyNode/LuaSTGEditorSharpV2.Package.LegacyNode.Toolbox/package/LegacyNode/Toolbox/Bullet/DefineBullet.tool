{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Bullet/DefineBullet",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/bulletdefine.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineBullet",
			"Properties": {
				"identifier": "",
				"difficulty": "All"
			},
			"Children": [
				{
					"TypeUID": "BulletInit",
					"Properties": {
						"parameters": "",
						"style": "grain_a",
						"color": "COLOR_RED",
						"stay": true,
						"destroyable": true
					}
				}
			]
		}
	],
	"Caption": {
		"Neutral": "Define bullet",
		"Localized": {
			"zh": "定义子弹"
		}
	}
}