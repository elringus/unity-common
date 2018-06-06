using System.Text;
using System.Threading.Tasks;

public class TxtToStringConverter : IRawConverter<string>
{
    public RawDataRepresentation[] Representations { get { return new RawDataRepresentation[] {
        new RawDataRepresentation(".txt", "text/plain")
    }; } }

    public Task<string> ConvertAsync (byte[] obj) => Task.FromResult(Encoding.UTF8.GetString(obj));

    public async Task<object> ConvertAsync (object obj) => await ConvertAsync(obj as byte[]);
}
