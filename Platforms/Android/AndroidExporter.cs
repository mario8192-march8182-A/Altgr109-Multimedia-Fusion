using System;
using System.IO;
using AkpProjectFormat;

namespace AkpEngine.Platforms.Android
{
    /// <summary>
    /// Exportador para plataforma Android
    /// </summary>
    public class AndroidExporter
    {
        private string _projectPath;
        private string _outputPath;

        public AndroidExporter(string projectPath, string outputPath)
        {
            _projectPath = projectPath;
            _outputPath = outputPath;
        }

        /// <summary>
        /// Exporta o projeto para Android
        /// </summary>
        public void Export(AkpProject project)
        {
            Console.WriteLine($"[AndroidExporter] Exportando para Android: {project.Name}");
            Console.WriteLine($"[AndroidExporter] Output: {_outputPath}");

            try
            {
                Directory.CreateDirectory(_outputPath);
                Directory.CreateDirectory(Path.Combine(_outputPath, "Assets"));
                Directory.CreateDirectory(Path.Combine(_outputPath, "src"));

                // Copiar assets
                CopyAssets(project);

                // Gerar AndroidManifest.xml
                GenerateAndroidManifest(project);

                // Gerar code
                GenerateJavaStubs(project);

                Console.WriteLine($"[AndroidExporter] Exportação completa!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AndroidExporter] Erro na exportação: {ex.Message}");
            }
        }

        private void CopyAssets(AkpProject project)
        {
            Console.WriteLine($"[AndroidExporter] Copiando assets...");
            foreach (var asset in project.Assets)
            {
                string sourcePath = Path.Combine(_projectPath, asset.Value);
                string destPath = Path.Combine(_outputPath, "Assets", Path.GetFileName(asset.Value));

                if (File.Exists(sourcePath))
                {
                    File.Copy(sourcePath, destPath, true);
                }
            }
        }

        private void GenerateAndroidManifest(AkpProject project)
        {
            string manifest = $@"<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android"
    package="com.akpengine.{project.Name.ToLower()}"
    android:versionCode="1"
    android:versionName="{project.Version}">

    <uses-sdk
        android:minSdkVersion="21"
        android:targetSdkVersion="33" />

    <application
        android:label="@string/app_name"
        android:icon="@drawable/ic_launcher">
        
        <activity
            android:name=".MainActivity"
            android:label="{project.Name}">
            <intent-filter>
                <action android:name="android.intent.action.MAIN" />
                <category android:name="android.intent.category.LAUNCHER" />
            </intent-filter>
        </activity>
    </application>
</manifest>";

            File.WriteAllText(Path.Combine(_outputPath, "AndroidManifest.xml"), manifest);
        }

        private void GenerateJavaStubs(AkpProject project)
        {
            string javaCode = $@"package com.akpengine.{project.Name.ToLower()};

import android.app.Activity;
import android.os.Bundle;
import android.view.SurfaceView;

public class MainActivity extends Activity {{
    private AkpGameView gameView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {{
        super.onCreate(savedInstanceState);
        gameView = new AkpGameView(this);
        setContentView(gameView);
    }}

    @Override
    protected void onPause() {{
        super.onPause();
        gameView.pause();
    }}

    @Override
    protected void onResume() {{
        super.onResume();
        gameView.resume();
    }}
}}";

            string srcPath = Path.Combine(_outputPath, "src", "MainActivity.java");
            File.WriteAllText(srcPath, javaCode);
        }
    }
}
