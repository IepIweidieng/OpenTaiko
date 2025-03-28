﻿using System.Text;
using FDK;
using NLua;

namespace OpenTaiko;

class ScriptBGFunc {
	private Dictionary<string, CTexture> Textures;
	private string DirPath;

	public ScriptBGFunc(Dictionary<string, CTexture> texs, string dirPath) {
		Textures = texs;
		DirPath = dirPath;
	}
	public (int x, int y) DrawText(double x, double y, string text) {
		return OpenTaiko.actTextConsole.Print((int)x, (int)y, CTextConsole.EFontType.White, text);
	}
	public (int x, int y) DrawNum(double x, double y, double text) {
		return OpenTaiko.actTextConsole.Print((int)x, (int)y, CTextConsole.EFontType.White, text.ToString());
	}
	public void AddGraph(string fileName) {
		string trueFileName = fileName.Replace('/', Path.DirectorySeparatorChar);
		trueFileName = trueFileName.Replace('\\', Path.DirectorySeparatorChar);
		Textures.Add(fileName, OpenTaiko.tテクスチャの生成($@"{DirPath}{Path.DirectorySeparatorChar}{trueFileName}"));
	}
	public void DrawGraph(double x, double y, string fileName) {
		Textures[fileName]?.t2D描画((int)x, (int)y);
	}
	public void DrawRectGraph(double x, double y, int rect_x, int rect_y, int rect_width, int rect_height, string fileName) {
		Textures[fileName]?.t2D描画((int)x, (int)y, new System.Drawing.RectangleF(rect_x, rect_y, rect_width, rect_height));
	}
	public void DrawGraphCenter(double x, double y, string fileName) {
		Textures[fileName]?.t2D拡大率考慮中央基準描画((int)x, (int)y);
	}
	public void DrawGraphRectCenter(double x, double y, int rect_x, int rect_y, int rect_width, int rect_height, string fileName) {
		Textures[fileName]?.t2D拡大率考慮中央基準描画((int)x, (int)y, new System.Drawing.RectangleF(rect_x, rect_y, rect_width, rect_height));
	}
	public void SetOpacity(double opacity, string fileName) {
		if (Textures[fileName] != null)
			Textures[fileName].Opacity = (int)opacity;
	}
	public void SetScale(double xscale, double yscale, string fileName) {
		if (Textures[fileName] != null) {
			Textures[fileName].vcScaleRatio.X = (float)xscale;
			Textures[fileName].vcScaleRatio.Y = (float)yscale;
		}
	}
	public void SetRotation(double angle, string fileName) {
		if (Textures[fileName] != null) {
			Textures[fileName].fZ軸中心回転 = (float)(angle * Math.PI / 180);
		}
	}
	public void SetColor(double r, double g, double b, string fileName) {
		if (Textures[fileName] != null) {
			Textures[fileName].color4 = new Color4((float)r, (float)g, (float)b, 1f);
		}
	}
	public void SetBlendMode(string type, string fileName) {
		if (Textures[fileName] != null) {
			switch (type) {
				case "Normal":
				default:
					Textures[fileName].b加算合成 = false;
					Textures[fileName].b乗算合成 = false;
					Textures[fileName].b減算合成 = false;
					Textures[fileName].bスクリーン合成 = false;
					break;
				case "Add":
					Textures[fileName].b加算合成 = true;
					Textures[fileName].b乗算合成 = false;
					Textures[fileName].b減算合成 = false;
					Textures[fileName].bスクリーン合成 = false;
					break;
				case "Multi":
					Textures[fileName].b加算合成 = false;
					Textures[fileName].b乗算合成 = true;
					Textures[fileName].b減算合成 = false;
					Textures[fileName].bスクリーン合成 = false;
					break;
				case "Sub":
					Textures[fileName].b加算合成 = false;
					Textures[fileName].b乗算合成 = false;
					Textures[fileName].b減算合成 = true;
					Textures[fileName].bスクリーン合成 = false;
					break;
				case "Screen":
					Textures[fileName].b加算合成 = false;
					Textures[fileName].b乗算合成 = false;
					Textures[fileName].b減算合成 = false;
					Textures[fileName].bスクリーン合成 = true;
					break;
			}
		}
	}

