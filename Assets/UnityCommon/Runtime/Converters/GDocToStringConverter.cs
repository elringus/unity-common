using System.Text;

public class GDocToStringConverter : IGoogleDriveConverter<string>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation(null, "application/vnd.google-apps.document")
    }; } }

    public string ExportMimeType { get { return "text/plain"; } }

    public string Convert (byte[] obj)
    {
        return Encoding.UTF8.GetString(obj);
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
