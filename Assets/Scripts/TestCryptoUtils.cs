using UnityCommon;
using UnityEngine;

public class TestCryptoUtils : MonoBehaviour
{
    public string Input;

    private void OnEnable ()
    {
        print("CryptoUtils.PersistentHashCode: " + CryptoUtils.PersistentHashCode(Input));
        print("CryptoUtils.PersistentHexCode: " + CryptoUtils.PersistentHexCode(Input));
    }
}