	public double GetTextureWidth(string fileName) {
		if (Textures[fileName] != null) {
			return Textures[fileName].szTextureSize.Width;
		}
		return -1;
	}

	public double GetTextureHeight(string fileName) {
		if (Textures[fileName] != null) {
			return Textures[fileName].szTextureSize.Height;
		}
		return -1;
	}
}
class ScriptBG : IDisposable {
	public Dictionary<string, CTexture> Textures;

	protected Lua LuaScript;

	protected LuaFunction LuaSetConstValues;
	protected LuaFunction LuaUpdateValues;
	protected LuaFunction LuaClearIn;
	protected LuaFunction LuaClearOut;
	protected LuaFunction LuaInit;
	protected LuaFunction LuaUpdate;
	protected LuaFunction LuaDraw;

	public ScriptBG(string filePath) {
		Textures = new Dictionary<string, CTexture>();

		if (!File.Exists(filePath)) return;

		LuaScript = new Lua();
		LuaScript.State.Encoding = Encoding.UTF8;
		LuaSecurity.Secure(LuaScript);

		LuaScript["func"] = new ScriptBGFunc(Textures, Path.GetDirectoryName(filePath));


		try {
			using (var streamAPI = new StreamReader("BGScriptAPI.lua", Encoding.UTF8)) {
				using (var stream = new StreamReader(filePath, Encoding.UTF8)) {
					var text = $"{streamAPI.ReadToEnd()}\n{stream.ReadToEnd()}";
					LuaScript.DoString(text);
				}
			}

			LuaSetConstValues = LuaScript.GetFunction("setConstValues");
			LuaUpdateValues = LuaScript.GetFunction("updateValues");
			LuaClearIn = LuaScript.GetFunction("clearIn");
			LuaClearOut = LuaScript.GetFunction("clearOut");
			LuaInit = LuaScript.GetFunction("init");
			LuaUpdate = LuaScript.GetFunction("update");
			LuaDraw = LuaScript.GetFunction("draw");
		} catch (Exception ex) {
			Crash(ex);
		}
	}
	public bool Exists() {
		return LuaScript != null;
	}
	private void Crash(Exception exception) {
		LogNotification.PopError($"Lua ScriptBG Error: {exception.ToString()}");
		LuaScript?.Dispose();
		LuaScript = null;
	}
	public void Dispose() {
		List<CTexture> texs = new List<CTexture>();
		foreach (var tex in Textures.Values) {
			texs.Add(tex);
		}
		for (int i = 0; i < texs.Count; i++) {
			var tex = texs[i];
			OpenTaiko.tテクスチャの解放(ref tex);
		}

		Textures.Clear();

		LuaScript?.Dispose();

		LuaSetConstValues?.Dispose();
		LuaUpdateValues?.Dispose();
		LuaClearIn?.Dispose();
		LuaClearOut?.Dispose();
		LuaInit?.Dispose();
		LuaUpdate?.Dispose();
		LuaDraw?.Dispose();
	}

