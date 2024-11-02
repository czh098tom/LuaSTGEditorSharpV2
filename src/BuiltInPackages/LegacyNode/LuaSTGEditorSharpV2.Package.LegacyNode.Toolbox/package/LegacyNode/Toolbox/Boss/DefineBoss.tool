{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Boss/DefineBoss",
	"IconSource": "pack://application:,,,/LuaSTGEditorSharpV2.Package.LegacyNode.Resources.Shared;component/images/nodes/16x16/bossdefine.png",
	"Order": 1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineBoss",
			"Properties": {
				"identifier": "Boss",
				"difficulty": "All",
				"name": "Boss",
				"background": "",
				"bgm": ""
			},
			"Children": [
				{
					"TypeUID": "BossInit",
					"Properties": {
						"position": "240,384",
						"scbg": ""
					}
				},
				{
					"TypeUID": "DefineSpellCard",
					"Properties": {
						"spellcard_name": "\"\"",
						"protect_time": 2,
						"resist_time": 5,
						"total_time": 30,
						"hp": 900,
						"power_amount": 0,
						"faith_amount": 0,
						"point_amount": 0,
						"bomb_immunity": false,
						"opening_performance": false
					},
					"Children": [
						{
							"TypeUID": "SpellCardBeforeStart"
						},
						{
							"TypeUID": "SpellCardStart",
							"Children": [
								{
									"TypeUID": "Task",
									"Properties": {
										"target": "self",
										"redirect_to_self": false
									}
								}
							]
						},
						{
							"TypeUID": "SpellCardBeforeFinish"
						},
						{
							"TypeUID": "SpellCardFinish"
						},
						{
							"TypeUID": "SpellCardAfterFinish"
						}
					]
				}
			]
		}
	],
	"Caption": {
		"Neutral": "Define boss",
		"Localized": {
			"zh": "定义Boss"
		}
	}
}