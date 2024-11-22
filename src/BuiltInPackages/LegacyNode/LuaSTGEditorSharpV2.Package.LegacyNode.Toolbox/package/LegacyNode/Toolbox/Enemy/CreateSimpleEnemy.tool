{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Enemy/CreateSimpleEnemy",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/enemysimple.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "CreateSimpleEnemy",
			"Properties": {
				"style": 1,
				"hp": 10,
				"position": "self.x, self.y",
				"power_amount": 0,
				"faith_amount": 0,
				"point_amount": 0,
				"protect_time": 15,
				"clear_bullet_when_killed": false,
				"bound": true,
				"collision_damage": true
			}
		}
	],
	"Caption": {
		"Neutral": "Create simple bullet",
		"Localized": {
			"zh": "创建简单敌机"
		}
	}
}