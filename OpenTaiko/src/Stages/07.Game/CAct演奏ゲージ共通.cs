using System.Diagnostics;
using System.Numerics;
using FDK;

namespace OpenTaiko;

/// <summary>
/// CAct演奏Drumsゲージ と CAct演奏Gutiarゲージ のbaseクラス。ダメージ計算やDanger/Failed判断もこのクラスで行う。
///
/// 課題
/// _STAGE FAILED OFF時にゲージ回復を止める
/// _黒→閉店までの差を大きくする。
/// </summary>
internal class CAct演奏ゲージ共通 : CActivity {
	// Properties
	public CActLVLNFont actLVLNFont { get; protected set; }

	// コンストラクタ
	public CAct演奏ゲージ共通() {
		//actLVLNFont = new CActLVLNFont();		// On活性化()に移動
		//actLVLNFont.On活性化();
	}

	// CActivity 実装

	public override void Activate() {
		for (int i = 0; i < 3; i++) {
			for (int n = 0; n < 3; n++) {
				dbゲージ増加量_Branch[i, n] = new float[5];
			}
		}
		for (int i = 0; i < this.DTX.Length; ++i)
			this.DTX[i] = OpenTaiko.GetTJA(i)!;
		actLVLNFont = new CActLVLNFont();
		actLVLNFont.Activate();
		base.Activate();
	}
	public override void DeActivate() {
		actLVLNFont.DeActivate();
		actLVLNFont = null;
		base.DeActivate();
	}

	const double GAUGE_MAX = 100.0;
	const double GAUGE_INITIAL = 2.0 / 3;
	const double GAUGE_MIN = -0.1;
	const double GAUGE_ZERO = 0.0;
	const double GAUGE_DANGER = 0.3;

	public bool bRisky                          // Riskyモードか否か
	{
		get;
		private set;
	}
	public int nRiskyTimes_Initial              // Risky初期値
	{
		get;
		private set;
	}
	public int nRiskyTimes                      // 残Miss回数
	{
		get;
		private set;
	}
	public bool IsFailed(EInstrumentPad part)   // 閉店状態になったかどうか
	{
		if (bRisky) {
			return (nRiskyTimes <= 0);
		}
		return this.db現在のゲージ値[(int)part] <= GAUGE_MIN;
	}
	public bool IsDanger(EInstrumentPad part)   // DANGERかどうか
	{
		if (bRisky) {
			switch (nRiskyTimes_Initial) {
				case 1:
					return false;
				case 2:
				case 3:
					return (nRiskyTimes <= 1);
				default:
					return (nRiskyTimes <= 2);
			}
		}
		return (this.db現在のゲージ値[(int)part] <= GAUGE_DANGER);
	}

