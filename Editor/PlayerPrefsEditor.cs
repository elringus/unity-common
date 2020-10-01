#if UNITY_EDITOR_WIN

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class PlayerPrefsEditor : EditorWindow
{
    private enum PrefType { Float = 0, Int, String, Bool };

    [Serializable]
    private struct PrefPair
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }

    private static readonly Encoding encoding = new UTF8Encoding();
    private static readonly DateTime missingDateTime = new DateTime(1601, 1, 1);

    private bool showEditorPrefs = false;
    private SearchField searchField = default;
    private List<PrefPair> deserializedPlayerPrefs = new List<PrefPair>();
    private List<PrefPair> filteredPlayerPrefs = new List<PrefPair>();
    private DateTime? lastDeserialization = null;
    private Vector2 scrollPosition;
    private Vector2 lastScrollPosition;
    private int inspectorUpdateFrame = 0;
    private string searchFilter = string.Empty;
    private string keyQueuedForDeletion = null;
    private PrefType newEntryType = PrefType.String;
    private string newEntryKey = "";
    private float newEntryValueFloat = 0;
    private int newEntryValueInt = 0;
    private bool newEntryValueBool = false;
    private string newEntryValueString = "";

    private void OnEnable ()
    {
        searchField = new SearchField();
    }

    [MenuItem("Window/PlayerPrefs Editor")]
    private static void OpenWindow ()
    {
        var editor = GetWindow<PlayerPrefsEditor>("Prefs Editor", true);
        editor.titleContent = new GUIContent("Prefs Editor", EditorGUIUtility.IconContent("Settings").image);
        editor.minSize = new Vector2(230, 400);
    }

    private void OnGUI ()
    {
        EditorGUILayout.Space();

        DrawTopBar();

        if (!lastDeserialization.HasValue || DateTime.UtcNow - lastDeserialization.Value > TimeSpan.FromMilliseconds(500))
        {
            deserializedPlayerPrefs = new List<PrefPair>(RetrieveSavedPrefs(PlayerSettings.companyName, PlayerSettings.productName));
            lastDeserialization = DateTime.UtcNow;
        }

        DrawMainList();
        DrawAddEntry();
        DrawBottomMenu();

        EditorGUILayout.Space();

        // If the user has scrolled, deselect - this is because control IDs within carousel will change when scrolled
        // so we'd end up with the wrong box selected.
        if (scrollPosition != lastScrollPosition) GUI.FocusControl("");
    }

    private void OnInspectorUpdate ()
    {
        // If a PlayerPref has been specified for deletion.
        if (!string.IsNullOrEmpty(keyQueuedForDeletion))
        {
            // If the user just deleted a PlayerPref, find the ID and defer it for deletion by OnInspectorUpdate().
            if (deserializedPlayerPrefs != null)
            {
                var entryCount = deserializedPlayerPrefs.Count;
                for (int i = 0; i < entryCount; i++)
                {
                    if (deserializedPlayerPrefs[i].Key == keyQueuedForDeletion)
                    {
                        deserializedPlayerPrefs.RemoveAt(i);
                        break;
                    }
                }
            }

            // Remove the queued key since we've just deleted it.
            keyQueuedForDeletion = null;

            // Update the search results and repaint the window.
            UpdateSearch();
            Repaint();
        }
        else if (inspectorUpdateFrame % 10 == 0) // Once a second (every 10th frame)
        {
            // Force the window to repaint.
            Repaint();
        }

        // Track what frame we're on, so we can call code less often.
        inspectorUpdateFrame++;
    }

    private void DeleteAll ()
    {
        if (showEditorPrefs) EditorPrefs.DeleteAll();
        else PlayerPrefs.DeleteAll();
    }

    private void DeleteKey (string key)
    {
        if (showEditorPrefs) EditorPrefs.DeleteKey(key);
        else PlayerPrefs.DeleteKey(key);
    }

    private int GetInt (string key, int defaultValue = 0)
    {
        if (showEditorPrefs) return EditorPrefs.GetInt(key, defaultValue);
        else return PlayerPrefs.GetInt(key, defaultValue);
    }

    private float GetFloat (string key, float defaultValue = 0.0f)
    {
        if (showEditorPrefs) return EditorPrefs.GetFloat(key, defaultValue);
        else return PlayerPrefs.GetFloat(key, defaultValue);
    }

    private string GetString (string key, string defaultValue = "")
    {
        if (showEditorPrefs) return EditorPrefs.GetString(key, defaultValue);
        else return PlayerPrefs.GetString(key, defaultValue);
    }

    private bool GetBool (string key, bool defaultValue = false)
    {
        if (showEditorPrefs) return EditorPrefs.GetBool(key, defaultValue);
        else throw new NotSupportedException("PlayerPrefs interface does not natively support booleans.");
    }

    private void SetInt (string key, int value)
    {
        if (showEditorPrefs) EditorPrefs.SetInt(key, value);
        else PlayerPrefs.SetInt(key, value);
    }

    private void SetFloat (string key, float value)
    {
        if (showEditorPrefs) EditorPrefs.SetFloat(key, value);
        else PlayerPrefs.SetFloat(key, value);
    }

    private void SetString (string key, string value)
    {
        if (showEditorPrefs) EditorPrefs.SetString(key, value);
        else PlayerPrefs.SetString(key, value);
    }

    private void SetBool (string key, bool value)
    {
        if (showEditorPrefs) EditorPrefs.SetBool(key, value);
        else throw new NotSupportedException("PlayerPrefs interface does not natively support booleans.");
    }

    private void Save ()
    {
        if (showEditorPrefs) { } 
        else PlayerPrefs.Save();
    }

    /// <summary>
    /// This returns an array of the stored PlayerPrefs from the Windows registry, to allow 
    /// us to to look up what's actually in the PlayerPrefs. This is used as a kind of lookup table.
    /// </summary>
    private PrefPair[] RetrieveSavedPrefs (string companyName, string productName)
    {
        RegistryKey registryKey;

        if (showEditorPrefs)
        {
            var majorVersion = Application.unityVersion.Split('.')[0];
            registryKey = Registry.CurrentUser.OpenSubKey("Software\\Unity Technologies\\Unity Editor " + majorVersion + ".x");
        }
        // On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
        else registryKey = Registry.CurrentUser.OpenSubKey("Software\\Unity\\UnityEditor\\" + companyName + "\\" + productName);

        // No prefs saved for the project.
        if (registryKey is null) return new PrefPair[0];

        var valueNames = registryKey.GetValueNames();
        var tempPlayerPrefs = new PrefPair[valueNames.Length];

        for (int i = 0; i < valueNames.Length; i++)
        {
            var valueName = valueNames[i];
            var key = valueNames[i];

            // Remove the _h193410979 style suffix used on PlayerPref keys in Windows registry.
            var index = key.LastIndexOf("_", StringComparison.Ordinal);
            key = key.Remove(index, key.Length - index);

            var ambiguousValue = registryKey.GetValue(valueName);

            // Unfortunately floats will come back as an int (at least on 64 bit) because the float is stored as
            // 64 bit but marked as 32 bit - which confuses the GetValue() method greatly! 
            if (ambiguousValue is int)
            {
                // If the PlayerPref is not actually an int then it must be a float, this will evaluate to true
                // (impossible for it to be 0 and -1 at the same time).
                if (GetInt(key, -1) == -1 && GetInt(key) == 0)
                    ambiguousValue = GetFloat(key);
                // If it reports a non default value as a bool, it's a bool not a string.
                else if (showEditorPrefs && (GetBool(key, true) != true || GetBool(key)))
                    ambiguousValue = GetBool(key);
            }
            else if (ambiguousValue.GetType() == typeof(byte[]))
                // On Unity 5 a string may be stored as binary, so convert it back to a string.
                ambiguousValue = encoding.GetString((byte[])ambiguousValue).TrimEnd('\0');

            tempPlayerPrefs[i] = new PrefPair() { Key = key, Value = ambiguousValue };
        }

        return tempPlayerPrefs;
    }

    private void UpdateSearch ()
    {
        filteredPlayerPrefs.Clear();

        if (string.IsNullOrEmpty(searchFilter)) return;

        var entryCount = deserializedPlayerPrefs.Count;

        for (int i = 0; i < entryCount; i++)
        {
            var fullKey = deserializedPlayerPrefs[i].Key;
            var displayKey = fullKey;

            if (displayKey.ToLower().Contains(searchFilter.ToLower()))
                filteredPlayerPrefs.Add(deserializedPlayerPrefs[i]);
        }
    }

    private void DrawTopBar ()
    {
        var newSearchFilter = searchField.OnGUI(searchFilter);
        GUILayout.Space(4);

        // If the requested search filter has changed.
        if (newSearchFilter != searchFilter)
        {
            searchFilter = newSearchFilter;
            // Trigger UpdateSearch to calculate new search results.
            UpdateSearch();
        }

        // Allow the user to toggle between editor and PlayerPrefs.
        var oldIndex = showEditorPrefs ? 1 : 0;
        var newIndex = GUILayout.Toolbar(oldIndex, new[] { "PlayerPrefs", "EditorPrefs" });

        // Has the toggle changed?
        if (newIndex != oldIndex)
        {
            // Reset.
            lastDeserialization = null;
            showEditorPrefs = (newIndex == 1);
        }
    }

    private void DrawMainList ()
    {
        // The bold table headings.
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Key", EditorStyles.boldLabel);
        GUILayout.Label("Value", EditorStyles.boldLabel);
        GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(37));
        GUILayout.Label("Del", EditorStyles.boldLabel, GUILayout.Width(25));
        EditorGUILayout.EndHorizontal();

        var textFieldStyle = new GUIStyle(GUI.skin.textField);
        var activePlayerPrefs = deserializedPlayerPrefs;

        if (!string.IsNullOrEmpty(searchFilter))
            activePlayerPrefs = filteredPlayerPrefs;

        int entryCount = activePlayerPrefs.Count;
        lastScrollPosition = scrollPosition;

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        if (scrollPosition.y < 0) scrollPosition.y = 0;

        // The following code has been optimised so that rather than attempting to draw UI for every single PlayerPref
        // it instead only draws the UI for those currently visible in the scroll view and pads above and below those
        // results to maintain the right size using GUILayout.Space(). This enables us to work with thousands of 
        // PlayerPrefs without slowing the interface to a halt.

        var rowHeight = 18;
        var visibleCount = Mathf.CeilToInt((float)Screen.height / rowHeight);
        var firstShownIndex = Mathf.FloorToInt(scrollPosition.y / rowHeight);
        var shownIndexLimit = firstShownIndex + visibleCount;

        if (entryCount < shownIndexLimit) shownIndexLimit = entryCount;

        // If the number of displayed PlayerPrefs is smaller than the number we can display (like we're at the end
        // of the list) then move the starting index back to adjust.
        if (shownIndexLimit - firstShownIndex < visibleCount)
            firstShownIndex -= visibleCount - (shownIndexLimit - firstShownIndex);

        // Can't have a negative index of a first shown PlayerPref, so clamp to 0
        if (firstShownIndex < 0) firstShownIndex = 0;

        // Pad above the on screen results so that we're not wasting draw calls on invisible UI and the drawn player
        // prefs end up in the same place in the list.
        GUILayout.Space(firstShownIndex * rowHeight);

        for (int i = firstShownIndex; i < shownIndexLimit; i++)
        {
            textFieldStyle.normal.textColor = GUI.skin.textField.normal.textColor;
            textFieldStyle.focused.textColor = GUI.skin.textField.focused.textColor;

            var fullKey = activePlayerPrefs[i].Key;
            var displayKey = fullKey;
            var deserializedValue = activePlayerPrefs[i].Value;

            EditorGUILayout.BeginHorizontal();

            var valueType = deserializedValue.GetType();
            EditorGUILayout.TextField(displayKey, textFieldStyle);

            if (valueType == typeof(float))
            {
                var initialValue = GetFloat(fullKey);
                var newValue = EditorGUILayout.FloatField(initialValue, textFieldStyle);
                if (!Mathf.Approximately(newValue, initialValue))
                {
                    SetFloat(fullKey, newValue);
                    Save();
                }
                GUILayout.Label("float", GUILayout.Width(37));
            }
            else if (valueType == typeof(int))
            {
                var initialValue = GetInt(fullKey);
                int newValue = EditorGUILayout.IntField(initialValue, textFieldStyle);
                if (newValue != initialValue)
                {
                    SetInt(fullKey, newValue);
                    Save();
                }
                GUILayout.Label("int", GUILayout.Width(37));
            }
            else if (valueType == typeof(bool))
            {
                var initialValue = GetBool(fullKey);
                var newValue = EditorGUILayout.Toggle(initialValue);
                if (newValue != initialValue)
                {
                    SetBool(fullKey, newValue);
                    Save();
                }
                GUILayout.Label("bool", GUILayout.Width(37));
            }
            else if (valueType == typeof(string))
            {
                var initialValue = GetString(fullKey);
                var newValue = EditorGUILayout.TextField(initialValue, textFieldStyle);
                if (newValue != initialValue)
                {
                    SetString(fullKey, newValue);
                    Save();
                }
                GUILayout.Label("string", GUILayout.Width(37));
            }

            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                DeleteKey(fullKey);
                Save();
                DeleteCachedRecord(fullKey);
            }
            EditorGUILayout.EndHorizontal();
        }

        var bottomPadding = (entryCount - shownIndexLimit) * rowHeight;
        if (bottomPadding > 0) GUILayout.Space(bottomPadding);

        EditorGUILayout.EndScrollView();

        GUILayout.Label("Entry Count: " + entryCount);

        var rect = GUILayoutUtility.GetLastRect();
        rect.height = 1;
        rect.y -= 4;
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    private void DrawAddEntry ()
    {
        var textFieldStyle = new GUIStyle(GUI.skin.textField);

        EditorGUILayout.Space();

        GUILayout.Label(showEditorPrefs ? "Add EditorPref" : "Add PlayerPref", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();

        if (showEditorPrefs)
            newEntryType = (PrefType)GUILayout.Toolbar((int)newEntryType, new[] { "float", "int", "string", "bool" });
        else
        {
            if (newEntryType == PrefType.Bool) newEntryType = PrefType.String;
            newEntryType = (PrefType)GUILayout.Toolbar((int)newEntryType, new[] { "float", "int", "string" });
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Key", EditorStyles.boldLabel);
        GUILayout.Label("Value", EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        GUI.SetNextControlName("newEntryKey");
        newEntryKey = EditorGUILayout.TextField(newEntryKey, textFieldStyle);
        GUI.SetNextControlName("newEntryValue");

        switch (newEntryType)
        {
            case PrefType.Float: newEntryValueFloat = EditorGUILayout.FloatField(newEntryValueFloat, textFieldStyle); break;
            case PrefType.Int: newEntryValueInt = EditorGUILayout.IntField(newEntryValueInt, textFieldStyle); break;
            case PrefType.String: newEntryValueString = EditorGUILayout.TextField(newEntryValueString, textFieldStyle); break;
            case PrefType.Bool: newEntryValueBool = EditorGUILayout.Toggle(newEntryValueBool); break;
        }

        // If the user hit enter while either the key or value fields were being edited.
        var keyboardAddPressed = Event.current.isKey && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && (GUI.GetNameOfFocusedControl() == "newEntryKey" || GUI.GetNameOfFocusedControl() == "newEntryValue");

        // If the user clicks the Add button or hits return (and there is a non-empty key), create the PlayerPref.
        if ((GUILayout.Button("Add", GUILayout.Width(40)) || keyboardAddPressed) && !string.IsNullOrEmpty(newEntryKey))
        {
            if (newEntryType == PrefType.Float)
            {
                SetFloat(newEntryKey, newEntryValueFloat);
                CacheRecord(newEntryKey, newEntryValueFloat);
            }
            else if (newEntryType == PrefType.Int)
            {
                SetInt(newEntryKey, newEntryValueInt);
                CacheRecord(newEntryKey, newEntryValueInt);
            }
            else if (newEntryType == PrefType.Bool)
            {
                SetBool(newEntryKey, newEntryValueBool);
                CacheRecord(newEntryKey, newEntryValueBool);
            }
            else
            {
                SetString(newEntryKey, newEntryValueString);
                CacheRecord(newEntryKey, newEntryValueString);
            }

            Save();
            Repaint();

            newEntryKey = "";
            newEntryValueFloat = 0;
            newEntryValueInt = 0;
            newEntryValueString = "";

            // Deselect.
            GUI.FocusControl("");
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawBottomMenu ()
    {
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        float buttonWidth = (EditorGUIUtility.currentViewWidth - 10) / 2f;
        if (GUILayout.Button("Delete All Preferences", GUILayout.Width(buttonWidth)))
        {
            if (EditorUtility.DisplayDialog("Delete All?", "Are you sure you want to delete all preferences?", "Delete All", "Cancel"))
            {
                DeleteAll();
                Save();
                deserializedPlayerPrefs.Clear();
            }
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Force Save", GUILayout.Width(buttonWidth))) Save();
        EditorGUILayout.EndHorizontal();
    }

    private void CacheRecord (string key, object value)
    {
        // First of all check if this key already exists, if so replace it's value with the new value/
        var replaced = false;
        var entryCount = deserializedPlayerPrefs.Count;
        for (int i = 0; i < entryCount; i++)
        {
            if (deserializedPlayerPrefs[i].Key == key)
            {
                deserializedPlayerPrefs[i] = new PrefPair { Key = key, Value = value };
                replaced = true;
                break;
            }
        }

        // PlayerPref doesn't already exist (and wasn't replaced) so add it as new.
        if (!replaced)
        {
            // Cache a PlayerPref the user just created so it can be instantly display (mainly for OSX)
            deserializedPlayerPrefs.Add(new PrefPair { Key = key, Value = value });
        }

        // Update the search if it's active
        UpdateSearch();
    }

    private void DeleteCachedRecord (string fullKey)
    {
        keyQueuedForDeletion = fullKey;
    }
}

#endif
