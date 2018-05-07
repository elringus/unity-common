using System.Text;
using System.Threading.Tasks;

public class GDocToStringConverter : IGoogleDriveConverter<string>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation(null, "application/vnd.google-apps.document")
    }; } }

    public string ExportMimeType { get { return "text/plain"; } }

    public Task<string> ConvertAsync (byte[] obj) => Task.FromResult(Encoding.UTF8.GetString(obj));

    public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
}
