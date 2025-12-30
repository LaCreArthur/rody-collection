using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ReferenceScanner
{
    /// <summary>
    /// EditorWindow for scanning ScriptableObject references, Variable listeners,
    /// and BetterEvent usages across scenes and prefabs.
    /// </summary>
    public class ReferenceScannerWindow : EditorWindow
    {
        enum Tab { SOReferences, BuildReport, VariableListeners, BetterEventAudit }

        Tab _currentTab;
        Vector2 _scrollPosition;

        // SO References tab state
        ScriptableObject _targetSO;
        List<ScanResult> _soResults = new();
        bool _isScanning;
        float _scanProgress;
        string _scanStatus;

        // Build Report tab state
        BuildReportAnalyzer.BuildReportData _buildReportData;
        List<string> _unusedFromBuild = new();
        HashSet<string> _selectedForDeletion = new();
        Vector2 _buildReportScrollPosition;
        bool _showUnusedFromBuild;
        bool _usedGuidScanFallback;

        // Styles
        GUIStyle _headerStyle;
        GUIStyle _resultStyle;
        bool _stylesInitialized;

        [MenuItem("Tools/Reference Scanner")]
        static void Open()
        {
            var window = GetWindow<ReferenceScannerWindow>("Reference Scanner");
            window.minSize = new Vector2(400, 300);
        }

        void InitStyles()
        {
            if (_stylesInitialized) return;

            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(0, 0, 10, 10)
            };

            _resultStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                padding = new RectOffset(4, 4, 2, 2)
            };

            _stylesInitialized = true;
        }

        void OnGUI()
        {
            InitStyles();

            // Tab bar
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Toggle(_currentTab == Tab.SOReferences, "SO References", EditorStyles.toolbarButton))
                _currentTab = Tab.SOReferences;
            if (GUILayout.Toggle(_currentTab == Tab.BuildReport, "Build Report", EditorStyles.toolbarButton))
                _currentTab = Tab.BuildReport;
            if (GUILayout.Toggle(_currentTab == Tab.VariableListeners, "Variable Listeners", EditorStyles.toolbarButton))
                _currentTab = Tab.VariableListeners;
            if (GUILayout.Toggle(_currentTab == Tab.BetterEventAudit, "BetterEvent Audit", EditorStyles.toolbarButton))
                _currentTab = Tab.BetterEventAudit;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            switch (_currentTab)
            {
                case Tab.SOReferences:
                    DrawSOReferencesTab();
                    break;
                case Tab.BuildReport:
                    DrawBuildReportTab();
                    break;
                case Tab.VariableListeners:
                    DrawVariableListenersTab();
                    break;
                case Tab.BetterEventAudit:
                    DrawBetterEventAuditTab();
                    break;
            }
        }

        void DrawSOReferencesTab()
        {
            EditorGUILayout.LabelField("ScriptableObject References", _headerStyle);

            // Target selection
            EditorGUILayout.BeginHorizontal();
            var newTarget = EditorGUILayout.ObjectField("Target SO", _targetSO, typeof(ScriptableObject), false) as ScriptableObject;
            if (newTarget != _targetSO)
            {
                _targetSO = newTarget;
                _soResults.Clear();
            }

            EditorGUI.BeginDisabledGroup(_targetSO == null || _isScanning);
            if (GUILayout.Button("Scan", GUILayout.Width(80)))
            {
                ScanForReferences();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Progress bar
            if (_isScanning)
            {
                EditorGUILayout.Space(5);
                var rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, _scanProgress, _scanStatus ?? "Scanning...");
            }

            // Results
            if (_soResults.Count > 0)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField($"Found {_soResults.Count} reference(s):", EditorStyles.boldLabel);

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
                foreach (var result in _soResults)
                {
                    DrawResultRow(result);
                }
                EditorGUILayout.EndScrollView();
            }
            else if (_targetSO != null && !_isScanning && _soResults.Count == 0)
            {
                EditorGUILayout.HelpBox("No references found. Check the Build Report tab to find all orphaned SOs.", MessageType.Info);
            }
        }

        void DrawResultRow(ScanResult result)
        {
            EditorGUILayout.BeginHorizontal();

            // Type icon
            var icon = result.AssetType == "Scene"
                ? EditorGUIUtility.IconContent("SceneAsset Icon")
                : EditorGUIUtility.IconContent("Prefab Icon");

            GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(18));

            // Path
            EditorGUILayout.LabelField(result.ToString(), _resultStyle);

            // Actions
            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(result.AssetPath);
                Selection.activeObject = obj;
                EditorGUIUtility.PingObject(obj);
            }

            if (result.AssetType == "Scene" && GUILayout.Button("Open", GUILayout.Width(50)))
            {
                if (UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(result.AssetPath);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void ScanForReferences()
        {
            _isScanning = true;
            _soResults.Clear();

            EditorApplication.delayCall += () =>
            {
                _soResults = SOReferenceFinder.FindReferences(_targetSO, (progress, status) =>
                {
                    _scanProgress = progress;
                    _scanStatus = System.IO.Path.GetFileName(status);
                    Repaint();
                });

                _isScanning = false;
                Repaint();
            };
        }

        void DrawBuildReportTab()
        {
            EditorGUILayout.LabelField("Unused ScriptableObjects", _headerStyle);

            // Show current method status
            if (_buildReportData != null && _buildReportData.IsValid)
            {
                EditorGUILayout.HelpBox(
                    $"Build report loaded ({_buildReportData.Platform}, {_buildReportData.BuildTime:g})\n" +
                    "This is the most accurate way to find truly unused assets.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No build report available. You can:\n" +
                    "• Build your project first for accurate detection (recommended)\n" +
                    "• Use GUID scan as fallback (checks scene/prefab references only)",
                    MessageType.Info);
            }

            EditorGUILayout.Space(10);

            // Action buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(_isScanning);

            if (GUILayout.Button("Load Build Report"))
            {
                LoadBuildReport();
            }

            if (_buildReportData != null && _buildReportData.IsValid)
            {
                if (GUILayout.Button("Find Unused SOs (Build Report)"))
                {
                    FindUnusedFromBuild();
                }
            }
            else
            {
                if (GUILayout.Button("Find Unused SOs (GUID Scan)"))
                {
                    FindOrphanedSOsForBuildTab();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            // Progress bar
            if (_isScanning)
            {
                EditorGUILayout.Space(5);
                var rect = EditorGUILayout.GetControlRect(false, 20);
                EditorGUI.ProgressBar(rect, _scanProgress, _scanStatus ?? "Scanning...");
            }

            // Show unused assets
            if (_unusedFromBuild.Count > 0)
            {
                EditorGUILayout.Space(10);

                // Method indicator
                string methodLabel = _usedGuidScanFallback
                    ? $"Orphaned SOs - GUID Scan ({_unusedFromBuild.Count})"
                    : $"SOs NOT in Build ({_unusedFromBuild.Count})";

                _showUnusedFromBuild = EditorGUILayout.Foldout(_showUnusedFromBuild, methodLabel);

                if (_showUnusedFromBuild)
                {
                    if (_usedGuidScanFallback)
                    {
                        EditorGUILayout.HelpBox(
                            "These ScriptableObjects have no references in scenes or prefabs.\n" +
                            "Build your project and reload for more accurate results.",
                            MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox(
                            "These ScriptableObjects exist in your project but were NOT included in the last build. " +
                            "They may be safe to delete, but verify they aren't loaded dynamically (Resources, Addressables).",
                            MessageType.Info);
                    }

                    // Selection controls
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Select All", GUILayout.Width(80)))
                    {
                        foreach (var path in _unusedFromBuild)
                            _selectedForDeletion.Add(path);
                    }
                    if (GUILayout.Button("Select None", GUILayout.Width(80)))
                    {
                        _selectedForDeletion.Clear();
                    }
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(_selectedForDeletion.Count == 0);
                    var deleteStyle = new GUIStyle(GUI.skin.button);
                    deleteStyle.normal.textColor = Color.red;
                    if (GUILayout.Button($"Delete Selected ({_selectedForDeletion.Count})", deleteStyle, GUILayout.Width(150)))
                    {
                        DeleteSelectedAssets();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.Space(5);

                    _buildReportScrollPosition = EditorGUILayout.BeginScrollView(_buildReportScrollPosition,
                        GUILayout.MaxHeight(400));
                    foreach (var path in _unusedFromBuild)
                    {
                        EditorGUILayout.BeginHorizontal();

                        // Checkbox
                        bool isSelected = _selectedForDeletion.Contains(path);
                        bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                        if (newSelected != isSelected)
                        {
                            if (newSelected) _selectedForDeletion.Add(path);
                            else _selectedForDeletion.Remove(path);
                        }

                        // Path label
                        EditorGUILayout.LabelField(path, _resultStyle);

                        // Select button
                        if (GUILayout.Button("Select", GUILayout.Width(50)))
                        {
                            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                            Selection.activeObject = obj;
                            EditorGUIUtility.PingObject(obj);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                }
            }
            else if (!_isScanning && (_buildReportData != null || _unusedFromBuild.Count == 0))
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Click a scan button above to find unused ScriptableObjects.", MessageType.Info);
            }
        }

        void LoadBuildReport()
        {
            _buildReportData = BuildReportAnalyzer.LoadLastBuildReport();
            _unusedFromBuild.Clear();
            Repaint();
        }

        void FindUnusedFromBuild()
        {
            if (_buildReportData == null || !_buildReportData.IsValid) return;

            _isScanning = true;
            _unusedFromBuild.Clear();
            _usedGuidScanFallback = false;

            EditorApplication.delayCall += () =>
            {
                _unusedFromBuild = BuildReportAnalyzer.FindUnusedScriptableObjects(_buildReportData,
                    (progress, status) =>
                    {
                        _scanProgress = progress;
                        _scanStatus = System.IO.Path.GetFileName(status);
                        Repaint();
                    });

                _isScanning = false;
                _showUnusedFromBuild = _unusedFromBuild.Count > 0;
                _selectedForDeletion.Clear();
                Repaint();
            };
        }

        void FindOrphanedSOsForBuildTab()
        {
            _isScanning = true;
            _unusedFromBuild.Clear();
            _usedGuidScanFallback = true;

            EditorApplication.delayCall += () =>
            {
                _unusedFromBuild = SOReferenceFinder.FindOrphanedScriptableObjects("Assets", (progress, status) =>
                {
                    _scanProgress = progress;
                    _scanStatus = System.IO.Path.GetFileName(status);
                    Repaint();
                });

                _isScanning = false;
                _showUnusedFromBuild = _unusedFromBuild.Count > 0;
                _selectedForDeletion.Clear();
                Repaint();
            };
        }

        void DeleteSelectedAssets()
        {
            if (_selectedForDeletion.Count == 0) return;

            var toDelete = _selectedForDeletion.ToList();
            string message = $"Are you sure you want to delete {toDelete.Count} ScriptableObject(s)?\n\n";

            if (toDelete.Count <= 5)
            {
                foreach (var path in toDelete)
                    message += $"- {System.IO.Path.GetFileName(path)}\n";
            }
            else
            {
                for (int i = 0; i < 5; i++)
                    message += $"- {System.IO.Path.GetFileName(toDelete[i])}\n";
                message += $"... and {toDelete.Count - 5} more";
            }

            message += "\n\nThis action cannot be undone!";

            if (EditorUtility.DisplayDialog("Delete Unused Assets", message, "Delete", "Cancel"))
            {
                int deleted = 0;
                int failed = 0;

                foreach (var path in toDelete)
                {
                    if (AssetDatabase.DeleteAsset(path))
                    {
                        deleted++;
                        _unusedFromBuild.Remove(path);
                    }
                    else
                    {
                        failed++;
                        Debug.LogWarning($"Failed to delete: {path}");
                    }
                }

                _selectedForDeletion.Clear();
                AssetDatabase.Refresh();

                string result = $"Deleted {deleted} asset(s)";
                if (failed > 0) result += $", {failed} failed";
                EditorUtility.DisplayDialog("Delete Complete", result, "OK");

                Repaint();
            }
        }

        void DrawVariableListenersTab()
        {
            EditorGUILayout.LabelField("Variable Listeners", _headerStyle);
            EditorGUILayout.HelpBox(
                "This tab will scan for *VariableListener components and show their SO bindings.\n\n" +
                "Coming in Phase 2.",
                MessageType.Info);
        }

        void DrawBetterEventAuditTab()
        {
            EditorGUILayout.LabelField("BetterEvent Audit", _headerStyle);
            EditorGUILayout.HelpBox(
                "This tab will scan for BetterEvent fields and decode Odin serialization.\n\n" +
                "Coming in Phase 3.",
                MessageType.Info);
        }
    }
}
