using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class GFolderToFolderConverter : IRawConverter<Folder>
    {
        public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
            new RawDataRepresentation(null, "application/vnd.google-apps.folder")
        }; } }

        public Folder Convert (byte[] obj)
        {
            Debug.LogError("Google Drive folders doesn't have binary content and are not downloadable.");
            return null;
        }

        public Task<Folder> ConvertAsync (byte[] obj)
        {
            Debug.LogError("Google Drive folders doesn't have binary content and are not downloadable.");
            return null;
        }

        public object Convert (object obj) => Convert(obj as byte[]);

        public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