	public void ClearIn(int player) {
		if (LuaScript == null) return;
		try {
			LuaClearIn.Call(player);
		} catch (Exception ex) {
			Crash(ex);
		}
	}
	public void ClearOut(int player) {
		if (LuaScript == null) return;
		try {
			LuaClearOut.Call(player);
		} catch (Exception ex) {
			Crash(ex);
		}
	}
	public void Init() {
		if (LuaScript == null) return;
		try {
			// Preprocessing
			string[] raritiesP = { "Common", "Common", "Common", "Common", "Common" };
			string[] raritiesC = { "Common", "Common", "Common", "Common", "Common" };

			if (OpenTaiko.Tx.Puchichara != null && OpenTaiko.Tx.Characters != null) {
				for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
					raritiesP[i] = OpenTaiko.Tx.Puchichara[PuchiChara.tGetPuchiCharaIndexByName(OpenTaiko.GetActualPlayer(i))].metadata.Rarity;
					raritiesC[i] = OpenTaiko.Tx.Characters[OpenTaiko.SaveFileInstances[OpenTaiko.GetActualPlayer(i)].data.Character].metadata.Rarity;
				}
			}

			// Initialisation
			LuaSetConstValues.Call(OpenTaiko.ConfigIni.nPlayerCount,
				OpenTaiko.P1IsBlue(),
				OpenTaiko.ConfigIni.sLang,
				OpenTaiko.ConfigIni.SimpleMode,
				raritiesP,
				raritiesC
			);

			LuaUpdateValues.Call(OpenTaiko.FPS.DeltaTime,
				OpenTaiko.FPS.NowFPS,
				OpenTaiko.stageGameScreen.bIsAlreadyCleared,
				0,
				OpenTaiko.stageGameScreen.AIBattleState,
				OpenTaiko.stageGameScreen.bIsAIBattleWin,
				OpenTaiko.stageGameScreen.actGauge.db現在のゲージ値,
				OpenTaiko.stageGameScreen.actPlayInfo.dbBPM,
				new bool[] { false, false, false, false, false },
				-1
			);

			LuaInit.Call();
		} catch (Exception ex) {
			Crash(ex);
		}
	}

	public void Update() {
		if (LuaScript == null) return;
		try {
			float currentFloorPositionMax140 = 0;

			if (OpenTaiko.stageSongSelect.rChoosenSong != null && OpenTaiko.stageSongSelect.rChoosenSong.score[5] != null) {
				int maxFloor = OpenTaiko.stageSongSelect.rChoosenSong.score[5].譜面情報.nTotalFloor;
				int nightTime = Math.Max(140, maxFloor / 2);

				currentFloorPositionMax140 = Math.Min(OpenTaiko.stageGameScreen.actPlayInfo.NowMeasure[0] / (float)nightTime, 1f);
			}
			double timestamp = -1.0;

			if (OpenTaiko.TJA != null) {
				double msTimeOffset = OpenTaiko.stageSongSelect.nChoosenSongDifficulty[0] != (int)Difficulty.Dan ? 0 : -CTja.msDanNextSongDelay;
				// Due to the fact that all Dans use DELAY to offset instead of OFFSET, Dan offset can't be properly synced. ¯\_(ツ)_/¯

				timestamp = (OpenTaiko.TJA.RawTjaTimeToDefTime(
					OpenTaiko.TJA.TjaTimeToRawTjaTimeNote(
						OpenTaiko.TJA.GameTimeToTjaTime(SoundManager.PlayTimer.NowTimeMs))
				) + msTimeOffset) / 1000.0;
			}

			LuaUpdateValues.Call(OpenTaiko.FPS.DeltaTime,
				OpenTaiko.FPS.NowFPS,
				OpenTaiko.stageGameScreen.bIsAlreadyCleared,
				(double)currentFloorPositionMax140,
				OpenTaiko.stageGameScreen.AIBattleState,
				OpenTaiko.stageGameScreen.bIsAIBattleWin,
				OpenTaiko.stageGameScreen.actGauge.db現在のゲージ値,
				OpenTaiko.stageGameScreen.actPlayInfo.dbBPM,
				OpenTaiko.stageGameScreen.bIsGOGOTIME,
				timestamp);
			/*LuaScript.SetObjectToPath("fps", TJAPlayer3.FPS.n現在のFPS);
            LuaScript.SetObjectToPath("deltaTime", TJAPlayer3.FPS.DeltaTime);
            LuaScript.SetObjectToPath("isClear", TJAPlayer3.stage演奏ドラム画面.bIsAlreadyCleared);
            LuaScript.SetObjectToPath("towerNightOpacity", (double)(255 * currentFloorPositionMax140));*/
			LuaUpdate.Call();
		} catch (Exception ex) {
			Crash(ex);
		}
	}
	public void Draw() {
		if (LuaScript == null) return;
		try {
			LuaDraw.Call();
		} catch (Exception ex) {
			Crash(ex);
		}
	}
}
