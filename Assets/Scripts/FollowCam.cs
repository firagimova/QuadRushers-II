using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Camera DroneBackCam;
    public Camera MainCamera;

    void Start()
    {
        // Baþlangýçta sadece MainCamera'nýn açýk olmasýný saðla.
        DroneBackCam.enabled = true;
        MainCamera.enabled = false;
    }

    public void SwitchCam()
    {
        // Eðer MainCamera açýksa FollowCamera'yý aç, MainCamera'yý kapat.
        if (MainCamera.enabled == true)
        {
            DroneBackCam.enabled = true;
            MainCamera.enabled = false;
        }
        // Eðer FollowCamera açýksa MainCamera'yý aç, FollowCamera'yý kapat.
        else
        {
            DroneBackCam.enabled = false;
            MainCamera.enabled = true;
        }
    }
}

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class FollowCam : MonoBehaviour
//{
//    public Camera MainCamera;
//    public Camera DroneBackCam;

//    void Start()
//    {
//        // Baþlangýçta sadece MainCamera'nýn açýk olmasýný saðla.

//        MainCamera.enabled = false;
//        DroneBackCam.enabled = true;
//    }

//    public void SwitchCam()
//    {
//        // Eðer MainCamera açýksa FollowCamera'yý aç, MainCamera'yý kapat, DroneBackCam'i de kapat.
//        if (MainCamera.enabled == true)
//        {
//            MainCamera.enabled = false;
//            DroneBackCam.enabled = false;
//        }

//        // Eðer DroneBackCam açýksa MainCamera'yý aç, DroneBackCam'i kapat, FollowCamera'yý da kapat.
//        else if (DroneBackCam.enabled == true)
//        {
//            MainCamera.enabled = true;
//            DroneBackCam.enabled = false;
//        }
//    }
//}
