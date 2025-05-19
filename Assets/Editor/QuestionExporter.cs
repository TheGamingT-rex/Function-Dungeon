using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class QuestionExporter : EditorWindow {
    [MenuItem("Utilities/Export Questions")]
    public static void ShowWindow() {
        GetWindow<QuestionExporter>("Export questions");
    }

    QuestionList list;
    private void OnGUI() {
        list = (QuestionList)EditorGUILayout.ObjectField("Label:", list, typeof(QuestionList), false);

        EditorGUI.BeginDisabledGroup(list == null);
        if (GUILayout.Button("Export")) Export();
        EditorGUI.EndDisabledGroup();
    }

    private void Export() {
        string mdPath = EditorUtility.SaveFilePanel(title: "Save questions as Markdown", directory: Application.dataPath, defaultName: "questions.md", extension: "md");

        if (string.IsNullOrEmpty(mdPath)) return;

        string imageFolder = Path.Combine(Path.GetDirectoryName(mdPath)!, "images");
        Directory.CreateDirectory(imageFolder);

        StringBuilder md = new StringBuilder(8_192);
        md.AppendLine("# Question export").AppendLine();

        foreach (Question q in list.questions) {
            if (!q.enabled) continue;
            if (q.type == QuestionType.CUSTOM) continue;
            md.AppendLine($"## {Escape(q.name)} (ID {q.uniqueIdentifier})").AppendLine();

            //md.AppendLine($"* **Enabled:** {q.enabled}");
            //md.AppendLine($"* **Type:** {q.type}");
            //md.AppendLine($"* **Variation:** {Escape(q.variation)}");
            md.AppendLine($"* **Learning-goal level:** {Constants.learningGoalLevels[q.learningGoalLevel]}").AppendLine();

            foreach (QuestionText t in q.text) {
                string localeCode = t.locale != null ? t.locale.Identifier.Code : "unknown";
                md.AppendLine($"### Locale `{localeCode}`").AppendLine();
                md.AppendLine(Escape(t.question)).AppendLine();
                md.AppendLine($"- **Correct:** {Escape(t.correct)}");
                md.AppendLine($"- **Wrong 1:** {Escape(t.wrong1)}");
                md.AppendLine($"- **Wrong 2:** {Escape(t.wrong2)}");
                md.AppendLine($"- **Wrong 3:** {Escape(t.wrong3)}").AppendLine();
                md.AppendLine($"_Feedback:_ {Escape(t.feedback)}").AppendLine();
            }

            if (q.image != null) {
                string pngName = $"{Sanitize(q.name)}_{q.uniqueIdentifier}.png";
                string pngPath = Path.Combine(imageFolder, pngName);
                SaveSpriteAsPng(q.image, pngPath);

                md.AppendLine($"![{Escape(q.name)}](images/{pngName})").AppendLine();
            }

            md.AppendLine("---").AppendLine();
        }

        File.WriteAllText(mdPath, md.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();
        Debug.Log($"✅ Exported {list.questions.Count} questions to:\n{mdPath}");
    }

    private static void SaveSpriteAsPng(Sprite sprite, string path) {
        Texture2D src = sprite.texture;
        Rect r = sprite.rect;
        Texture2D tex;

        if (r.width != src.width || r.height != src.height) {
            tex = new Texture2D((int)r.width, (int)r.height, TextureFormat.RGBA32, false);
            tex.SetPixels(src.GetPixels((int)r.x, (int)r.y, (int)r.width, (int)r.height));
            tex.Apply();
        } else {
            tex = UnityEngine.Object.Instantiate(src);   // avoid overwriting read-only asset
        }

        File.WriteAllBytes(path, tex.EncodeToPNG());
        UnityEngine.Object.DestroyImmediate(tex);
    }

    private static string Sanitize(string fileName) {
        foreach (char c in Path.GetInvalidFileNameChars())
            fileName = fileName.Replace(c, '_');
        return fileName;
    }

    private static string Escape(string s) =>
        string.IsNullOrEmpty(s)
            ? ""
            : s.Replace("\\", "\\\\")
               .Replace("`", "\\`")
               .Replace("*", "\\*")
               .Replace("_", "\\_")
               .Replace("[", "\\[")
               .Replace("]", "\\]")
               .Replace("<", "\\<")
               .Replace(">", "\\>");
}