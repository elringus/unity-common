using System.Collections;
using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

public class TestNullables : MonoBehaviour
{
    public NullableFloat Float;
    public NullableString String;
    public NullableBoolean Bool;
    public NullableVector3 Vector3;
    public List<NullableVector3> Vector3List = new List<NullableVector3> { new NullableVector3(), new NullableVector3() };

    private IEnumerator Start ()
    {
        yield return new WaitForSeconds(2);

        Vector3List[1].Value = UnityEngine.Vector3.one;
        String = "abc";

        yield return new WaitForSeconds(2);

        String = default;
    }
}
