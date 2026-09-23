using System.Drawing;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace OpenTaiko;

[Serializable]
internal class CSongListNodeInheritable {
	// Properties

	// Colors are re-derived from the parent node on load, so they are not JSON-cached.
	public Color? ForeColor; // default: Color.White; Color for song title text fill
	public Color? BackColor; // default: Color.Black; Color for song title text outline
	public Color? BoxColor; // default: Color.White; Color for song entry panel
	public Color? BgColor; // default: Color.White; Color for genre text
	public string? BoxType;
	public string? BgType;
	public string? BoxChara;
	public CTja.ETjaCompat? Compat;
	public string? strSelectBGPath;
	public string? Preimage;

	// Metadata
	public string? songGenre;
	public string? songGenrePanel; // Used only for the panel under the song title

	// In-game visuals
	public string? strSkinPath;         // Removable?
	public string? strScenePresets; // includes commas

	private static readonly CSongListNodeInheritable InheritanceRoot = new() {
		ForeColor = Color.White,
		BackColor = Color.Black,
		BoxColor = Color.White,
		BgColor = Color.White,
		songGenre = "",
		songGenrePanel = "",
		strSkinPath = "",
	};

	public void InheritFromRoot() => InheritFrom(InheritanceRoot);

	public void InheritFrom(CSongListNodeInheritable? parent) {
		if (parent == null)
			return;
		this.ForeColor ??= parent.ForeColor;
		this.BackColor ??= parent.BackColor;
		this.BoxColor ??= parent.BoxColor;
		this.BgColor ??= parent.BgColor;
		this.BgType ??= parent.BgType;
		this.BoxType ??= parent.BoxType;
		this.BoxChara ??= parent.BoxChara;
		this.Compat ??= parent.Compat;
		this.strSelectBGPath ??= parent.strSelectBGPath;
		this.Preimage ??= parent.Preimage;
		this.songGenre ??= parent.songGenre;
		this.songGenrePanel ??= parent.songGenrePanel;
		this.strSkinPath ??= parent.strSkinPath;
		this.strScenePresets ??= parent.strScenePresets;
	}
}

internal class CSongListNode : CSongListNodeInheritable {
	[JsonIgnore] public CSongListNodeInheritable inherited;

	// Properties

	public ENodeType nodeType = ENodeType.UNKNOWN;
	public enum ENodeType {
		SCORE,
		SCORE_MIDI,
		BOX,
		BACKBOX,
		RANDOM,
		UNKNOWN
	}
	[JsonIgnore]  // runtime counter, reassigned by the constructor on load
	public int nID { get; private set; }
	public CScore[] score = new CScore[(int)Difficulty.Total];

	public string[] difficultyLabel = new string[(int)Difficulty.Total];

	[JsonIgnore] public List<CSongListNode> randomList;     // tree links: rebuilt by enumeration, not cached
	[JsonIgnore] public List<CSongListNode> childrenList;

	public int difficultiesCount; // 4~5 if AD

	[JsonIgnore] public CSongListNode rParentNode;     // re-established from the enumeration context on load

	// Internal
	public int Openindex;
	public bool bIsOpenFolder;
	public string strBreadcrumbs = "";      // Removable?

	// Metadata
	public CLocalizationData ldTitle = new CLocalizationData();
	public CLocalizationData ldSubtitle = new CLocalizationData();
	public string strMaker = "";
	public string[] strNotesDesigner = new string[(int)Difficulty.Total] { "", "", "", "", "", "", "" };
	public CTja.ESide nSide = CTja.ESide.eEx;
	public bool bExplicit = false;
	public bool bMovie = false;
	public int[] nLevel = new int[(int)Difficulty.Total] { 0, 0, 0, 0, 0, 0, 0 };
	// The LEVEL value with its fractional part kept (e.g. 12.888); parallels nLevel. -1 = no decimal known,
	// in which case consumers fall back to nLevel. Only the .tja path carries a real fraction; .tci/.tcm use the int.
	public double[] dLevel = new double[(int)Difficulty.Total] { -1, -1, -1, -1, -1, -1, -1 };
	public CTja.ELevelIcon[] nLevelIcon = new CTja.ELevelIcon[(int)Difficulty.Total] { CTja.ELevelIcon.eNone, CTja.ELevelIcon.eNone, CTja.ELevelIcon.eNone, CTja.ELevelIcon.eNone, CTja.ELevelIcon.eNone, CTja.ELevelIcon.eNone, CTja.ELevelIcon.eNone };

	// Custom metadata handlers
	public Dictionary<string, string> customMetadataGScope = new Dictionary<string, string>();
	public Dictionary<string, string>[] customMetadataCScope = Enumerable.Range(0, (int)Difficulty.Total).Select(_ => new Dictionary<string, string>()).ToArray();

	// Branches
	public bool bBranch = false;

	// Dan
	public List<CTja.DanSongs> DanSongs;
	public Dan_C[] Dan_C;

	// Tower Lives
	public int nLife = 5;
	public int nTotalFloor = 140;
	public string nTowerType;

	// Unique id
	public CSongUniqueID uniqueId;
	public List<string> shortcutIds = [];
	public bool shortcutIsParsed;

	public int nDanTick = 0;
	[JsonIgnore] public Color cDanTickColor = Color.White;

	public CLocalizationData[] strBoxText = new CLocalizationData[3] { new CLocalizationData(), new CLocalizationData(), new CLocalizationData() };

	// In-game visuals

	#region [ OpenTaiko-Exclusive TJA Extension Data ]

	public CTja.CutSceneDef? CutSceneIntro = null;
	public List<CTja.CutSceneDef> CutSceneOutros = [];

	#endregion

	public string tGetUniqueId() {
		return uniqueId?.data.id ?? "";
	}

	public void SetParent(CSongListNode? parent) {
		if (parent == null)
			return;
		this.rParentNode = parent;
		this.inherited.InheritFrom(parent.inherited);
		if (this.score[0] != null && parent.score[0] != null && string.IsNullOrEmpty(this.score[0].ChartInfo.Preimage))
			this.score[0].ChartInfo.Preimage = parent.score[0].ChartInfo.Preimage;
	}

	// Constructor

	public CSongListNode() {
		// Increment atomically because the song enumeration can build nodes concurrently.
		this.nID = Interlocked.Increment(ref lastAssignedID);
	}

	public CSongListNode(CBoxDef boxDef) : this() {
		this.InheritFrom(boxDef);
	}

	public CSongListNode Clone() {
		return (CSongListNode)MemberwiseClone();
	}

	public override bool Equals(object other) {
		if (other.GetType() == typeof(CSongListNode)) {
			CSongListNode obj = (CSongListNode)other;
			return this.nID == obj.nID;
		}
		return this.GetHashCode() == other.GetHashCode();
	}

	public override int GetHashCode() {
		return base.GetHashCode();
	}


	// その他

	/// <summary>
	/// Restores fields that have default initializers after BinaryFormatter deserialization,
	/// which bypasses constructors and field initializers for fields added in newer versions.
	/// </summary>
	[OnDeserialized]
	private void OnDeserialized(StreamingContext context) {
		customMetadataGScope ??= new Dictionary<string, string>();
		customMetadataCScope ??= Enumerable.Range(0, (int)Difficulty.Total).Select(_ => new Dictionary<string, string>()).ToArray();
	}

	#region [ private ]
	//-----------------
	private static int lastAssignedID = -1;
	//-----------------
	#endregion
}
