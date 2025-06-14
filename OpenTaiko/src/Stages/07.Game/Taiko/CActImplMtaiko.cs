﻿using System.Runtime.InteropServices;
using FDK;
using Rectangle = System.Drawing.Rectangle;

namespace OpenTaiko;

internal class CActImplMtaiko : CActivity {
	/// <summary>
	/// mtaiko部分を描画するクラス。左側だけ。
	///
	/// </summary>
	public CActImplMtaiko() {
		base.IsDeActivated = true;
	}

	public override void Activate() {
		for (int i = 0; i < 25; i++) {
			STパッド状態 stパッド状態 = new STパッド状態();
			stパッド状態.n明るさ = 0;
			this.stパッド状態[i] = stパッド状態;
		}

		this.ctレベルアップダウン = new CCounter[5];
		ctSymbolFlash = new CCounter[5];
		this.After = new CTja.ECourse[5];
		this.Before = new CTja.ECourse[5];
		for (int i = 0; i < 5; i++) {
			this.ctレベルアップダウン[i] = new CCounter();
			BackSymbolEvent(i);
		}

		base.Activate();
	}

	public override void DeActivate() {
		this.ctレベルアップダウン = null;

		base.DeActivate();
	}

	public override void CreateManagedResource() {
		base.CreateManagedResource();
	}

	public override void ReleaseManagedResource() {
		base.ReleaseManagedResource();
	}