	/// <summary>
	/// ゲージの初期化
	/// </summary>
	/// <param name="nRiskyTimes_Initial_">Riskyの初期値(0でRisky未使用)</param>
	public void Init(int nRiskyTimes_InitialVal, int nPlayer)       // ゲージ初期化
	{
		//ダメージ値の計算
		var chara = OpenTaiko.Tx.Characters[OpenTaiko.SaveFileInstances[OpenTaiko.GetActualPlayer(nPlayer)].data.Character];
		switch (chara.effect.tGetGaugeType()) {
			default:
			case "Normal":
				this.db現在のゲージ値[nPlayer] = 0;
				break;
			case "Hard":
			case "Extreme":
				this.db現在のゲージ値[nPlayer] = 100;
				break;
		}

		if (nRiskyTimes_InitialVal > 0) {
			this.bRisky = true;
			this.nRiskyTimes = OpenTaiko.ConfigIni.nRisky;
			this.nRiskyTimes_Initial = OpenTaiko.ConfigIni.nRisky;
		}

		float gaugeRate = 0f;
		float dbDamageRate = 2.0f;

		int nanidou = OpenTaiko.stageSongSelect.nChoosenSongDifficulty[nPlayer];

		switch (this.DTX[nPlayer].LEVELtaiko[nanidou]) {
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
				gaugeRate = this.fGaugeMaxRate[0];
				dbDamageRate = 0.625f;
				break;


			case 8:
				gaugeRate = this.fGaugeMaxRate[1];
				dbDamageRate = 0.625f;
				break;

			case 9:
			case 10:
			default:
				gaugeRate = this.fGaugeMaxRate[2];
				dbDamageRate = 2.0f;
				break;
		}

		double[] nGaugeRankValue_branch = (nanidou == (int)Difficulty.Tower) ? [0, 0, 0]
			: this.GetGaugeRankBranched(nPlayer, gaugeRate);

		//ゲージ値計算
		//実機に近い計算

		//2015.03.26 kairera0467 計算を初期化時にするよう修正。

		#region [ Handling infinity cases ]
		float gaugeRankLastFinite = 0.4f; // arbitrary fallback value
		float[] fAddVolume = new float[] { 1.0f, 0.5f, dbDamageRate };

		for (int ib = 0; ib < 3; ++ib) {
			if (double.IsFinite(nGaugeRankValue_branch[ib])) //値がInfintyかチェック
				gaugeRankLastFinite = (float)(nGaugeRankValue_branch[ib] / 100.0f);
			for (int ij = 0; ij < 3; ++ij)
				this.dbゲージ増加量_Branch[ib, ij][nPlayer] = gaugeRankLastFinite * fAddVolume[ij];
		}
		#endregion

		#region [Rounding process]
		Func<float, float>? gaugeRoundFunc = this.DTX[nPlayer].GaugeIncreaseMode switch {
			GaugeIncreaseMode.Normal or GaugeIncreaseMode.Floor => MathF.Truncate, // 切り捨て
			GaugeIncreaseMode.Round => MathF.Round, // 四捨五入
			GaugeIncreaseMode.Ceiling => MathF.Ceiling, // 切り上げ
			GaugeIncreaseMode.NotFix or _ => null, // 丸めない
		};
		if (gaugeRoundFunc != null) {
			for (int ib = 0; ib < 3; ++ib) {
				for (int ij = 0; ij < 3; ++ij)
					dbゲージ増加量_Branch[ib, ij][nPlayer] = gaugeRoundFunc(dbゲージ増加量_Branch[ib, ij][nPlayer] * 10000.0f) / 10000.0f;
			}
		}

		float gaugeFillRatio = chara.effect.tGetGaugeType() switch {
			"Hard" => HGaugeMethods.HardGaugeFillRatio,
			"Extreme" => HGaugeMethods.ExtremeGaugeFillRatio,
			"Normal" or _ => 1.0f,
		};
		if (gaugeFillRatio != 1) {
			for (int ib = 0; ib < 3; ++ib) {
				for (int ij = 0; ij < 3; ++ij)
					dbゲージ増加量_Branch[ib, ij][nPlayer] *= gaugeFillRatio;
			}
		}
		#endregion
	}

