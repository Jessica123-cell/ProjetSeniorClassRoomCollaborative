using UnityEngine;
using UnityEngine.Android;
using System.Collections;

public class RequestMicrophonePermission : MonoBehaviour
{
    void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(RequestMicCoroutine());
#endif
    }

    IEnumerator RequestMicCoroutine()
    {
        // Attendre une frame pour être sûr que l'appli est bien lancée
        yield return null;

        // Déjà autorisé ?
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
            yield break;

        // Demande la permission
        Permission.RequestUserPermission(Permission.Microphone);

        // Attente jusqu’à ce que l'utilisateur dise oui
        while (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
            yield return null;

        Debug.Log("permission micro accordée !");
    }
}
