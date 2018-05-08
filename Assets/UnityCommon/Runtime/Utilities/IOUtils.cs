using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public static class IOUtils
{
    /// <summary>
    /// Reads a file with the provided path using async or sync IO depending on the platform.
    /// </summary>
    public static async Task<byte[]> ReadFileAsync (string filePath)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) return File.ReadAllBytes(filePath);
        else
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            {
                var fileData = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileData, 0, (int)fileStream.Length);
                return fileData;
            }
        }
    }

    /// <summary>
    /// Writes a file's data to the provided path using async or sync IO depending on the platform.
    /// </summary>
    public static async Task WriteFileAsync (string filePath, byte[] fileData)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) File.WriteAllBytes(filePath, fileData);
        else
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, fileData.Length, true))
                await fileStream.WriteAsync(fileData, 0, fileData.Length);
        }

        // Flush cached file writes to IndexedDB on WebGL.
        // https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLExtensions.SyncFs();
        #endif
    }

    /// <summary>
    /// Reads a text file with the provided path using async or sync IO depending on the platform.
    /// </summary>
    public static async Task<string> ReadTextFileAsync (string filePath)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) return File.ReadAllText(filePath);
        else
        {
            using (var stream = File.OpenText(filePath))
                return await stream.ReadToEndAsync();
        }
    }

    /// <summary>
    /// Writes a text file's data to the provided path using async or sync IO depending on the platform.
    /// </summary>
    public static async Task WriteTextFileAsync (string filePath, string fileText)
    {
        if (Application.platform == RuntimePlatform.WebGLPlayer) File.WriteAllText(filePath, fileText);
        else
        {
            using (var stream = File.CreateText(filePath))
                await stream.WriteAsync(fileText);
        }

        // Flush cached file writes to IndexedDB on WebGL.
        // https://forum.unity.com/threads/webgl-filesystem.294358/#post-1940712
        #if UNITY_WEBGL && !UNITY_EDITOR
        WebGLExtensions.SyncFs();
        #endif
    }
}