	private double[] GetGaugeRankBranched(int iPlayer, float percentGreatsToMaxGauge) {
		// For branched chart, each main route can have notes in different branch and therefore different gauge rate
		// ideal gauge rate: sum[bn](nNotes_bt_bn[bt][bn] * gaugeRate[bn]) ≒ gaugeTarget
		CTja tja = this.DTX[iPlayer];
		var nNotesC_bn = tja.nNotes_Initial_Common;
		var nNotes_bt_bn = tja.nNotes_Branch;
		double gaugeTarget = (double)10000.0f / (percentGreatsToMaxGauge / 100.0f);

		// No missable notes in all main routes after initial common section
		if (nNotes_bt_bn.All(nbt => nbt.All(nbn => (nbn == 0)))) {
			double rankC = gaugeTarget / nNotesC_bn.Sum();
			return [rankC, rankC, rankC];
		}

		// Less notes per every branch than any other routes (e.g., "roll-only" branch at end of chart)
		// => only consider the "superset" route
		// => negative gauge rate avoided <- impossible to derive sum[bn](x[bn] * gaugeRate[bn]) = 0 with some x[bn] > 0 and the other x[bn] = 0
		// => impossible solution avoided <- impossible to decompose nNotes_bt_bn[route] into sum[bt](x[bt] * nNotes_bt_bn[bt]) with x[route] = 0
		var routeSuper = FindSuperSetRoute(nNotes_bt_bn);

		// Significant less or no notes after initial common section, and not a "subset" route (e.g., "hidden Expert route")
		// => excluded from gauge rate calculating
		const double rateMax = 2;
		var nNotesA_bt_bn = nNotes_bt_bn.Select(nbt => nbt.Zip(nNotesC_bn).Select(nbn => nbn.First + nbn.Second).ToArray()).ToArray();
		var nNotesA_bt = nNotesA_bt_bn.Select(nbt => nbt.Sum()).ToArray();
		var nNotesA_max = nNotesA_bt.Max();

		var routes = routeSuper.Distinct().ToArray();
		List<CTja.ECourse> routesSolve = new(routes.Length);
		List<CTja.ECourse> routesUnsolve = new(routes.Length);
		foreach (var r in routes)
			((nNotesA_bt[(int)r] >= nNotesA_max / rateMax) ? routesSolve : routesUnsolve).Add(r);

		// find gaugeRate[bn] such that sum[bn](nNotes_bt_bn[bt][bn] * gaugeRate[bn]) = gaugeTarget
		double[][] mat = routesSolve
			.Select(r => (double[])[.. nNotesA_bt_bn[(int)routeSuper[(int)r]], gaugeTarget])
			.ToArray();
		(int nEquats, int nFalses) = ToReducedEchelonForm(mat, 3);

		void addGaugeWarn(string reason, string? solution = null) {
			LogNotification.PopWarning($"[{tja.strFileName}]: Unable to calculate a set of suitable gauge increments for player {iPlayer + 1}: {reason}{(string.IsNullOrEmpty(solution) ? "" : $"; {solution}")}");
			Trace.TraceWarning($"nNotes_Initial_Common: [{string.Join(", ", nNotesC_bn)}], nNotes_Branch: {string.Join(", ", nNotes_bt_bn.Select(nbt => $"[{string.Join(", ", nbt)}]"))}");
			Trace.TraceWarning($"Considered routes: {string.Join(", ", routesSolve)}, resulting matrix: {string.Join(", ", mat.Select(row => $"[{string.Join(", ", row)}]"))}");
			Trace.TraceWarning($"TJA file: '{tja.strFullPath}', at {(Difficulty)tja.n参照中の難易度}");
		}

		if (nFalses > 0) { // unexpected algorithm failure, should not happen
			addGaugeWarn("contradiction of solution set", "fall back to simple division");
			return nNotesA_bt.Select(nbt => gaugeTarget / nbt).ToArray();
		}

		// Minimize difference between gaugeRate: find the point on the solution space which is closest to the line (rankN = rankE = rankM)
		double[] ranks = [double.NaN, double.NaN, double.NaN];
		// solve independent variables first
		for (int row = 0; row < mat.Length; ++row) {
			for (int col = 0; col < 3; ++col) {
				if (mat[row][col] == 0)
					continue;
				if (!mat[row][(col + 1)..3].All(c => (c == 0)))
					break;
				var x = mat[row][3] / mat[row][col];
				if (!(double.IsFinite(x) && x > 0))
					addGaugeWarn($"non-positive solution for route {(CTja.ECourse)row}: {x}");
				ranks[row] = x;
				// remove row from equation rows
				--nEquats;
				// shift rows
				var rowR = mat[row];
				for (int ir = row; ir < mat.Length - 1; ++ir)
					mat[ir] = mat[ir + 1];
				mat[mat.Length - 1] = rowR;
				Array.Fill(rowR, 0);
				--row;
			}
		}
		// check dependent variables (where some a_i > 0 and the other a_i == 0)
		if (nEquats == 1) { // a plane: [1 a_E a_M | g_N] or [0 1 a_M | g_N]
			var x = mat[0][3] / mat[0][0..3].Sum();
			if (!(double.IsFinite(x) && x > 0)) {
				addGaugeWarn($"non-positive plane solution: {x}");
			} else { // insect with (rankN = rankE = rankM)
				for (int ix = 0; ix < 3; ++ix) {
					if (!(double.IsFinite(ranks[ix]) && ranks[ix] > 0) && mat[0][ix] != 0) // only solve relevant rates
						ranks[ix] = x;
				}
			}
		} else if (nEquats == 2) { // a line: [1 0 a_MN | g_N] [0 1 a_ME | g_E] [0 0 0 | 0]
			// => line: (rankN - g_N) / a_MN = (rankE - g_E) / a_ME = rankM / -1
			// => line = (g_N, g_E, 0) + t0 * (a_MN, a_ME, -1), t0 ∈ ℝ
			double[] v = [a_MN, a_ME, -1];
			// target line: rankN = rankE = rankM
			// => target line = t1 * (1, 1, 1), t1 ∈ ℝ
			double[] vt = [1, 1, 1];
			// perpendicular line: (g_N, g_E, 0) + t0 * (a_MN, a_ME, -1) + t2 * (1, 1, 1) × (a_MN, a_ME, -1)
			double[] vp = [vt[1] * v[2] - vt[2] * v[1], -(vt[0] * v[2] - vt[2] * v[0]), vt[0] * v[1] - vt[1] * v[0]];
			if (vp.All(x => (x == 0))) {
				addGaugeWarn($"non-positive line solution: line vector: [{string.Join(", ", v)}]^T");
			} else {
				// solve: (g_N, g_E, 0) + t0 * (a_MN, a_ME, -1) + t2 * (1, 1, 1) × (a_MN, a_ME, -1) = t1 * (1, 1, 1)
				double[][] matT = Enumerable.Range(0, 3).Select(i => (double[])[v[i], -vt[i], vp[i], mat[i][3]]).ToArray();
				ToReducedEchelonForm(matT, 3);
			}
		}

		return ranks;
	}

