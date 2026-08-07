namespace OpenTaiko {
	public class LuaActivityWrapper : LuaROActivityWrapper {
		// Used to search activities in the global activities dictionary from lua stages
		public static Dictionary<string, LuaActivityWrapper> _allLuaActivities = new Dictionary<string, LuaActivityWrapper>();

		// All global operations except querying is also performed on the global ROActivities dictionary
		#region [Setters]

		public static void ResetLuaActivityDictionary() {
			ResetROActivityDictionary();
			foreach (KeyValuePair<string, LuaActivityWrapper> _act in _allLuaActivities) {
				_act.Value.DisposeActivity();
			}
			_allLuaActivities.Clear();
		}

		#endregion

		#region [Getters]

		public static LuaActivityWrapper? GetLuaActivity(string name) {
			if (_allLuaActivities.TryGetValue(name, out var _act)) {
				return _act;
			}
			return null;
		}

		#endregion

		#region [Executers]

		public static new void PropagateAfterSongEnumEvent() {
			LuaROActivityWrapper.PropagateAfterSongEnumEvent();
			foreach (KeyValuePair<string, LuaActivityWrapper> _act in _allLuaActivities) {
				_act.Value.AfterSongsEnum();
			}
		}

		public static new void PropagateOnDestroy() {
			LuaROActivityWrapper.PropagateOnDestroy();
			foreach (KeyValuePair<string, LuaActivityWrapper> _act in _allLuaActivities) {
				_act.Value.OnDestroy();
			}
		}

		#endregion

		public LuaActivityWrapper(string name, bool isGlobal = false) {
			if (isGlobal == false) lcActScript = new CLuaActivityScript(CSkin.Path($"Modules/Activities/{name}"), name);
			else lcActScript = new CLuaActivityScript(CSkin.Path($"Global/Activities/{name}"), $"[GLOBAL]{name}");

			_allLuaActivities.Add(name, this);

		}
		}

	public class LuaActivityFunc : LuaROActivityFunc {
		public LuaActivityWrapper? GetActivity(string name) {
			return LuaActivityWrapper.GetLuaActivity(name);
		}

	}
}
