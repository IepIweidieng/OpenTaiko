using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenTaiko;

internal class CBoxDef : CSongListNodeInheritable {
	// Properties

	public Color? FontColor; // default: Color.Black (?) Color for texts not genre nor song title
	public CLocalizationData Title = new CLocalizationData();
	public CLocalizationData[] strBoxText = new CLocalizationData[3] { new CLocalizationData(), new CLocalizationData(), new CLocalizationData() };

	// Constructor

	public CBoxDef(string boxdefFileName) => this.tLoad(boxdefFileName);

	// メソッド
	private static readonly Regex KeyAndValueRegex =
		new Regex(@"^[ \t]*(#[A-Z0-9]+)(?::\s?|\s)(.+?)?$", RegexOptions.Compiled);

	public void tLoad(string boxdefFileName) {
		StreamReader reader = new StreamReader(boxdefFileName, Encoding.GetEncoding(OpenTaiko.sEncType));
		string str = null;
		while ((str = reader.ReadLine()) != null) {
			if (str.Length != 0) {
				try {
					var match = KeyAndValueRegex.Match(str);

					var key = match.Groups[1].Value;
					var argumentMatchGroup = match.Groups[2];
					var valueFull = argumentMatchGroup.Success ? argumentMatchGroup.Value : "";

					// For handling arguments ending in a ` `-or-`,`-containing string, use argumentFull

					//命令の最後に,が残ってしまっているときの対応
					var value = valueFull.TrimStart([' ', '\t']).TrimEnd([',', ' ', '\t']);

					if (match.Success) {
						if (value.IndexOf(';') != -1) {
							value = value.Substring(0, value.IndexOf(';'));
						}

						var split = str.Split(':');
						if (split.Length == 2) {
							if (key == "#TITLE") {
								this.Title.SetString("default", valueFull);
							} else if (key.StartsWith("#TITLE")) {
								string _lang = key.Substring(6).ToLowerInvariant();
								this.Title.SetString(_lang, valueFull);
							} else if (key == "#GENRE") {
								this.songGenre = valueFull;
							} else if (key == "#SELECTBG") {
								this.strSelectBGPath = value;
							} else if (key == "#FONTCOLOR") {
								this.FontColor = ColorTranslator.FromHtml(value);
							} else if (key == "#FORECOLOR") {
								this.ForeColor = ColorTranslator.FromHtml(value);
							} else if (key == "#BACKCOLOR") {
								this.BackColor = ColorTranslator.FromHtml(value);
							} else if (key == "#BOXCOLOR") {
								this.BoxColor = ColorTranslator.FromHtml(value);
							} else if (key == "#BGCOLOR") {
								this.BgColor = ColorTranslator.FromHtml(value);
							} else if (key == "#BGTYPE") {
								this.BgType = value;
							} else if (key == "#BOXTYPE") {
								this.BoxType = value;
							} else if (key == "#BOXCHARA") {
								this.BoxChara = value;
							} else if (key == "#SCENEPRESET") {
								this.strScenePresets = value;
							} else if (key == "#DEFAULTPREIMAGE") {
								this.Preimage = Path.Combine(Directory.GetParent(boxdefFileName).FullName, value);
							} else if (key == "#COMPAT") {
								this.Compat = CTja.strConvertTjaCompat(value);
							} else {
								for (int i = 0; i < 3; i++) {
									if (key == "#BOXEXPLANATION" + (i + 1).ToString()) {
										this.strBoxText[i].SetString("default", valueFull);
									} else if (key.StartsWith("#BOXEXPLANATION") && key.EndsWith((i + 1).ToString())) {
										string _lang = key.Substring(15)[..^1].ToLowerInvariant();
										this.strBoxText[i].SetString(_lang, valueFull);
									}
								}
							}
						}
					}
					continue;
				} catch (Exception e) {
					Trace.TraceError(e.ToString());
					Trace.TraceError("例外が発生しましたが処理を継続します。 (178a9a36-a59e-4264-8e4c-b3c3459db43c)");
					continue;
				}
			}
		}
		reader.Close();
	}
}
