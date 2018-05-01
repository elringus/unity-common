using UnityEngine;

public class TestThenOut : MonoBehaviour
{
    private void OnEnable ()
    {
        WaitForAndReturnValue(1).Then(PrintAndReturnString).Then(IncrementStringAndReturnInt)
            .ThenAsync(WaitForAndReturnValue).Then(PrintAndReturnString).Then(IncrementStringAndReturnInt)
            .ThenAsync(WaitForAndReturnValue).Then(PrintAndReturnString).Then(IncrementStringAndReturnInt);
    }

    private AsyncAction<int> WaitForAndReturnValue (int time)
    {
        return new Timer(time).Run().Then((o) => { return time; });
    }

    private string PrintAndReturnString (int value)
    {
        print(value);
        return value.ToString();
    }

    private int IncrementStringAndReturnInt (string value)
    {
        var v = int.Parse(value);
        return v + 1;
    }
}
