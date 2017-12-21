using System.Text;

public class TxtToStringConverter : IRawConverter<string>
{
    public string Extension { get { return "txt"; } }
    public string MimeType { get { return "text/plain"; } }

    public string Convert (byte[] obj)
    {
        return Encoding.UTF8.GetString(obj);
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
