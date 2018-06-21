// Copyright 2017-2018 Elringus (Artyom Sovetnikov). All Rights Reserved.

using UnityEngine;

namespace UnityGoogleDrive.Data
{
    /// <summary>
    /// The metadata for an audio file stored in Google Drive.
    /// Unity-specific data to use with <see cref="GoogleDriveFiles.DownloadAudioRequest"/>.
    /// </summary>
    public class AudioFile : File
    {
        [Newtonsoft.Json.JsonIgnore]
        public AudioClip AudioClip { get; set; }
    }
}
