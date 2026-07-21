namespace OpenTaiko {
	/// <summary>
	/// Read-only base of <see cref="LuaConfigIniFunc"/> for use in <see cref="LuaROActivityWrapper"/> scripts.
	/// Any attempt to call a setter or mutating method logs an error and does nothing.
	/// </summary>
	public class LuaROConfigIniFunc {
		private static void BlockWrite(string member) {
			LogNotification.PopError($"[ROActivity] 'CONFIG.{member}' is a write operation and is not allowed in a read-only module.");
		}

		public LuaROConfigIniFunc AsReadOnly() => new();

		public bool ConfigIsNew {
			get => OpenTaiko.ConfigIsNew;
		}

		#region [General variables]

		// No setter for the Language for now, no reason to use it outside the first boot screen and the settings for the moment
		public string Language {
			get => OpenTaiko.ConfigIni.sLang;
		}

		public virtual int PlayerCount {
			get => OpenTaiko.ConfigIni.nPlayerCount;
			set => BlockWrite(nameof(PlayerCount));
		}

		public virtual bool IsAIBattleMode {
			get => OpenTaiko.ConfigIni.bAIBattleMode;
			set => BlockWrite(nameof(IsAIBattleMode));
		}

		public virtual int AILevel {
			get => OpenTaiko.ConfigIni.nAILevel;
			set => BlockWrite(nameof(AILevel));
		}

		public virtual bool IsTrainingMode {
			get => OpenTaiko.ConfigIni.bTokkunMode;
			set => BlockWrite(nameof(IsTrainingMode));
		}

		public virtual bool UseModernScoringMethod {
			get => OpenTaiko.ConfigIni.ShinuchiMode;
			set => BlockWrite(nameof(UseModernScoringMethod));
		}

		public virtual int UsedLegacyScoringMethod {
			get => OpenTaiko.ConfigIni.nScoreMode;
			set => BlockWrite(nameof(UsedLegacyScoringMethod));
		}

		public int GetGameType(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return (int)EGameType.Taiko;
			return (int)OpenTaiko.ConfigIni.nGameType[player];
		}

		public virtual void SetGameType(int player, int gt) => BlockWrite(nameof(SetGameType));

		public int GetDefaultCourse(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return (int)Difficulty.Normal;
			return OpenTaiko.ConfigIni.nDefaultCourse;
		}
		public virtual void SetDefaultCourse(int player, int diff) => BlockWrite(nameof(SetDefaultCourse));

		// There might be some funny usages of this
		public bool AreSongUnlockablesDisabled {
			get => OpenTaiko.ConfigIni.bIgnoreSongUnlockables;
		}
		#endregion

		#region [Gameplay mods]

		public virtual int SongSpeed {
			get => OpenTaiko.ConfigIni.nSongSpeed;
			set => BlockWrite(nameof(SongSpeed));
		}

		public int GetScrollSpeed(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return 9;
			return OpenTaiko.ConfigIni.nScrollSpeed[player];
		}
		public virtual void SetScrollSpeed(int player, int speed) => BlockWrite(nameof(SetScrollSpeed));

		public int GetTimingZone(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return 2;
			return OpenTaiko.ConfigIni.nTimingZones[player];
		}
		public virtual void SetTimingZone(int player, int zone) => BlockWrite(nameof(SetTimingZone));

		public bool GetAutoStatus(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return false;
			// replay playback shows the auto modicon too (even though it isn't auto-judging)
			return OpenTaiko.ConfigIni.bAutoPlay[player] || OpenTaiko.bReplayMode[player];
		}
		public virtual void SetAutoStatus(int player, bool isAuto) => BlockWrite(nameof(SetAutoStatus));

		public int GetRandomMod(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return (int)ERandomMode.Off;
			return (int)OpenTaiko.ConfigIni.eRandom[player];
		}
		public virtual void SetRandomMod(int player, int mode) => BlockWrite(nameof(SetRandomMod));

		public int GetFunMod(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return (int)EFunMods.None;
			return (int)OpenTaiko.ConfigIni.nFunMods[player];
		}
		public virtual void SetFunMod(int player, int mod) => BlockWrite(nameof(SetFunMod));

		public int GetStealthMod(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return (int)EStealthMode.Off;
			return (int)OpenTaiko.ConfigIni.eSTEALTH[player];
		}
		public virtual void SetStealthMod(int player, int mode) => BlockWrite(nameof(SetStealthMod));

		public int GetJusticeMod(int player) {
			if (player < 0 || player >= OpenTaiko.MAX_PLAYERS) return 0;
			return OpenTaiko.ConfigIni.bJust[player];
		}

		public Int64 GetModFlags(int player) {
			byte[] _flags = new byte[8] { 0, 0, 0, 0, 0, 0, 0, 0 };

			_flags[0] = (byte)Math.Min(255, GetScrollSpeed(player));
			_flags[1] = (byte)GetStealthMod(player);
			_flags[2] = (byte)GetRandomMod(player);
			_flags[3] = (byte)Math.Min(255, SongSpeed);
			_flags[4] = (byte)GetTimingZone(player);
			_flags[5] = (byte)GetJusticeMod(player);
			_flags[7] = (byte)GetFunMod(player);

			return BitConverter.ToInt64(_flags, 0);
		}
		public virtual void SetJusticeMod(int player, int mode) => BlockWrite(nameof(SetJusticeMod));
		public virtual void SetModFlags(int player, long flags) => BlockWrite(nameof(SetModFlags));

		#endregion

		#region [Volume]

		public virtual int MasterVolume {
			get => OpenTaiko.ConfigIni.MasterLevel;
			set => BlockWrite(nameof(MasterVolume));
		}

		public virtual int SoundEffectVolume {
			get => OpenTaiko.ConfigIni.SoundEffectLevel;
			set => BlockWrite(nameof(SoundEffectVolume));
		}

		public virtual int VoiceVolume {
			get => OpenTaiko.ConfigIni.VoiceLevel;
			set => BlockWrite(nameof(VoiceVolume));
		}

		public virtual int SongVolume {
			get => OpenTaiko.ConfigIni.SongPlaybackLevel;
			set => BlockWrite(nameof(SongVolume));
		}

		public virtual int PreviewVolume {
			get => OpenTaiko.ConfigIni.SongPreviewLevel;
			set => BlockWrite(nameof(PreviewVolume));
		}

		#endregion
	}
}
