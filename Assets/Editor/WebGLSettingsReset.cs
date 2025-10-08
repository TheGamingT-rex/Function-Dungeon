using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WebGLSettingsReset : MonoBehaviour {
    [MenuItem("Tools/Reset WebGL Settings")]
    public static void ResetWebGLSettings() {
        PlayerSettings.WebGL.emscriptenArgs = string.Empty;
        PlayerSettings.WebGL.memorySize = 256;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        EditorApplication.RepaintHierarchyWindow();

        UnityEngine.Debug.Log("WebGL settings reset to default.");
    }
}