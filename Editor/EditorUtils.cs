using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityCommon
{
    public static class EditorUtils
    {
        /// <summary>
        /// Gets strongly typed value of a <see cref="SerializedPropertyType.Generic"/> <see cref="SerializedProperty"/> using reflection.
        /// </summary>
        public static TValue GetGenericValue<TValue> (this SerializedProperty property)
        {
            if (property is null || property.propertyType != SerializedPropertyType.Generic)
                throw new NullReferenceException("The property is null or not generic.");

            var targetObject = property.serializedObject.targetObject as object;
            var propertyPath = property.propertyPath;
            var paths = propertyPath.Split('.');
            var fieldInfo = default(FieldInfo);

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (targetObject == null)
                    throw new NullReferenceException("Can't set a value on a null instance.");

                var type = targetObject.GetType();
                if (path == "Array")
                {
                    path = paths[++i];

                    var array = targetObject as System.Collections.IEnumerable;
                    if (array is null)
                        throw new ArgumentException($"Property at path '{propertyPath}' can't be parsed: '{paths[i - 2]}' is not an enumerable.");

                    var indexString = path.Split('[', ']');

                    if (indexString.Length < 2)
                        throw new FormatException($"Property path '{propertyPath}' is malformed.");

                    if (!int.TryParse(indexString[1], out var index))
                        throw new FormatException($"Property path '{propertyPath}' is malformed.");

                    if (i == (paths.Length - 1)) // Our property is an array.
                    {
                        var targetArray = (System.Collections.IList)targetObject;
                        return (TValue)targetArray[index];
                    }

                    var elementIndex = 0; // Continue traversing the path chain over the array.
                    foreach (var element in array)
                    {
                        if (elementIndex == index)
                        {
                            targetObject = element;
                            break;
                        }
                        elementIndex++;
                    }
                    continue;
                }

                fieldInfo = type.GetFieldWithInheritance(path, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo == null)
                    throw new MissingFieldException($"The field '{path}' in '{propertyPath}' could not be found.");

                if (i < paths.Length - 1)
                    targetObject = fieldInfo.GetValue(targetObject);
            }

            var valueType = typeof(TValue);
            if (valueType is null || fieldInfo?.FieldType is null || !valueType.IsAssignableFrom(fieldInfo.FieldType))
                throw new InvalidCastException($"Cannot cast '{valueType}' into field type '{fieldInfo?.FieldType}'.");

            return (TValue)fieldInfo.GetValue(targetObject);
        }

        /// <summary>
        /// Sets value to a <see cref="SerializedPropertyType.Generic"/> <see cref="SerializedProperty"/> using reflection.
        /// </summary>
        public static void SetGenericValue (this SerializedProperty property, object value)
        {
            if (property is null || property.propertyType != SerializedPropertyType.Generic)
                throw new NullReferenceException("The property is null or not generic.");

            var targetObject = property.serializedObject.targetObject as object;
            var propertyPath = property.propertyPath;
            var paths = propertyPath.Split('.');
            var fieldInfo = default(FieldInfo);

            for (int i = 0; i < paths.Length; i++)
            {
                var path = paths[i];
                if (targetObject == null)
                    throw new NullReferenceException("Can't set a value on a null instance.");

                var type = targetObject.GetType();
                if (path == "Array")
                {
                    path = paths[++i];

                    var array = targetObject as System.Collections.IEnumerable;
                    if (array is null)
                        throw new ArgumentException($"Property at path '{propertyPath}' can't be parsed: '{paths[i - 2]}' is not an enumerable.");

                    var indexString = path.Split('[', ']');

                    if (indexString.Length < 2)
                        throw new FormatException($"Property path '{propertyPath}' is malformed.");

                    if (!int.TryParse(indexString[1], out var index))
                        throw new FormatException($"Property path '{propertyPath}' is malformed.");

                    if (i == (paths.Length - 1)) // Our property is an array.
                    {
                        var targetArray = (System.Collections.IList)targetObject;
                        targetArray[index] = value;
                        return;
                    }

                    var elementIndex = 0; // Continue traversing the path chain over the array.
                    foreach (var element in array)
                    {
                        if (elementIndex == index)
                        {
                            targetObject = element;
                            break;
                        }
                        elementIndex++;
                    }
                    continue;
                }

                fieldInfo = type.GetFieldWithInheritance(path, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                if (fieldInfo == null)
                    throw new MissingFieldException($"The field '{path}' in '{propertyPath}' could not be found.");

                if (i < paths.Length - 1)
                    targetObject = fieldInfo.GetValue(targetObject);
            }

            var valueType = value.GetType();
            if (valueType is null || fieldInfo?.FieldType is null || !valueType.IsAssignableFrom(fieldInfo.FieldType))
                throw new InvalidCastException($"Cannot cast '{valueType}' into field type '{fieldInfo?.FieldType}'.");

            fieldInfo.SetValue(targetObject, value);
        }

        /// <summary>
        /// Checks whether asset with the provided GUID exists.
        /// </summary>
        public static bool AssetExistsByGuid (string guid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            return AssetExistsByPath(assetPath);
        }

        /// <summary>
        /// Checks whether asset with the provided relative project asset path (`Assets/...`) exists.
        /// </summary>
        public static bool AssetExistsByPath (string path)
        {
            // Using GUIDToAssetPath() is not enough, as it could still return a path of a deleted asset.
            return AssetDatabase.GetMainAssetTypeAtPath(path) != null;
        }

        /// <summary>
        /// Performs <see cref="AssetDatabase.AssetPathToGUID(string)"/> and <see cref="AssetDatabase.LoadAssetAtPath(string, Type)"/>.
        /// Will return null in case asset with the provided GUID doesn't exist.
        /// </summary>
        public static Object LoadAssetByGuid (string guid, Type type)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath(path, type);
        }

        /// <summary>
        /// Performs <see cref="AssetDatabase.AssetPathToGUID(string)"/> and <see cref="AssetDatabase.LoadAllAssetsAtPath(string)"/>.
        /// Will return null in case asset with the provided GUID doesn't exist.
        /// </summary>
        public static T LoadAssetByGuid<T> (string guid, Type type) where T : Object
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAssetAtPath<T>(path);
        }

        /// <summary>
        /// For the provided editor object, will assign all the readonly <see cref="SerializedProperty"/> fields with the references to the corresponding fields of the edited object
        /// and static readonly <see cref="GUIContent"/> fields with the value of <see cref="TooltipAttribute"/> assigned to the corresponding fields of the edited object.
        /// Editor fields should be named after the edited fields (case doesn't matter), but end with `Property` for <see cref="SerializedProperty"/> and `Content` for <see cref="GUIContent"/>.
        /// </summary>
        /// <typeparam name="T">Type of the edited object.</typeparam>
        /// <param name="editor">The editor instance.</param>
        public static void BindSerializedProperties<T> (Editor editor)
        {
            var editorType = editor.GetType();
            var editedType = typeof(T);
            if (!(editedType.IsSubclassOf(typeof(MonoBehaviour)) || editedType.IsSubclassOf(typeof(ScriptableObject)))) return;

            var serializedFields = editedType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>(true) != null).ToList();
            var editorSerializedProperties = editorType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsInitOnly && f.FieldType == typeof(SerializedProperty)).ToList();
            var editorContentProperties = editorType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.IsInitOnly && f.IsStatic && f.FieldType == typeof(GUIContent)).ToList();

            foreach (var serializedField in serializedFields)
            {
                var serializedProperty = editorSerializedProperties.Find(p => p.Name.EqualsFastIgnoreCase(serializedField.Name + "Property"));
                if (serializedProperty != null)
                    serializedProperty.SetValue(editor, editor.serializedObject.FindProperty(serializedField.Name));

                var contentProperty = editorContentProperties.Find(p => p.Name.EqualsFastIgnoreCase(serializedField.Name + "Content"));
                if (contentProperty != null && contentProperty.GetValue(editor) is null)
                {
                    var contentText = ObjectNames.NicifyVariableName(serializedField.Name);
                    var contentTooltip = serializedField.GetCustomAttribute<TooltipAttribute>(true)?.tooltip ?? string.Empty;
                    contentProperty.SetValue(editor, new GUIContent(contentText, contentTooltip));
                }
            }
        }

        public static ScriptableObject LoadOrCreateSerializableAsset (string assetPath, Type assetType)
        {
            var existingAsset = AssetDatabase.LoadAssetAtPath(assetPath, assetType) as ScriptableObject;
            if (existingAsset) return existingAsset;

            var asset = ScriptableObject.CreateInstance(assetType);
            CreateFolderAsset(Path.GetDirectoryName(assetPath));
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            return asset;
        }

        public static T LoadOrCreateSerializableAsset<T> (string assetPath) where T : ScriptableObject
        {
            var assetType = typeof(T);
            return LoadOrCreateSerializableAsset(assetPath, assetType) as T;
        }

        public static T CreateOrReplaceAsset<T> (this Object asset, string path) where T : Object
        {
            var existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existingAsset == null)
            {
                AssetDatabase.CreateAsset(asset, path);
                return asset as T;
            }
            else
            {
                EditorUtility.CopySerialized(asset, existingAsset);
                return existingAsset;
            }
        }

        public static void SetListValues<T> (this SerializedProperty serializedProperty, List<T> listValues, bool clearSourceList = true) where T : Object
        {
            Debug.Assert(serializedProperty != null && serializedProperty.isArray);

            var targetObject = serializedProperty.serializedObject.targetObject;
            var objectType = targetObject.GetType();
            var fieldInfo = objectType.GetField(serializedProperty.name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (!(fieldInfo is null))
            {
                var list = (List<T>)fieldInfo.GetValue(targetObject);
                if (clearSourceList) list.Clear();
                list.AddRange(listValues);
                list.RemoveAll(item => !item || item == null);
            }

            serializedProperty.serializedObject.CopyFromSerializedProperty(new SerializedObject(targetObject).FindProperty(serializedProperty.name));
        }

        public static SerializedProperty GetArrayElementAtIndexOrNull (this SerializedProperty serializedProperty, int index)
        {
            if (!serializedProperty.isArray) return null;
            if (index < 0 || index >= serializedProperty.arraySize) return null;
            return serializedProperty.GetArrayElementAtIndex(index);
        }

        public static Texture2D SaveAsPng (this Texture2D texture, string path, TextureImporterType textureType = TextureImporterType.Default,
            TextureImporterCompression compression = TextureImporterCompression.Uncompressed, bool generateMipmaps = false, bool destroyInitialTextureObject = true)
        {
            var wrapMode = texture.wrapMode;
            var alphaIsTransparency = texture.alphaIsTransparency;
            var maxSize = Mathf.Max(texture.width, texture.height);

            path = $"{path.GetBeforeLast("/")}/{texture.name}.png";
            Debug.Assert(AssetDatabase.IsValidFolder(path.GetBefore("/")));
            var bytes = texture.EncodeToPNG();
            using (var fileStream = File.Create(path))
                fileStream.Write(bytes, 0, bytes.Length);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (!(textureImporter is null))
            {
                textureImporter.textureType = textureType;
                textureImporter.alphaIsTransparency = alphaIsTransparency;
                textureImporter.wrapMode = wrapMode;
                textureImporter.mipmapEnabled = generateMipmaps;
                textureImporter.textureCompression = compression;
                textureImporter.maxTextureSize = maxSize;
            }

            AssetDatabase.ImportAsset(path);

            if (destroyInitialTextureObject)
                Object.DestroyImmediate(texture);

            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        public static void ToggleLeftGUI (Rect position, SerializedProperty property, GUIContent label)
        {
            var toggleValue = property.boolValue;
            EditorGUI.showMixedValue = property.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            var oldIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            toggleValue = EditorGUI.ToggleLeft(position, label, toggleValue);
            EditorGUI.indentLevel = oldIndent;
            if (EditorGUI.EndChangeCheck())
                property.boolValue = property.hasMultipleDifferentValues || !property.boolValue;
            EditorGUI.showMixedValue = false;
        }

        public static void FolderField (SerializedProperty property, bool local = true,
            string title = default, string defaultPath = default)
        {
            PathField(property, (t, p) => EditorUtility.OpenFolderPanel(t, p, ""), local, title, defaultPath);
        }

        public static void FileField (SerializedProperty property, string extension, bool local = true,
            string title = default, string defaultPath = default)
        {
            PathField(property, (t, p) => EditorUtility.OpenFilePanel(t, p, extension), local, title, defaultPath);
        }

        public static void FileField (SerializedProperty property, string[] filters, bool local = true,
            string title = default, string defaultPath = default)
        {
            PathField(property, (t, p) => EditorUtility.OpenFilePanelWithFilters(t, p, filters), local, title, defaultPath);
        }

        public static void PathField (SerializedProperty property, Func<string, string, string> openPanel,
            bool local = false, string title = default, string defaultPath = default)
        {
            if (title is null) title = property.displayName;
            if (defaultPath is null) defaultPath = Application.dataPath;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(property);
            if (GUILayout.Button("Select", EditorStyles.miniButton, GUILayout.Width(65)))
            {
                var selectedPath = openPanel(title, defaultPath);
                if (local) selectedPath = PathUtils.AbsoluteToAssetPath(selectedPath);
                property.stringValue = selectedPath;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Creates a new folder in the project's `Assets` directory. 
        /// Path should be relative to the project (starting with `Assets/`).
        /// </summary>
        public static void CreateFolderAsset (string assetPath)
        {
            EnsureFolderIsCreatedRecursively(assetPath);

            void EnsureFolderIsCreatedRecursively (string targetFolder)
            {
                if (!AssetDatabase.IsValidFolder(targetFolder))
                {
                    EnsureFolderIsCreatedRecursively(Path.GetDirectoryName(targetFolder));
                    AssetDatabase.CreateFolder(Path.GetDirectoryName(targetFolder), Path.GetFileName(targetFolder));
                }
            }
        }
    }
}
