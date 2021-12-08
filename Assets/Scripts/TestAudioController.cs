using System.Collections;
using UnityCommon;
using UnityEngine;

[RequireComponent(typeof(AudioController))]
public class TestAudioController : MonoBehaviour
{
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip main;

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