	public override int Draw() {
		if (base.IsFirstDraw) {
			this.nフラッシュ制御タイマ = SoundManager.PlayTimer.NowTimeMs;
			base.IsFirstDraw = false;
		}

		long num = SoundManager.PlayTimer.NowTimeMs;
		if (num < this.nフラッシュ制御タイマ) {
			this.nフラッシュ制御タイマ = num;
		}
		while ((num - this.nフラッシュ制御タイマ) >= 20) {
			for (int j = 0; j < 25; j++) {
				if (this.stパッド状態[j].n明るさ > 0) {
					this.stパッド状態[j].n明るさ--;
				}
			}
			this.nフラッシュ制御タイマ += 20;
		}


		//this.nHS = TJAPlayer3.ConfigIni.nScrollSpeed.Drums < 8 ? TJAPlayer3.ConfigIni.nScrollSpeed.Drums : 7;



		for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
			int bg_x;
			int bg_y;
			if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
				bg_x = OpenTaiko.Skin.Game_Taiko_Background_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				bg_y = OpenTaiko.Skin.Game_Taiko_Background_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);
			} else if (OpenTaiko.ConfigIni.nPlayerCount == 4 || OpenTaiko.ConfigIni.nPlayerCount == 3) {
				bg_x = OpenTaiko.Skin.Game_Taiko_Background_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				bg_y = OpenTaiko.Skin.Game_Taiko_Background_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);
			} else {
				bg_x = OpenTaiko.Skin.Game_Taiko_Background_X[i];
				bg_y = OpenTaiko.Skin.Game_Taiko_Background_Y[i];
			}

			CTexture tex = null;

			switch (i) {
				case 0: {
						if (OpenTaiko.stageSongSelect.nChoosenSongDifficulty[0] == (int)Difficulty.Dan) {
							tex = OpenTaiko.Tx.Taiko_Background[2];
						} else if (OpenTaiko.ConfigIni.bTokkunMode) {
							if (OpenTaiko.P1IsBlue())
								tex = OpenTaiko.Tx.Taiko_Background[6];
							else
								tex = OpenTaiko.Tx.Taiko_Background[5];
						} else {
							if (OpenTaiko.P1IsBlue())
								tex = OpenTaiko.Tx.Taiko_Background[4];
							else
								tex = OpenTaiko.Tx.Taiko_Background[0];
						}
					}
					break;
				case 1: {
						if (OpenTaiko.ConfigIni.bAIBattleMode) {
							tex = OpenTaiko.Tx.Taiko_Background[9];
						} else {
							if (OpenTaiko.ConfigIni.nPlayerCount == 2)
								tex = OpenTaiko.Tx.Taiko_Background[1];
							else
								tex = OpenTaiko.Tx.Taiko_Background[4];
						}
					}
					break;
				case 2:
					tex = OpenTaiko.Tx.Taiko_Background[7];
					break;
				case 3:
					tex = OpenTaiko.Tx.Taiko_Background[8];
					break;
				case 4:
					tex = OpenTaiko.Tx.Taiko_Background[11];
					break;
			}

			tex?.t2D描画(bg_x, bg_y);
		}
		/*
        if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Dan)  // Dan-i Dojo
            TJAPlayer3.Tx.Taiko_Background[2]?.t2D描画(bg_x[0], bg_y[0]);
        else if (TJAPlayer3.stage選曲.n確定された曲の難易度[0] == (int)Difficulty.Tower) // Taiko Towers
            TJAPlayer3.Tx.Taiko_Background[3]?.t2D描画(bg_x[0], bg_y[0]);
        else if (!TJAPlayer3.ConfigIni.bTokkunMode
                || TJAPlayer3.Tx.Taiko_Background[5] == null
                || TJAPlayer3.Tx.Taiko_Background[6] == null)
        {
            // Taiko Mode
            if (TJAPlayer3.stage演奏ドラム画面.bDoublePlay)
            {
                if (TJAPlayer3.ConfigIni.nPlayerCount == 2)
                {
                    // 2P
                    if (!TJAPlayer3.ConfigIni.bAIBattleMode || TJAPlayer3.Tx.Taiko_Background[9] == null)
                        TJAPlayer3.Tx.Taiko_Background[1]?.t2D描画(bg_x[1], bg_y[1]);
                    else
                        TJAPlayer3.Tx.Taiko_Background[9]?.t2D描画(bg_x[1], bg_y[1]);
                }
                else
                {
                    if (TJAPlayer3.ConfigIni.nPlayerCount >= 2)
                        TJAPlayer3.Tx.Taiko_Background[4]?.t2D描画(bg_x[1], bg_y[1]);
                    if (TJAPlayer3.ConfigIni.nPlayerCount >= 3)
                        TJAPlayer3.Tx.Taiko_Background[7]?.t2D描画(bg_x[2], bg_y[2]);
                    if (TJAPlayer3.ConfigIni.nPlayerCount >= 4)
                        TJAPlayer3.Tx.Taiko_Background[8]?.t2D描画(bg_x[3], bg_y[3]);
                    if (TJAPlayer3.ConfigIni.nPlayerCount >= 5)
                        TJAPlayer3.Tx.Taiko_Background[11]?.t2D描画(bg_x[4], bg_y[4]);
                }
            }
            if (TJAPlayer3.P1IsBlue())
                 TJAPlayer3.Tx.Taiko_Background[4]?.t2D描画(bg_x[0], bg_y[0]);
            else
                TJAPlayer3.Tx.Taiko_Background[0]?.t2D描画(bg_x[0], bg_y[0]);
        }
        else
        {
            // Training Mode
            if (TJAPlayer3.P1IsBlue())
                TJAPlayer3.Tx.Taiko_Background[6]?.t2D描画(bg_x[0], bg_y[0]);
            else
                TJAPlayer3.Tx.Taiko_Background[5]?.t2D描画(bg_x[0], bg_y[0]);
        }
        */

		int getMTaikoOpacity(int brightness) {
			if (OpenTaiko.ConfigIni.SimpleMode) {
				return brightness <= 0 ? 0 : 255;
			} else {
				return brightness * 73;
			}
		}

		for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
			int taiko_x;
			int taiko_y;
			if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
				taiko_x = OpenTaiko.Skin.Game_Taiko_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				taiko_y = OpenTaiko.Skin.Game_Taiko_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);
			} else if (OpenTaiko.ConfigIni.nPlayerCount == 4 || OpenTaiko.ConfigIni.nPlayerCount == 3) {
				taiko_x = OpenTaiko.Skin.Game_Taiko_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				taiko_y = OpenTaiko.Skin.Game_Taiko_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);
			} else {
				taiko_x = OpenTaiko.Skin.Game_Taiko_X[i];
				taiko_y = OpenTaiko.Skin.Game_Taiko_Y[i];
			}

			int _actual = OpenTaiko.GetActualPlayer(i);
			EGameType _gt = OpenTaiko.ConfigIni.nGameType[_actual];
			int playerShift = i * 5;

			// Drum base
			OpenTaiko.Tx.Taiko_Base[(int)_gt]?.t2D描画(taiko_x, taiko_y);

			// Taiko hits
			if (_gt == EGameType.Taiko) {
				if (OpenTaiko.Tx.Taiko_Don_Left != null && OpenTaiko.Tx.Taiko_Don_Right != null && OpenTaiko.Tx.Taiko_Ka_Left != null && OpenTaiko.Tx.Taiko_Ka_Right != null) {
					OpenTaiko.Tx.Taiko_Ka_Left.Opacity = getMTaikoOpacity(this.stパッド状態[playerShift].n明るさ);
					OpenTaiko.Tx.Taiko_Ka_Right.Opacity = getMTaikoOpacity(this.stパッド状態[1 + playerShift].n明るさ);
					OpenTaiko.Tx.Taiko_Don_Left.Opacity = getMTaikoOpacity(this.stパッド状態[2 + playerShift].n明るさ);
					OpenTaiko.Tx.Taiko_Don_Right.Opacity = getMTaikoOpacity(this.stパッド状態[3 + playerShift].n明るさ);

					OpenTaiko.Tx.Taiko_Ka_Left.t2D描画(taiko_x, taiko_y, new Rectangle(0, 0, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Height));
					OpenTaiko.Tx.Taiko_Ka_Right.t2D描画(taiko_x + OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, taiko_y, new Rectangle(OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, 0, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Height));
					OpenTaiko.Tx.Taiko_Don_Left.t2D描画(taiko_x, taiko_y, new Rectangle(0, 0, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Height));
					OpenTaiko.Tx.Taiko_Don_Right.t2D描画(taiko_x + OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, taiko_y, new Rectangle(OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, 0, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Width / 2, OpenTaiko.Tx.Taiko_Ka_Right.szTextureSize.Height));
				}
			} else if (_gt == EGameType.Konga) {
				if (OpenTaiko.Tx.Taiko_Konga_Clap != null && OpenTaiko.Tx.Taiko_Konga_Don != null && OpenTaiko.Tx.Taiko_Konga_Ka != null) {
					OpenTaiko.Tx.Taiko_Konga_Clap.Opacity = getMTaikoOpacity(this.stパッド状態[4 + playerShift].n明るさ);
					OpenTaiko.Tx.Taiko_Konga_Don.Opacity = getMTaikoOpacity(Math.Max(this.stパッド状態[2 + playerShift].n明るさ, this.stパッド状態[3 + playerShift].n明るさ));
					OpenTaiko.Tx.Taiko_Konga_Ka.Opacity = getMTaikoOpacity(Math.Max(this.stパッド状態[playerShift].n明るさ, this.stパッド状態[1 + playerShift].n明るさ));

					OpenTaiko.Tx.Taiko_Konga_Ka.t2D描画(taiko_x, taiko_y);
					OpenTaiko.Tx.Taiko_Konga_Don.t2D描画(taiko_x, taiko_y);
					OpenTaiko.Tx.Taiko_Konga_Clap.t2D描画(taiko_x, taiko_y);
				}
			}

		}


		int[] nLVUPY = new int[] { 127, 127, 0, 0 };

		for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
			if (OpenTaiko.ConfigIni.nPlayerCount > 2 || OpenTaiko.ConfigIni.SimpleMode) break;

			if (!this.ctレベルアップダウン[i].IsStoped) {
				this.ctレベルアップダウン[i].Tick();
				if (this.ctレベルアップダウン[i].IsEnded) {
					this.ctレベルアップダウン[i].Stop();
					this.Before[i] = this.After[i];
				}
			}
			if ((this.ctレベルアップダウン[i].IsTicked && (OpenTaiko.Tx.Taiko_LevelUp != null && OpenTaiko.Tx.Taiko_LevelDown != null)) && !OpenTaiko.ConfigIni.bNoInfo) {
				//this.ctレベルアップダウン[ i ].n現在の値 = 110;

				//2017.08.21 kairera0467 t3D描画に変更。
				float fScale = 1.0f;
				int nAlpha = 255;
				float[] fY = new float[] { 206, -206, 0, 0 };
				if (this.ctレベルアップダウン[i].CurrentValue >= 0 && this.ctレベルアップダウン[i].CurrentValue <= 20) {
					nAlpha = 60;
					fScale = 1.14f;
				} else if (this.ctレベルアップダウン[i].CurrentValue >= 21 && this.ctレベルアップダウン[i].CurrentValue <= 40) {
					nAlpha = 60;
					fScale = 1.19f;
				} else if (this.ctレベルアップダウン[i].CurrentValue >= 41 && this.ctレベルアップダウン[i].CurrentValue <= 60) {
					nAlpha = 220;
					fScale = 1.23f;
				} else if (this.ctレベルアップダウン[i].CurrentValue >= 61 && this.ctレベルアップダウン[i].CurrentValue <= 80) {
					nAlpha = 230;
					fScale = 1.19f;
				} else if (this.ctレベルアップダウン[i].CurrentValue >= 81 && this.ctレベルアップダウン[i].CurrentValue <= 100) {
					nAlpha = 240;
					fScale = 1.14f;
				} else if (this.ctレベルアップダウン[i].CurrentValue >= 101 && this.ctレベルアップダウン[i].CurrentValue <= 120) {
					nAlpha = 255;
					fScale = 1.04f;
				} else {
					nAlpha = 255;
					fScale = 1.0f;
				}

				if (OpenTaiko.ConfigIni.nPlayerCount > 2) continue;

				int levelChange_x = OpenTaiko.Skin.Game_Taiko_LevelChange_X[i];
				int levelChange_y = OpenTaiko.Skin.Game_Taiko_LevelChange_Y[i];

				if (this.After[i] - this.Before[i] >= 0) {
					//レベルアップ
					OpenTaiko.Tx.Taiko_LevelUp.vcScaleRatio.X = fScale;
					OpenTaiko.Tx.Taiko_LevelUp.vcScaleRatio.Y = fScale;
					OpenTaiko.Tx.Taiko_LevelUp.Opacity = nAlpha;
					OpenTaiko.Tx.Taiko_LevelUp.t2D拡大率考慮中央基準描画(levelChange_x,
						levelChange_y);
				} else {
					OpenTaiko.Tx.Taiko_LevelDown.vcScaleRatio.X = fScale;
					OpenTaiko.Tx.Taiko_LevelDown.vcScaleRatio.Y = fScale;
					OpenTaiko.Tx.Taiko_LevelDown.Opacity = nAlpha;
					OpenTaiko.Tx.Taiko_LevelDown.t2D拡大率考慮中央基準描画(levelChange_x,
						levelChange_y);
				}
			}
		}

		for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
			if (OpenTaiko.ConfigIni.bAIBattleMode && i == 1) break;


			int modIcons_x;
			int modIcons_y;
			int couse_symbol_x;
			int couse_symbol_y;
			if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
				modIcons_x = OpenTaiko.Skin.Game_Taiko_ModIcons_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				modIcons_y = OpenTaiko.Skin.Game_Taiko_ModIcons_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);

				couse_symbol_x = OpenTaiko.Skin.Game_CourseSymbol_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				couse_symbol_y = OpenTaiko.Skin.Game_CourseSymbol_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);
			} else if (OpenTaiko.ConfigIni.nPlayerCount == 4 || OpenTaiko.ConfigIni.nPlayerCount == 3) {
				modIcons_x = OpenTaiko.Skin.Game_Taiko_ModIcons_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				modIcons_y = OpenTaiko.Skin.Game_Taiko_ModIcons_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);

				couse_symbol_x = OpenTaiko.Skin.Game_CourseSymbol_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				couse_symbol_y = OpenTaiko.Skin.Game_CourseSymbol_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);
			} else {
				modIcons_x = OpenTaiko.Skin.Game_Taiko_ModIcons_X[i];
				modIcons_y = OpenTaiko.Skin.Game_Taiko_ModIcons_Y[i];

				couse_symbol_x = OpenTaiko.Skin.Game_CourseSymbol_X[i];
				couse_symbol_y = OpenTaiko.Skin.Game_CourseSymbol_Y[i];
			}

			ModIcons.tDisplayMods(modIcons_x, modIcons_y, i);

			if (OpenTaiko.Tx.Couse_Symbol[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]] != null) {
				OpenTaiko.Tx.Couse_Symbol[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].t2D描画(
					couse_symbol_x,
					couse_symbol_y
				);
			}


			if (OpenTaiko.ConfigIni.ShinuchiMode) {
				if (OpenTaiko.Tx.Couse_Symbol[(int)Difficulty.Total] != null) {
					OpenTaiko.Tx.Couse_Symbol[(int)Difficulty.Total].t2D描画(
						couse_symbol_x,
						couse_symbol_y
					);
				}

			}
		}

		for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
			int namePlate_x;
			int namePlate_y;
			int playerNumber_x;
			int playerNumber_y;
			if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
				namePlate_x = OpenTaiko.Skin.Game_Taiko_NamePlate_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				namePlate_y = OpenTaiko.Skin.Game_Taiko_NamePlate_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);

				playerNumber_x = OpenTaiko.Skin.Game_Taiko_PlayerNumber_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				playerNumber_y = OpenTaiko.Skin.Game_Taiko_PlayerNumber_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);
			} else if (OpenTaiko.ConfigIni.nPlayerCount == 4 || OpenTaiko.ConfigIni.nPlayerCount == 3) {
				namePlate_x = OpenTaiko.Skin.Game_Taiko_NamePlate_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				namePlate_y = OpenTaiko.Skin.Game_Taiko_NamePlate_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);

				playerNumber_x = OpenTaiko.Skin.Game_Taiko_PlayerNumber_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				playerNumber_y = OpenTaiko.Skin.Game_Taiko_PlayerNumber_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);
			} else {
				namePlate_x = OpenTaiko.Skin.Game_Taiko_NamePlate_X[i];
				namePlate_y = OpenTaiko.Skin.Game_Taiko_NamePlate_Y[i];

				playerNumber_x = OpenTaiko.Skin.Game_Taiko_PlayerNumber_X[i];
				playerNumber_y = OpenTaiko.Skin.Game_Taiko_PlayerNumber_Y[i];
			}

			OpenTaiko.NamePlate.tNamePlateDraw(namePlate_x, namePlate_y, i);

			if (OpenTaiko.Tx.Taiko_PlayerNumber[i] != null) {
				OpenTaiko.Tx.Taiko_PlayerNumber[i].t2D描画(playerNumber_x, playerNumber_y);
			}
		}
		return base.Draw();
	}

	public void tMtaikoEvent(int nChannel, int nHand, int nPlayer) {
		CConfigIni configIni = OpenTaiko.ConfigIni;
		bool bAutoPlay = configIni.bAutoPlay[nPlayer];
		int playerShift = 5 * nPlayer;
		var _gt = configIni.nGameType[OpenTaiko.GetActualPlayer(nPlayer)];

		switch (nPlayer) {
			case 1:
				bAutoPlay = configIni.bAutoPlay[nPlayer] || OpenTaiko.ConfigIni.bAIBattleMode;
				break;
		}

		if (!bAutoPlay) {
			switch (nChannel) {
				case 0x11:
				case 0x13:
				case 0x15:
				case 0x16:
				case 0x17: {
						this.stパッド状態[2 + nHand + playerShift].n明るさ = 8;
					}
					break;
				case 0x12: {
						this.stパッド状態[nHand + playerShift].n明るさ = 8;
					}
					break;
				case 0x14: {
						if (_gt == EGameType.Konga) {
							this.stパッド状態[4 + playerShift].n明るさ = 8;
						} else {
							this.stパッド状態[nHand + playerShift].n明るさ = 8;
						}
					}
					break;

			}
		} else {
			switch (nChannel) {
				case 0x11:
				case 0x15:
				case 0x16:
				case 0x17:
				case 0x1F: {
						this.stパッド状態[2 + nHand + playerShift].n明るさ = 8;
					}
					break;

				case 0x13:
				case 0x1A: {
						if (_gt == EGameType.Konga) {
							this.stパッド状態[0 + playerShift].n明るさ = 8;
							this.stパッド状態[2 + playerShift].n明るさ = 8;
						} else {
							this.stパッド状態[2 + playerShift].n明るさ = 8;
							this.stパッド状態[3 + playerShift].n明るさ = 8;
						}
					}
					break;

				case 0x12: {
						this.stパッド状態[nHand + playerShift].n明るさ = 8;
					}
					break;

				case 0x14:
				case 0x1B: {
						if (_gt == EGameType.Konga) {
							this.stパッド状態[4 + playerShift].n明るさ = 8;
						} else {
							this.stパッド状態[0 + playerShift].n明るさ = 8;
							this.stパッド状態[1 + playerShift].n明るさ = 8;
						}

					}
					break;

				case 0x101: {
						this.stパッド状態[nHand + playerShift].n明るさ = 8;
						this.stパッド状態[2 + (nHand == 0 ? 1 : 0) + playerShift].n明るさ = 8;
						break;
					}
			}
		}

	}

	public void tBranchEvent(CTja.ECourse After, int player, bool stopAnime = false) {
		if (stopAnime) {
			this.ctレベルアップダウン[player].Stop();
			this.Before[player] = this.After[player] = After;
			return;
		}
		if (After != this.After[player])
			this.ctレベルアップダウン[player] = new CCounter(0, 1000, 1, OpenTaiko.Timer);

		this.Before[player] = this.After[player];
		this.After[player] = After;
	}


	public void BackSymbolEvent(int player) {
		ctSymbolFlash[player] = new CCounter(0, 1000, 0.2f, OpenTaiko.Timer);
	}

	public void DrawBackSymbol() {
		for (int i = 0; i < OpenTaiko.ConfigIni.nPlayerCount; i++) {
			ctSymbolFlash[i].Tick();

			int couse_symbol_x;
			int couse_symbol_y;
			if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
				couse_symbol_x = OpenTaiko.Skin.Game_CourseSymbol_Back_5P[0] + (OpenTaiko.Skin.Game_UIMove_5P[0] * i);
				couse_symbol_y = OpenTaiko.Skin.Game_CourseSymbol_Back_5P[1] + (OpenTaiko.Skin.Game_UIMove_5P[1] * i);
			} else if (OpenTaiko.ConfigIni.nPlayerCount == 4 || OpenTaiko.ConfigIni.nPlayerCount == 3) {
				couse_symbol_x = OpenTaiko.Skin.Game_CourseSymbol_Back_4P[0] + (OpenTaiko.Skin.Game_UIMove_4P[0] * i);
				couse_symbol_y = OpenTaiko.Skin.Game_CourseSymbol_Back_4P[1] + (OpenTaiko.Skin.Game_UIMove_4P[1] * i);
			} else {
				couse_symbol_x = OpenTaiko.Skin.Game_CourseSymbol_Back_X[i];
				couse_symbol_y = OpenTaiko.Skin.Game_CourseSymbol_Back_Y[i];
			}


			if (OpenTaiko.Tx.Couse_Symbol_Back[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]] != null) {
				int originX = 0;
				int originY = 0;
				int width = OpenTaiko.Tx.Couse_Symbol_Back[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].szTextureSize.Width;
				int height = OpenTaiko.Tx.Couse_Symbol_Back[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].szTextureSize.Height;

				if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
					originX = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[0];
					originY = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[1];
					width = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[2];
					height = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[3];
				} else if (OpenTaiko.ConfigIni.nPlayerCount > 2) {
					originX = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[0];
					originY = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[1];
					width = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[2];
					height = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[3];
				}

				OpenTaiko.Tx.Couse_Symbol_Back[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].t2D描画(
					couse_symbol_x,
					couse_symbol_y,
					new System.Drawing.RectangleF(originX, originY, width, height));
			}

			if (OpenTaiko.Tx.Couse_Symbol_Back_Flash[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]] != null && !OpenTaiko.ConfigIni.SimpleMode) {
				int originX = 0;
				int originY = 0;
				int width = OpenTaiko.Tx.Couse_Symbol_Back[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].szTextureSize.Width;
				int height = OpenTaiko.Tx.Couse_Symbol_Back[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].szTextureSize.Height;

				if (OpenTaiko.ConfigIni.nPlayerCount == 5) {
					originX = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[0];
					originY = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[1];
					width = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[2];
					height = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_5P[3];
				} else if (OpenTaiko.ConfigIni.nPlayerCount > 2) {
					originX = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[0];
					originY = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[1];
					width = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[2];
					height = OpenTaiko.Skin.Game_CourseSymbol_Back_Rect_4P[3];
				}

				OpenTaiko.Tx.Couse_Symbol_Back_Flash[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].Opacity = 255 - (int)((ctSymbolFlash[i].CurrentValue / 1000.0) * 255);
				OpenTaiko.Tx.Couse_Symbol_Back_Flash[OpenTaiko.stageSongSelect.nChoosenSongDifficulty[i]].t2D描画(
					couse_symbol_x,
					couse_symbol_y,
					new System.Drawing.RectangleF(originX, originY, width, height));
			}
		}
	}


	#region[ private ]
	//-----------------
	//構造体
	[StructLayout(LayoutKind.Sequential)]
	private struct STパッド状態 {
		public int n明るさ;
	}

	//太鼓
	private STパッド状態[] stパッド状態 = new STパッド状態[5 * 5];
	private long nフラッシュ制御タイマ;

	//private CTexture[] txコースシンボル = new CTexture[ 6 ];
	private string[] strCourseSymbolFileName;

	//オプション
	private CTexture txオプションパネル_HS;
	private CTexture txオプションパネル_RANMIR;
	private CTexture txオプションパネル_特殊;
	private int nHS;

	//譜面分岐
	private CCounter[] ctレベルアップダウン;
	public CTja.ECourse[] After;
	public CTja.ECourse[] Before;
	private CCounter[] ctSymbolFlash = new CCounter[5];
	//-----------------
	#endregion

}
