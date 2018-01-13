using System.Text;

public class TxtToStringConverter : IRawConverter<string>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation("txt", "text/plain")
    }; } }

    public string Convert (byte[] obj)
    {
        return Encoding.UTF8.GetString(obj);
    }

    public object Convert (object obj)
    {
        return Convert(obj as byte[]);
    }
}
