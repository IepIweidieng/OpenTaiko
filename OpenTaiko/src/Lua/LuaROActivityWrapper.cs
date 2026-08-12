namespace OpenTaiko {
	/// <summary>
	/// Lua-facing accessor for looking up ROActivities by name.
	/// Exposed as the <c>ROACTIVITY</c> global in all Lua scripts.
	/// These methods are also accessible in the <c>ACTIVITY</c> global in writable Lua scripts.
	/// </summary>
	public class LuaROActivityFunc {
		public LuaROActivityWrapper? GetROActivity(string name) =>
			LuaROActivityWrapper.GetROActivity(name);
	}

	/// <summary>
	/// The public constructor wraps a <see cref="CLuaROActivityScript"/> loaded from <c>Modules/ROActivities/{name}/Script.lua</c>.
	/// Scripts in ROActivities receive read-only views of CONFIG and GetSaveFile — any attempt
	/// to write through those globals produces an error rather than modifying game state.
	/// </summary>
	public class LuaROActivityWrapper {
		public static Dictionary<string, LuaROActivityWrapper> _allROActivities = new Dictionary<string, LuaROActivityWrapper>();

		#region [Static management]

		public static void ResetROActivityDictionary() {
			foreach (var pair in _allROActivities)
				pair.Value.DisposeActivity();
			_allROActivities.Clear();
		}

		public static LuaROActivityWrapper? GetROActivity(string name) {
			_allROActivities.TryGetValue(name, out var act);
			return act;
		}

		public static void PropagateAfterSongEnumEvent() {
			foreach (var pair in _allROActivities)
				pair.Value.AfterSongsEnum();
		}


		public static void PropagateOnDestroy() {
			foreach (var pair in _allROActivities)
				pair.Value.OnDestroy();
		}

		#endregion

		protected CLuaActivityScriptBase lcActScript;

		public void DisposeActivity() {
			lcActScript?.Dispose();
		}

		public LuaROActivityWrapper(string name) {
			lcActScript = new CLuaROActivityScript(CSkin.Path($"Modules/ROActivities/{name}"), name);
			_allROActivities[name] = this;
		}

		protected LuaROActivityWrapper() { } // initialized by derived class

		#region [Standard lifecycle events]

		public bool IsActive => lcActScript?.IsActive() ?? false;

		public object[]? Activate(params object[] args) => lcActScript?.Activate(args);
		public object[]? Deactivate(params object[] args) => lcActScript?.Deactivate(args);
		public object[]? Draw(params object[] args) => lcActScript?.Draw(args);
		public object[]? Update(params object[] args) => lcActScript?.Update(FDK.Game.TimeMs, args);

		#endregion

		#region [Generic Lua function call]

		/// <summary>
		/// Calls a named Lua function defined in this (RO)Activity's script.
		/// </summary>
		public object[]? Call(string functionName, params object[] args) => lcActScript?.CallFunction(functionName, args);

		#endregion

		#region [Events not present on CStage/CActivity]

		// Incremental onStart (see LuaStageWrapper) — drives a yielding onStart across frames with the bar.
		internal void BeginOnStart() => lcActScript?.BeginOnStart();
		internal bool StepOnStart(out float progress) {
			if (lcActScript == null) { progress = 0f; return false; }
			return lcActScript.StepOnStart(out progress);
		}

		// Executes everytime songs enum is done, including soft/hard reload and at start
		protected void AfterSongsEnum() => lcActScript?.AfterSongsEnum();

		// Executes before skin change, in order to deallocate any ressources carried by the skin's Lua modules
		protected void OnDestroy() => lcActScript?.OnDestroy();

		#endregion

	}
}
