using System.Text;

public class GDocToStringConverter : IGoogleDriveConverter<string>
{
    public string Extension { get { return null; } }
    public string MimeType { get { return "application/vnd.google-apps.document"; } }
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
