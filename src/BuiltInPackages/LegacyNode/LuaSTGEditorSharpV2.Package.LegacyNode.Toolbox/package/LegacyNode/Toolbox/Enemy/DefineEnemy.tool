{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Enemy/DefineEnemy",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/enemydefine.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineEnemy",
			"Properties": {
				"identifier": "\"\"",
				"difficulty": "\"All\""
			},
			"Children": [
				{
					"TypeUID": "EnemyInit",
					"Properties": {
						"parameters": "",
						"style": 9,
						"hp": 100,
						"power_amount": 0,
						"faith_amount": 0,
						"point_amount": 0,
						"protect_time": 30,
						"clear_bullet_when_killed": true,
						"bound": true,
						"collision_damage": true
					}
				}
			]
		}
	],
	"Caption": {
		"Neutral": "Define enemy",
		"Localized": {
			"zh": "定义敌机"
		}
	}
}