using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassControl : MonoBehaviour
{
    public RawImage compassRawImage;

    private void Update()
    {
        compassRawImage.uvRect = new Rect(Camera.main.transform.localEulerAngles.y / 360, 0, 1, 1);
    }
}
