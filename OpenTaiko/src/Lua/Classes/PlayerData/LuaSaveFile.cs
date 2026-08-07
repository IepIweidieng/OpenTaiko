namespace OpenTaiko {
	/// <summary>
	/// Writable derivation of <see cref="LuaROSaveFile"/>.
	/// </summary>
	internal class LuaSaveFile : LuaROSaveFile {
		#region [Coins]

		public override void SpendCoins(long price) {
			_sf.data.Medals = Math.Max(0, _sf.data.Medals - price);
			DBSaves.AlterCoinsAndTotalPlayCount(_sf.data.SaveId, -price, 0);
		}

		public override void EarnCoins(long amount) {
			_sf.data.Medals += amount;
			_sf.data.TotalEarnedMedals += amount;
			DBSaves.AlterCoinsAndTotalPlayCount(_sf.data.SaveId, amount, 0);
		}

		#endregion

		#region [Unlockables]

		public override void UnlockNameplate(int id) {
			if (!IsNameplateUnlocked(id)) {
				_sf.data.UnlockedNameplateIds.Add(id);
				DBSaves.RegisterUnlockedNameplate(_sf.data.SaveId, id);
			}
		}

		public override void UnlockSong(string uniqueId) {
			if (!IsSongUnlocked(uniqueId)) {
				_sf.data.UnlockedSongs.Add(uniqueId);
				DBSaves.RegisterStringUnlockedAsset(_sf.data.SaveId, "unlocked_songs", uniqueId);
			}
		}

		#endregion

		#region [Hitsounds]

		/// <summary>The folder name of this player's selected hitsound set (e.g. "Taiko").</summary>
		public override string SelectedHitsounds {
			get => base.SelectedHitsounds;
			set {
				if (_sf.data.SelectedHitsounds == value) return;
				_sf.data.SelectedHitsounds = value;
				DBSaves.SetSelectedHitsounds(_sf.data.SaveId, value);

				// Apply immediately if the hitsounds are loaded
				var hs = OpenTaiko.Skin.hsHitSoundsInformations;
				if (hs != null) {
					int idx = hs.GetIndexByFolderName(value);
					hs.tReloadHitSounds(idx, _mounted);
				}
			}
		}

		#endregion

		#region [Triggers and Counters]

		public override void SetGlobalTrigger(string triggerName, bool triggerValue) {
			_sf.tSetGlobalTrigger(triggerName, triggerValue);
		}

		public override void SetGlobalCounter(string counterName, double counterValue) {
			_sf.tSetGlobalCounter(counterName, counterValue);
		}

		#endregion

		#region [Characters and Puchis]

		/// <summary>
		/// Changes the player's character to the one with the given directory name.
		/// Returns false without making any change if the character name is not found.
		/// Returns true immediately (no-op) if the character is already set.
		/// </summary>
		public override bool ChangeCharacter(string name) {
			int newIdx = Array.FindIndex(OpenTaiko.Tx.Characters, c => c.dirName == name);
			if (newIdx < 0) return false;
			int oldIdx = _sf.data.Character;
			if (oldIdx == newIdx) return true;
			OpenTaiko.Tx.ReloadCharacter(oldIdx, newIdx, _mounted);
			_sf.data.Character = newIdx;
			_sf.tUpdateCharacterName(OpenTaiko.Tx.Characters[newIdx].dirName);
			_sf.tApplyHeyaChanges();
			return true;
		}

		public override void UnlockPuchichara(string folderName) {
			if (!IsPuchicharaUnlocked(folderName)) {
				_sf.data.UnlockedPuchicharas.Add(folderName);
				DBSaves.RegisterStringUnlockedAsset(_sf.data.SaveId, "unlocked_puchicharas", folderName);
				_sf.tApplyHeyaChanges();
			}
		}

		public override void ChangePuchichara(string folderName) {
			if (_sf.data.PuchiChara == folderName) return;
			_sf.data.PuchiChara = folderName;
			_sf.tApplyHeyaChanges();
		}

		public override void UnlockCharacter(string dirName) {
			if (!IsCharacterUnlocked(dirName)) {
				_sf.data.UnlockedCharacters.Add(dirName);
				DBSaves.RegisterStringUnlockedAsset(_sf.data.SaveId, "unlocked_characters", dirName);
				_sf.tApplyHeyaChanges();
			}
		}

		// ── Dan title ──────────────────────────────────────────────────────────────

		/// <summary>Sets the player's active dan title and persists the change.</summary>
		public override void ChangeDan(string title) {
			bool isGold = false;
			int  cs     = 0;
			if (_sf.data.DanTitles != null && _sf.data.DanTitles.TryGetValue(title, out var dt)) {
				isGold = dt.isGold;
				cs     = dt.clearStatus;
			}
			_sf.data.Dan     = title;
			_sf.data.DanGold = isGold;
			_sf.data.DanType = cs;
			OpenTaiko.NamePlate?.tNamePlateRefreshTitles(_mounted);
			_sf.tApplyHeyaChanges();
		}

		// ── Player name ────────────────────────────────────────────────────────────

		/// <summary>Changes the player's displayed name and persists the change.</summary>
		public override void ChangeName(string name) {
			if (string.IsNullOrEmpty(name) || _sf.data.Name == name) return;
			_sf.data.Name = name;
			OpenTaiko.NamePlate?.tNamePlateRefreshTitles(_mounted);
			_sf.tApplyHeyaChanges();
		}

		public override void ChangeNameplate(int id) {
			if (_sf.data.TitleId == id) return;
			_sf.data.TitleId = id;
			if (OpenTaiko.Databases.DBNameplateUnlockables.data.TryGetValue((Int64)id, out var nameplate)) {
				_sf.data.Title = nameplate.nameplateInfo.cld.GetString("");
				_sf.data.TitleType = nameplate.nameplateInfo.iType;
				_sf.data.TitleRarityInt = HRarity.tRarityToLangInt(nameplate.rarity);
			} else {
				_sf.data.Title = "";
				_sf.data.TitleType = 0;
				_sf.data.TitleRarityInt = 1;
			}
			OpenTaiko.NamePlate?.tNamePlateRefreshTitles(_mounted);
			_sf.tApplyHeyaChanges();
		}

		#endregion

		public LuaSaveFile(SaveFile sf, int mountedPlayer) : base(sf, mountedPlayer) { }
	}
}
