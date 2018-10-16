using System.Threading.Tasks;
using UnityEngine;

namespace UnityCommon
{
    public class DirectoryToFolderConverter : IRawConverter<Folder>
    {
        public RawDataRepresentation[] Representations { get { return null; } }

        public Folder Convert (byte[] obj)
        {
            Debug.LogError("Directory doesn't have binary content and can't be converted.");
            return default(Folder);
        }

        public Task<Folder> ConvertAsync (byte[] obj)
        {
            Debug.LogError("Directory doesn't have binary content and can't be converted.");
            return Task.FromResult(default(Folder));
        }

        public object Convert (object obj) => Convert(obj as byte[]);

        public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
