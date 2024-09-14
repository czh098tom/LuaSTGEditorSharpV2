{
	"$type": "LuaSTGEditorSharpV2.Toolbox.Model.SimpleToolboxItem, LuaSTGEditorSharpV2.Toolbox, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
	"Path": "Boss/DefineSpellCard",
	"Order":1,
	"NodeTemplate": [
		{
			"TypeUID": "DefineSpellCard",
			"Properties": {
				"spellcard_name": "",
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
							"TypeUID": "AttachTask",
							"Properties": {
								"target": "self"
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
	],
	"Caption": {
		"Neutral": "Define spellcard",
		"Localized": {
			"zh": "定义符卡"
		}
	}
}