//using UnityEngine;

//using UnityEngine.UI;

//using UnityEngine.Windows.WebCam;

//public class CameraFeed : MonoBehaviour

//{

// private WebCamTexture webcam;

// [SerializeField] private RawImage img = default;

// public int deviceIndex = 0;

//private void Start()

//{

//   WebCamDevice my_device = new WebCamDevice();

//   WebCamDevice[] devices = WebCamTexture.devices;

//   for (int i = 0; i < devices.Length; i++)

//   {

//       Debug.Log(devices[i].name);

//       my_device = devices[deviceIndex];

//   }

//  webcam = new WebCamTexture(my_device.name);

//   Renderer renderer = GetComponent<Renderer>();

//  renderer.material.mainTexture = webcam;

//  webcam.Play();

//}

//private void Start()

//{

//    Application.targetFrameRate = 72;

//    // Check if there are any webcams connected

//    if (WebCamTexture.devices.Length == 0)

//    {

//        Debug.LogError("No webcam found!");

//        return;

//    }

//    // Initialize the webcam texture with specific parameters

//    webcam = new WebCamTexture(WebCamTexture.devices[0].name, 1280, 720, 30);

//    // Assign the webcam texture to the material of the panel

//    img.texture = webcam;

//    // Start the webcam

//    webcam.Play();

//}

//private void OnDestroy()

//{

//    // Stop the webcam when the object is destroyed

//    if (webcam != null && webcam.isPlaying)

//    {

//        webcam.Stop();

//    }

//}

//}

//Le code que j'ai modifié le 04/11/2015

using UnityEngine;

using UnityEngine.UI;

public class CameraFeed : MonoBehaviour

{

    private WebCamTexture webcam;

    [SerializeField] private RawImage img = default;

    public int deviceIndex = 0;

    private void Start()

    {

        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length == 0)

        {

            Debug.LogError("Aucune caméra trouvée !");

            return;

        }

        // Choisir le device

        if (deviceIndex >= devices.Length) deviceIndex = 0;

        string deviceName = devices[deviceIndex].name;

        Debug.Log("Camera sélectionnée : " + deviceName);

        // Créer le WebCamTexture

        webcam = new WebCamTexture(deviceName);

        // Afficher sur RawImage

        if (img != null)

        {

            img.texture = webcam;

            img.material.mainTexture = webcam;

        }

        // Démarrer la caméra

        webcam.Play();

    }

    private void OnDestroy()

    {

        if (webcam != null && webcam.isPlaying)

            webcam.Stop();

    }

}
