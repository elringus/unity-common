using System.Collections;
using UnityCommon;
using UnityEngine;

[RequireComponent(typeof(AudioController))]
public class TestAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip intro = default;
    [SerializeField] private AudioClip main = default;

    private AudioController controller;

    private void Awake ()
    {
        controller = GetComponent<AudioController>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);

        controller.PlayClip(main, loop: true, introClip: intro);
    }

}
