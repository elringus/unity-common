using System.Text;
using System.Threading.Tasks;

namespace UnityCommon
{
    public class GDocToStringConverter : IGoogleDriveConverter<string>
    {
        public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
            new RawDataRepresentation(null, "application/vnd.google-apps.document")
        }; } }

        public string ExportMimeType { get { return "text/plain"; } }

        public string Convert (byte[] obj) => Encoding.UTF8.GetString(obj);

        public Task<string> ConvertAsync (byte[] obj) => Task.FromResult(Encoding.UTF8.GetString(obj));

        public object Convert (object obj) => Convert(obj as byte[]);

        public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
    }
}
