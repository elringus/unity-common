using System.IO;
using UnityEngine;

namespace UnityCommon
{
    public static class IOUtils
    {
        /// <summary>
        /// Reads a file with the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static async UniTask<byte[]> ReadFileAsync (string filePath)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer) return File.ReadAllBytes(filePath);
            using (var fileStream = File.OpenRead(filePath))
            {
                var fileData = new byte[fileStream.Length];
                // ReSharper disable once MustUseReturnValue
                await fileStream.ReadAsync(fileData, 0, (int)fileStream.Length);
                return fileData;
            }
        }

        /// <summary>
        /// Writes a file's data to the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static async UniTask WriteFileAsync (string filePath, byte[] fileData)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer) File.WriteAllBytes(filePath, fileData);
            else
            {
                using (var fileStream = File.OpenWrite(filePath))
                    await fileStream.WriteAsync(fileData, 0, fileData.Length);
            }
            WebGLSyncFs();
        }

        /// <summary>
        /// Reads a text file with the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static async UniTask<string> ReadTextFileAsync (string filePath)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer) return File.ReadAllText(filePath);
            using (var stream = File.OpenText(filePath))
                return await stream.ReadToEndAsync();
        }

        /// <summary>
        /// Writes a text file's data to the provided path using async or sync IO depending on the platform.
        /// </summary>
        public static async UniTask WriteTextFileAsync (string filePath, string fileText)
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer) File.WriteAllText(filePath, fileText);
            else
            {
                using (var stream = File.CreateText(filePath))
                    await stream.WriteAsync(fileText);
            }
            WebGLSyncFs();
        }

        /// <summary>
        /// Deletes file at the provided path. Will insure for correct IO on specific platforms.
        /// </summary>
        public static void DeleteFile (string filePath)
        {
            File.Delete(filePath);
            WebGLSyncFs();
        }

        /// <summary>
        /// Moves a file from <paramref name="sourceFilePath"/> to <paramref name="destFilePath"/>.
        /// Will overwrite the <paramref name="destFilePath"/> in case it exists.
        /// </summary>
        public static void MoveFile (string sourceFilePath, string destFilePath)
        {
            File.Delete(destFilePath);
            File.Move(sourceFilePath, destFilePath);
            WebGLSyncFs();
        }

        /// <summary>
        /// Creates a new directory at the provided path. Will insure for correct IO on specific platforms.
        /// </summary>
        public static void CreateDirectory (string path)
        {
            Directory.CreateDirectory(path);
            WebGLSyncFs();
        }

        /// <summary>
        /// Deletes directory at the provided path. Will insure for correct IO on specific platforms.
        /// </summary>
        public static void DeleteDirectory (string path, bool recursive)
        {
            Directory.Delete(path, recursive);
            WebGLSyncFs();
        }

        /// <summary>
        /// Flush cached file writes to IndexedDB on WebGL.
        /// https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        /// </summary>
        public static void WebGLSyncFs ()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            WebGLExtensions.SyncFs();
            #endif
        }
    }
}
