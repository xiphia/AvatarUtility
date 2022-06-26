using System;
using System.Collections;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using static System.String;

namespace XiPHiA.Avatar.Utility.Editor
{
    public class ScreenShot : UnityEditor.Editor
    {
        private static class Types
        {
            public static readonly Type GameView = Type.GetType("UnityEditor.GameView,UnityEditor");
        }

        private static EditorWindow GetGameView()
        {
            return EditorWindow.GetWindow(Types.GameView);
        }

        private static Vector2 GetGameViewSize()
        {
            var getSizeOfMainGameView = Types.GameView.GetMethod("GetSizeOfMainGameView", BindingFlags.NonPublic | BindingFlags.Static);
            if (getSizeOfMainGameView == null) return Vector2.zero;
            return (Vector2) getSizeOfMainGameView.Invoke(null, null);
        }

        private static void TakeScreenshot(bool transparent)
        {
            var prefix = transparent ? "Transparent" : "Screenshot";
            var size = GetGameViewSize();
            var resolution = size.x + "x" + size.y;
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss.fff");
            var filename = Join("_", prefix, resolution, timestamp) + ".png";
            if (transparent)
            {
                var camera = Camera.main;
                if (camera == null) return;
                var currentCameraTargetTexture = camera.targetTexture;
                var currentCameraClearFlags = camera.clearFlags;
                var currentCameraBackgroundColor = camera.backgroundColor;
                var currentRenderTextureActive = RenderTexture.active;
                var width = (int)size.x;
                var height = (int)size.y;

                var textureTransparent = new Texture2D(width, height, TextureFormat.ARGB32, false);
                var renderTextureTransparent = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
                var grabArea = new Rect(0, 0, width, height);

                RenderTexture.active = renderTextureTransparent;
                camera.targetTexture = renderTextureTransparent;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.clear;
                camera.Render();
                textureTransparent.ReadPixels(grabArea, 0, 0);
                textureTransparent.Apply();

                var data = textureTransparent.EncodeToPNG();
                File.WriteAllBytes(filename, data);

                camera.clearFlags = currentCameraClearFlags;
                camera.targetTexture = currentCameraTargetTexture;
                camera.backgroundColor = currentCameraBackgroundColor;
                RenderTexture.active = currentRenderTextureActive;
                RenderTexture.ReleaseTemporary(renderTextureTransparent);
                DestroyImmediate(textureTransparent);
            }
            else
            {
                ScreenCapture.CaptureScreenshot(filename);
                GetGameView().Repaint();
            }
            Debug.Log($"Screenshot has been saved as \"{filename}\".");
        }

        [MenuItem("AvatarUtil/Screenshot #F12")]
        private static void Screenshot()
        {
            TakeScreenshot(false);
        }

        [MenuItem("AvatarUtil/Screenshot(Transparent) #&F12")]
        private static void ScreenshotTransparent() {

            TakeScreenshot(true);
        }
    }
}
