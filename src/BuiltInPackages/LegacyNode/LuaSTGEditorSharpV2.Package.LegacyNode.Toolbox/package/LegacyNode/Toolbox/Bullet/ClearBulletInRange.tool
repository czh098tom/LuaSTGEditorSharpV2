{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Bullet/ClearBulletInRange",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/bulletcleanrange.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "ClearBulletInRange",
			"Properties": {
				"position": "self.x, self.y",
				"radius": 64,
				"expanding_time": 30,
				"clearing_time": 30,
				"convert_to_faith": true,
				"include_indestructible": false,
				"vertical_veloctiy": "0"
			}
		}
	],
	"Caption": {
		"Neutral": "Clear bullet in range",
		"Localized": {
			"zh": "清除范围内的子弹"
		}
	}
}