	private static CTja.ECourse[] FindSuperSetRoute(int[][] nNotes_bt_bn) {
		// make set
		CTja.ECourse[] iRouteSuper = [CTja.ECourse.eNormal, CTja.ECourse.eExpert, CTja.ECourse.eMaster];
		// find most or (for 0) nearest-proper "superset" note distribution
		for (int ib = 0; ib < 3; ++ib) {
			bool noNotesI = (nNotes_bt_bn[ib].Sum() == 0);
			int tb = ib;
			int diff = 0;
			for (int jb = 0; jb < 3; ++jb) {
				if (ib == jb)
					continue;
				int[] diffsJ = nNotes_bt_bn[jb]
					.Zip(nNotes_bt_bn[ib])
					.Select(nbn => nbn.First - nbn.Second)
					.ToArray();
				if (noNotesI ? diffsJ.All(d => (d > 0)) : diffsJ.All(d => (d >= 0))) {
					int diffJ = diffsJ.Sum();
					if ((tb == ib) || (noNotesI ? diffJ < diff : diffJ > diff)) {
						tb = jb;
						diff = diffJ;
					}
				}
			}
			if (tb != ib)
				DisjointSetUnion(iRouteSuper, (CTja.ECourse)ib, (CTja.ECourse)tb);
		}
		// finalize
		for (int ib = 0; ib < 3; ++ib)
			DisjointSetFind(iRouteSuper, (CTja.ECourse)ib);
		return iRouteSuper;
	}

	private static void DisjointSetUnion(CTja.ECourse[] sets, CTja.ECourse src, CTja.ECourse dst) {
		var rootSrc = DisjointSetFind(sets, src);
		var rootDst = DisjointSetFind(sets, dst);
		if (rootDst != rootSrc)
			sets[(int)rootSrc] = rootDst;
	}

	private static CTja.ECourse DisjointSetFind(CTja.ECourse[] sets, CTja.ECourse x) {
		if (sets[(int)x] == x)
			return x;
		return sets[(int)x] = DisjointSetFind(sets, sets[(int)x]);
	}

	private static (int nEquats, int nFalses) ToReducedEchelonForm(double[][] mat, int nCoefs) {
		// Gauss-Jordan elimination without normalizing
		for (int row = 0, col = 0; row < mat.Length && col < nCoefs; ++col) {
			// find pivot row
			int rowPivot = row;
			for (int ir = row + 1; ir < mat.Length; ++ir) {
				if (Math.Abs(mat[ir][col]) > Math.Abs(mat[rowPivot][col]))
					rowPivot = ir;
			}
			if (mat[rowPivot][col] == 0) // no pivot found, skip col
				continue;
			if (rowPivot != row)
				(mat[rowPivot], mat[row]) = (mat[row], mat[rowPivot]); // swap rows
			// eliminate other rows
			for (int ir = 0; ir < mat.Length; ++ir) {
				if (ir == row)
					continue; // skip pivot row
				double factor = mat[ir][col] / mat[row][col];
				for (int ic = 0; ic < mat[ir].Length; ++ic)
					mat[ir][ic] -= factor * mat[row][ic];
				mat[ir][col] = 0; // eliminated completely
			}
			++row;
		}
		// final normalization + solution statistics
		int nEquats = 0, nFalses = 0;
		for (int row = 0; row < mat.Length; ++row) {
			for (int col = 0; col < mat[row].Length; ++col) {
				if (col == 0)
					continue;
				for (int ic = col + 1; ic < mat[row].Length; ++ic)
					mat[row][ic] /= mat[row][col];
				mat[row][col] = 1; // normalized
				if (col < nCoefs)
					++nEquats;
				else
					++nFalses;
				break;
			}
		}
		return (nEquats, nFalses);
	}

