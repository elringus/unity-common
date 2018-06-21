using System.Collections;
using System.Collections.Generic;
using UnityCommon;
using UnityEngine;

public class TestNullables : MonoBehaviour
{
    public NullableFloat Float;
    public NullableString String;
    public NullableBool Bool;
    public NullableVector3 Vector3;
    public List<NullableVector3> Vector3List;

    private IEnumerator Start ()
    {
        yield return new WaitForSeconds(2);

        Vector3List[2].Value = UnityEngine.Vector3.one;
    }
}
