namespace OpenTaiko;

/// <summary>
///		Integer values indicating the type of chip.
/// </summary>
/// <remarks>
///		For avoiding confusions, do not cast number literals to this type.
///		Instead, add a hex-suffixed member and use it, so that its usage can be easily located.
/// </remarks>
public enum EChipType : int {
	Error = -1,

	// 0x0X: system
	Unknown = 0x00,
	Bgm = 0x01,
	MeasureChange = 0x02,
	BpmInfoChange = 0x03, // actual change for DTX legacy, info for TJA
	BgaLayer1 = 0x04, // DTX legacy
	BgaExtendObj = 0x05, // DTX legacy, unimplemented
	BgaMiss = 0x06, // DTX legacy, unimplemented
	BgaLayer2 = 0x07, // DTX legacy

	BpmChangeEx = 0x08, // normal change in TJA
	NMScroll = 0x09,
	BMScroll = 0x0A,
	HBScroll = 0x0B,

	// 0x1X: Drums (DTX legacy), Taiko
	T0Blank = 0x10,
	T1DonReg = 0x11,
	T2KaReg = 0x12,
	T3DonBig = 0x13,
	T4KaBig = 0x14,
	T5RollReg = 0x15,
	T6RollBig = 0x16,
	T7BalloonReg = 0x17,

	T8EndRoll = 0x18,
	T9BalloonEx = 0x19,
	TADonBigHand = 0x1A,
	TBKaBigHand = 0x1B,
	TCMine = 0x1C,
	TDBalloonFuze = 0x1D,

	TFAdlib = 0x1F,

	// 0x2X: Guitar (DTX legacy), Taiko
	THClapRoll = 0x20,
	TILeftRoll = 0x21,

	// 0x3X: Drums invisible (DTX legacy)

	// 0x5X: system
	BarLine = 0x50,
	BeatLine = 0x51,
	BranchEnd = 0x52,

	BgaOn = 0x54,
	BgaOff = 0x55,
	Sys56 = 0x56,
	Sys57 = 0x57,

	Sys58 = 0x58,
	Sys59 = 0x59,
	Sys5A = 0x5A,
	Sys5B = 0x5B,
	Sys5C = 0x5C,
	Sys5D = 0x5D,
	Sys5E = 0x5E,
	Sys5F = 0x5F,

	// 0x6X - 0x7X: SE (DTX legacy), system (TJAP2fPC legacy)
	ScrollChangeOld = 0x60,
	DelayOld = 0x61,
	GogoStartOld = 0x62,
	GogoEndOld = 0x63,
	CamVMoveStartOld = 0x64,
	CamVMoveEndOld = 0x65,
	CamHMoveStartOld = 0x66,
	CamHMoveEndOld = 0x67,
	CamZoomStartOld = 0x68,
	CamZoomEndOld = 0x69,
	CamRotationStartOld = 0x6A,
	CamRotationEndOld = 0x6B,
	CamHScaleStartOld = 0x6C,
	CamHScaleEndOld = 0x6D,
	CamVScaleStartOld = 0x6E,
	CamVScaleEndOld = 0x6F,

	BorderColorChangeOld = 0x70,

	Se79 = 0x79,

	// 0x8X - 0x9X: SE (DTX legacy)
	Se80 = 0x80,

	Se89 = 0x89,

	Se90 = 0x90,

	Se92 = 0x92,

	// 0x9X: SE (DTX legacy), Taiko (TJAP2fPC legacy), system
	T1DonRegOld = 0x93,
	T2KaRegOld = 0x94,
	T3DonBigOld = 0x95,
	T4KaBigOld = 0x96,
	T5RollRegOld = 0x97,
	T6RollBigOld = 0x98,
	T7BalloonRegOld = 0x99,
	T8EndRollOld = 0x9A,
	NextSongStart = 0x9B,
	AnimeBpmChange = 0x9C,
	ScrollChange = 0x9D,
	GogoStart = 0x9E,
	GogoEnd = 0x9F,

	// 0xAX: Bass (DTX legacy), system (TJAP3-Extended)
	CamVMoveStart = 0xA0,
	CamVMoveEnd = 0xA1,
	CamHMoveStart = 0xA2,
	CamHMoveEnd = 0xA3,
	CamZoomStart = 0xA4,
	CamZoomEnd = 0xA5,
	CamRotationStart = 0xA6,
	CamRotationEnd = 0xA7,

	CamVScaleStart = 0xA8,
	CamVScaleEnd = 0xA9,

	TFAdlibOld = 0xAF,

	// 0xBX: player's empty hits (DTX legacy), system (TJAP3-Extended)
	CamHScaleStart = 0xB0,
	CamHScaleEnd = 0xB1,
	BorderColorChange = 0xB2,
	CamHOffsetChange = 0xB3,
	CamVOffsetChange = 0xB4,
	CamZoomChange = 0xB5,
	CamRotationChange = 0xB6,
	CamHScaleChange = 0xB7,

	CamVScaleChange = 0xB8,
	CamReset = 0xB9,
	EnableDoron = 0xBA,
	DisableDoron = 0xBB,
	AddObject = 0xBC,
	RemoveObject = 0xBD,
	ObjVMoveStart = 0xBE,
	ObjVMoveEnd = 0xBF,

	// 0xCX: system (TJAP3-Extended)
	ObjHMoveStart = 0xC0,
	ObjHMoveEnd = 0xC1,
	ObjVScaleStart = 0xC2,
	BeatBarLineVisibleChange = 0xC2, // DTX legacy
	ObjVScaleEnd = 0xC3,
	ObjHScaleStart = 0xC4,
	ObjHScaleEnd = 0xC5,
	ObjRotationStart = 0xC6,
	ObjRotationEnd = 0xC7,

	ObjOpacityStart = 0xC8,
	ObjOpacityEnd = 0xC9,
	ObjColorChange = 0xCA,
	ObjYChange = 0xCB,
	ObjXChange = 0xCC,
	ObjVScaleChange = 0xCD,
	ObjHScaleChange = 0xCE,
	ObjRotationChange = 0xCF,

	// 0xDX - 0xFX: system
	ObjOpacityChange = 0xD0,
	ChangeTexture = 0xD1,
	ResetTexture = 0xD2,
	SetConfig = 0xD3,
	ObjAnimStart = 0xD4,
	ObjAnimStartLoop = 0xD5,
	ObjAnimEnd = 0xD6,
	BgaChange = 0xD6, // DTX legacy (unused)
	ObjFrameChange = 0xD7,

	GameTypeChange = 0xD8,
	SplitLaneChange = 0xD9,
	AddSound = 0xDA,
	RemoveSound = 0xDB,
	Delay = 0xDC,
	BranchSectionReset = 0xDD,
	BranchVisual = 0xDE,
	BranchInternal = 0xDF,

	BarLineVisibleChange = 0xE0,
	BranchLevelHold = 0xE1,
	JPosScrollStart = 0xE2,
	SplitLaneReset = 0xE3,
	BarLineDummy = 0xE4,

	SysF0 = 0xF0,
	LyricChange = 0xF1,
	DirectionChange = 0xF2,
	SuddenChange = 0xF3,

	SysFE = 0xFE,
	EndChart = 0xFF,

	// 0x10X: Taiko, extra notes
	TGKadonOld = 0x100,
	TGKadon = 0x101,
}