	#region [ DAMAGE ]
#if true       // DAMAGELEVELTUNING
	#region [ DAMAGELEVELTUNING ]
	// ----------------------------------
	public float[,] fDamageGaugeDelta = {			// #23625 2011.1.10 ickw_284: tuned damage/recover factors
		// drums,   guitar,  bass
		{  0.004f,  0.006f,  0.006f,  0.004f },
		{  0.002f,  0.003f,  0.003f,  0.002f },
		{  0.000f,  0.000f,  0.000f,  0.000f },
		{ -0.020f, -0.030f, -0.030f, -0.020f },
		{ -0.050f, -0.050f, -0.050f, -0.050f }
	};
	public float[] fDamageLevelFactor = {
		0.5f, 1.0f, 1.5f
	};

	//譜面レベル, 判定
	public float[,][] dbゲージ増加量_Branch = new float[3, 3][];


	public float[] fGaugeMaxRate =
	{
		70.7f, // 1～7
		70f,   // 8
		75.0f, // 9～10
		78.5f, // 11
		80.5f, // 12
		82f,   // 13+
	};//おおよその値。

	// ----------------------------------
	#endregion
#endif



	public void MineDamage(int nPlayer) {
		this.db現在のゲージ値[nPlayer] = Math.Max(0, this.db現在のゲージ値[nPlayer] - HGaugeMethods.BombDamage);
	}

	public void FuseDamage(int nPlayer) {
		this.db現在のゲージ値[nPlayer] = Math.Max(0, this.db現在のゲージ値[nPlayer] - HGaugeMethods.FuserollDamage);
	}

	public void Damage(EInstrumentPad screenmode, ENoteJudge e今回の判定, int nPlayer, CTja.ECourse? chipBranch = null) {
		float fDamage;
		int nコース = (int)(chipBranch ?? OpenTaiko.stageGameScreen.nCurrentBranch[nPlayer]);

		switch (e今回の判定) {
			case ENoteJudge.Perfect:
			case ENoteJudge.Great: {
					fDamage = this.dbゲージ増加量_Branch[nコース, 0][nPlayer];
				}
				break;
			case ENoteJudge.Good: {
					fDamage = this.dbゲージ増加量_Branch[nコース, 1][nPlayer];
				}
				break;
			case ENoteJudge.Poor:
			case ENoteJudge.Miss: {
					fDamage = this.dbゲージ増加量_Branch[nコース, 2][nPlayer];

					if (fDamage >= 0) {
						fDamage = -fDamage;
					}

					var chara = OpenTaiko.Tx.Characters[OpenTaiko.SaveFileInstances[OpenTaiko.GetActualPlayer(nPlayer)].data.Character];

					int nanidou = OpenTaiko.stageSongSelect.nChoosenSongDifficulty[nPlayer];
					int level = this.DTX[nPlayer].LEVELtaiko[nanidou];

					switch (chara.effect.tGetGaugeType()) {
						case "Hard":
							fDamage = -HGaugeMethods.tHardGaugeGetDamage((Difficulty)nanidou, level);
							break;
						case "Extreme":
							fDamage = -HGaugeMethods.tExtremeGaugeGetDamage((Difficulty)nanidou, level);
							break;
					}

					if (this.bRisky) {
						this.nRiskyTimes--;
					}
				}

				break;



			default: {
					fDamage = this.dbゲージ増加量_Branch[nコース, 0][nPlayer];
					break;
				}


		}



		this.db現在のゲージ値[nPlayer] = Math.Round(this.db現在のゲージ値[nPlayer] + fDamage, 5, MidpointRounding.ToEven);

		if (this.db現在のゲージ値[nPlayer] >= 100.0)
			this.db現在のゲージ値[nPlayer] = 100.0;
		else if (this.db現在のゲージ値[nPlayer] <= 0.0)
			this.db現在のゲージ値[nPlayer] = 0.0;


		//CDTXMania.stage演奏ドラム画面.nGauge = fDamage;

	}

	public virtual void Start(int nLane, ENoteJudge judge, int player) {
	}

	//-----------------
	#endregion

	private CTja[] DTX = new CTja[OpenTaiko.MAX_PLAYERS];
	public double[] db現在のゲージ値 = new double[5];
	protected CCounter ct炎;
	protected CCounter ct虹アニメ;
	protected CCounter ct虹透明度;
	protected CTexture[] txゲージ虹 = new CTexture[12];
	protected CTexture[] txゲージ虹2P = new CTexture[12];
}
