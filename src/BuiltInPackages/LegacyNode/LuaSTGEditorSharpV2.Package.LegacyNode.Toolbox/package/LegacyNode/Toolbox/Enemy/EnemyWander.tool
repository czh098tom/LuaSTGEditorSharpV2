{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Enemy/EnemyWander",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/taskbosswander.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "EnemyWander",
			"Properties": {
				"duration": 60,
				"range": "-96, 96, 112, 144",
				"amplitude": "16, 32, 8, 16",
				"move_mode": "MOVE_DECEL",
				"direction_mode": "MOVE_X_TOWARDS_PLAYER"
			}
		}
	],
	"Caption": {
		"Neutral": "Wander",
		"Localized": {
			"zh": "随机移动"
		}
	}
}