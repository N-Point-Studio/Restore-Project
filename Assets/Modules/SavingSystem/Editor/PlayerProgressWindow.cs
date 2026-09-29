using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace Modules.SavingSystems.Editor
{
    public class PlayerProgressWindow : EditorWindow
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "saves");
        private static string BackupPath => Path.Combine(SavePath, "backups");
        private Vector2 scrollPosition;
        private bool showAllFiles = false;
        private string searchFilter = "";
        private bool showFileDetails = true;

        [MenuItem("Window/Game/Player Progress")]
        public static void ShowWindow()
        {
            GetWindow<PlayerProgressWindow>("Player Progress");
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Header
            EditorGUILayout.LabelField("Player Save Data Manager", EditorStyles.largeLabel);
            EditorGUILayout.LabelField($"Save Path: {SavePath}", EditorStyles.miniLabel);
            EditorGUILayout.Space(10);

            // Quick actions
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset All Progress", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Reset Progress",
                    "Are you sure you want to delete all save data? This cannot be undone.",
                    "Reset", "Cancel"))
                {
                    DeleteAllSaves();
                }
            }

            if (GUILayout.Button("Open Save Folder", GUILayout.Height(30)))
            {
                OpenSaveFolder();
            }

            if (GUILayout.Button("Create Backup", GUILayout.Height(30)))
            {
                CreateBackup();
            }

            if (GUILayout.Button("Restore Backup", GUILayout.Height(30)))
            {
                ShowRestoreBackupMenu();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            DrawSeparator();

            // Filters and options
            EditorGUILayout.BeginHorizontal();
            showAllFiles = EditorGUILayout.Toggle("Show All Save Files", showAllFiles);
            showFileDetails = EditorGUILayout.Toggle("Show File Details", showFileDetails);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(50));
            searchFilter = EditorGUILayout.TextField(searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
            {
                searchFilter = "";
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            DrawSeparator();

            // Save files browser
            EditorGUILayout.LabelField("Saved Data Files", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            if (Directory.Exists(SavePath))
            {
                var files = Directory.GetFiles(SavePath, "*.dat", SearchOption.AllDirectories)
                    .Where(file => showAllFiles || Path.GetFileNameWithoutExtension(file).Contains("PlayerProgress"))
                    .Where(file => string.IsNullOrEmpty(searchFilter) ||
                                   Path.GetFileName(file).ToLower().Contains(searchFilter.ToLower()))
                    .OrderBy(file => Path.GetFileName(file))
                    .ToArray();

                if (files.Length == 0)
                {
                    EditorGUILayout.HelpBox("No save data files match the current filter.", MessageType.Info);
                }
                else
                {
                    foreach (var file in files)
                    {
                        DrawSaveFileEntry(file);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No save data found. Save folder doesn't exist yet.", MessageType.Info);
            }
            EditorGUILayout.EndScrollView();
        }

        private void ViewSaveFile(string path)
        {
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var parsed = SimpleJSON.JSON.Parse(json);

            var viewer = CreateInstance<SaveDataViewer>();
            viewer.Initialize(Path.GetFileName(path), parsed);
            viewer.ShowUtility();
        }

        private void DeleteAllSaves()
        {
            if (Directory.Exists(SavePath))
            {
                Directory.Delete(SavePath, true);
                Debug.Log("All save data deleted.");
            }
        }

        private void DrawSeparator()
        {
            EditorGUILayout.Space(5);
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Space(5);
        }

        private void DrawSaveFileEntry(string file)
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.BeginHorizontal();

            // File icon and name
            EditorGUILayout.LabelField(EditorGUIUtility.IconContent("TextAsset Icon"), GUILayout.Width(20));
            EditorGUILayout.LabelField(Path.GetFileName(file), EditorStyles.boldLabel);

            // File actions
            if (GUILayout.Button("View", GUILayout.Width(60)))
            {
                ViewSaveFile(file);
            }
            if (GUILayout.Button("Edit", GUILayout.Width(60)))
            {
                EditSaveFile(file);
            }
            if (GUILayout.Button("Backup", GUILayout.Width(60)))
            {
                CreateBackupOfFile(file);
            }
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                if (EditorUtility.DisplayDialog("Delete Save File",
                    $"Delete {Path.GetFileName(file)}?",
                    "Delete", "Cancel"))
                {
                    File.Delete(file);
                    Debug.Log($"Deleted save file: {Path.GetFileName(file)}");
                }
            }

            EditorGUILayout.EndHorizontal();

            // File details
            if (showFileDetails)
            {
                var fileInfo = new FileInfo(file);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Size: {FormatFileSize(fileInfo.Length)}", EditorStyles.miniLabel, GUILayout.Width(80));
                EditorGUILayout.LabelField($"Modified: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm}", EditorStyles.miniLabel);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        private void OpenSaveFolder()
        {
            if (Directory.Exists(SavePath))
            {
                EditorUtility.RevealInFinder(SavePath);
            }
            else
            {
                EditorUtility.DisplayDialog("Folder Not Found",
                    "Save folder doesn't exist yet. Play the game to generate save data first.",
                    "OK");
            }
        }

        private void CreateBackup()
        {
            if (!Directory.Exists(SavePath))
            {
                EditorUtility.DisplayDialog("No Save Data", "No save data found to backup.", "OK");
                return;
            }

            var backupFolderName = $"backup_{System.DateTime.Now:yyyyMMdd_HHmmss}";
            var backupFullPath = Path.Combine(BackupPath, backupFolderName);

            try
            {
                if (!Directory.Exists(BackupPath))
                    Directory.CreateDirectory(BackupPath);

                Directory.CreateDirectory(backupFullPath);

                var files = Directory.GetFiles(SavePath, "*.dat");
                foreach (var file in files)
                {
                    var fileName = Path.GetFileName(file);
                    var destinationPath = Path.Combine(backupFullPath, fileName);
                    File.Copy(file, destinationPath);
                }

                Debug.Log($"Backup created: {backupFolderName}");
                EditorUtility.DisplayDialog("Backup Created",
                    $"Backup created successfully: {backupFolderName}", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to create backup: {ex.Message}");
                EditorUtility.DisplayDialog("Backup Failed",
                    $"Failed to create backup: {ex.Message}", "OK");
            }
        }

        private void CreateBackupOfFile(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var backupFileName = $"{fileName}_backup_{System.DateTime.Now:yyyyMMdd_HHmmss}.dat";
            var backupFilePath = Path.Combine(SavePath, backupFileName);

            try
            {
                File.Copy(filePath, backupFilePath);
                Debug.Log($"File backup created: {backupFileName}");
                EditorUtility.DisplayDialog("Backup Created",
                    $"File backup created: {backupFileName}", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to backup file: {ex.Message}");
                EditorUtility.DisplayDialog("Backup Failed",
                    $"Failed to backup file: {ex.Message}", "OK");
            }
        }

        private void ShowRestoreBackupMenu()
        {
            if (!Directory.Exists(BackupPath))
            {
                EditorUtility.DisplayDialog("No Backups", "No backups found.", "OK");
                return;
            }

            var backupFolders = Directory.GetDirectories(BackupPath)
                .Select(Path.GetFileName)
                .OrderByDescending(name => name)
                .ToArray();

            if (backupFolders.Length == 0)
            {
                EditorUtility.DisplayDialog("No Backups", "No backups found.", "OK");
                return;
            }

            var selection = EditorUtility.DisplayDialogComplex("Restore Backup",
                "Select a backup to restore:",
                "Cancel", backupFolders[0], backupFolders.Length > 1 ? backupFolders[1] : "");

            if (selection == 1) // First backup
            {
                RestoreBackup(backupFolders[0]);
            }
            else if (selection == 2 && backupFolders.Length > 1) // Second backup
            {
                RestoreBackup(backupFolders[1]);
            }
        }

        private void RestoreBackup(string backupFolderName)
        {
            var backupFullPath = Path.Combine(BackupPath, backupFolderName);

            if (EditorUtility.DisplayDialog("Restore Backup",
                $"This will overwrite current save data with backup: {backupFolderName}. Continue?",
                "Restore", "Cancel"))
            {
                try
                {
                    // Create save directory if it doesn't exist
                    if (!Directory.Exists(SavePath))
                        Directory.CreateDirectory(SavePath);

                    // Copy all files from backup
                    var backupFiles = Directory.GetFiles(backupFullPath, "*.dat");
                    foreach (var backupFile in backupFiles)
                    {
                        var fileName = Path.GetFileName(backupFile);
                        var destinationPath = Path.Combine(SavePath, fileName);
                        File.Copy(backupFile, destinationPath, true);
                    }

                    Debug.Log($"Restored backup: {backupFolderName}");
                    EditorUtility.DisplayDialog("Restore Complete",
                        $"Backup restored successfully: {backupFolderName}", "OK");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to restore backup: {ex.Message}");
                    EditorUtility.DisplayDialog("Restore Failed",
                        $"Failed to restore backup: {ex.Message}", "OK");
                }
            }
        }

        private void EditSaveFile(string path)
        {
            if (!File.Exists(path)) return;

            var json = File.ReadAllText(path);
            var parsed = SimpleJSON.JSON.Parse(json);

            var editor = CreateInstance<SaveDataEditor>();
            editor.Initialize(path, parsed);
            editor.ShowUtility();
        }

        private string FormatFileSize(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            return $"{bytes / (1024 * 1024)} MB";
        }
    }

    public class SaveDataViewer : EditorWindow
    {
        private string fileName;
        private string prettyJson;
        private Vector2 scrollPosition;

        public void Initialize(string fileName, SimpleJSON.JSONNode data)
        {
            this.fileName = fileName;
            this.prettyJson = data.ToString(4); // 4 spaces indent
            titleContent = new GUIContent($"Save Data: {fileName}");
            minSize = new Vector2(300, 200);
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"File: {fileName}", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.TextArea(prettyJson, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }
    }

    public class SaveDataEditor : EditorWindow
    {
        private string filePath;
        private string fileName;
        private string jsonContent;
        private string originalJsonContent;
        private Vector2 scrollPosition;
        private bool hasChanges;
        private GUIStyle textAreaStyle;

        public void Initialize(string filePath, SimpleJSON.JSONNode data)
        {
            this.filePath = filePath;
            this.fileName = Path.GetFileName(filePath);
            this.jsonContent = data.ToString(4); // 4 spaces indent
            this.originalJsonContent = this.jsonContent;
            this.hasChanges = false;

            titleContent = new GUIContent($"Edit Save Data: {fileName}");
            minSize = new Vector2(400, 300);

            // Initialize text area style
            if (textAreaStyle == null)
            {
                textAreaStyle = new GUIStyle(EditorStyles.textArea);
                textAreaStyle.wordWrap = false;
                textAreaStyle.font = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene).font;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // Header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Editing: {fileName}", EditorStyles.boldLabel);

            // Status indicator
            if (hasChanges)
            {
                GUI.color = Color.yellow;
                EditorGUILayout.LabelField("*", EditorStyles.boldLabel, GUILayout.Width(10));
                GUI.color = Color.white;
                EditorGUILayout.LabelField("(Modified)", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField("(Saved)", EditorStyles.miniLabel);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Action buttons
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = hasChanges;
            if (GUILayout.Button("Save Changes", GUILayout.Height(25)))
            {
                SaveJsonChanges();
            }

            if (GUILayout.Button("Revert Changes", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Revert Changes",
                    "Discard all changes and revert to original?",
                    "Revert", "Cancel"))
                {
                    RevertChanges();
                }
            }
            GUI.enabled = true;

            if (GUILayout.Button("Format JSON", GUILayout.Height(25)))
            {
                FormatJson();
            }

            if (GUILayout.Button("Validate JSON", GUILayout.Height(25)))
            {
                ValidateJson();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            // JSON editor
            EditorGUILayout.LabelField("JSON Content:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUI.BeginChangeCheck();
            jsonContent = EditorGUILayout.TextArea(jsonContent, textAreaStyle, GUILayout.ExpandHeight(true));
            if (EditorGUI.EndChangeCheck())
            {
                hasChanges = jsonContent != originalJsonContent;
            }

            EditorGUILayout.EndScrollView();

            // Footer info
            EditorGUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Lines: {jsonContent.Split('\n').Length}", EditorStyles.miniLabel);
            EditorGUILayout.LabelField($"Characters: {jsonContent.Length}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void SaveJsonChanges()
        {
            try
            {
                // Validate JSON before saving
                var parsed = SimpleJSON.JSON.Parse(jsonContent);
                if (parsed == null)
                {
                    EditorUtility.DisplayDialog("Invalid JSON",
                        "The JSON content is invalid and cannot be saved.", "OK");
                    return;
                }

                // Create backup before saving
                var backupPath = filePath + $".backup_{System.DateTime.Now:yyyyMMddHHmmss}";
                File.Copy(filePath, backupPath);

                // Save the file
                File.WriteAllText(filePath, jsonContent);

                originalJsonContent = jsonContent;
                hasChanges = false;

                Debug.Log($"Save data updated: {fileName}");
                EditorUtility.DisplayDialog("Saved",
                    $"Changes saved successfully!\nBackup created: {Path.GetFileName(backupPath)}", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to save changes: {ex.Message}");
                EditorUtility.DisplayDialog("Save Failed",
                    $"Failed to save changes:\n{ex.Message}", "OK");
            }
        }

        private void RevertChanges()
        {
            jsonContent = originalJsonContent;
            hasChanges = false;
        }

        private void FormatJson()
        {
            try
            {
                var parsed = SimpleJSON.JSON.Parse(jsonContent);
                if (parsed != null)
                {
                    jsonContent = parsed.ToString(4);
                    hasChanges = jsonContent != originalJsonContent;
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Format Failed",
                    $"Could not format JSON:\n{ex.Message}", "OK");
            }
        }

        private void ValidateJson()
        {
            try
            {
                var parsed = SimpleJSON.JSON.Parse(jsonContent);
                if (parsed != null)
                {
                    EditorUtility.DisplayDialog("Valid JSON",
                        "The JSON content is valid!", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid JSON",
                        "The JSON content is invalid.", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Invalid JSON",
                    $"JSON validation failed:\n{ex.Message}", "OK");
            }
        }

        private void OnDestroy()
        {
            if (hasChanges)
            {
                if (EditorUtility.DisplayDialog("Unsaved Changes",
                    "You have unsaved changes. Save before closing?",
                    "Save", "Discard"))
                {
                    SaveJsonChanges();
                }
            }
        }
    }
}