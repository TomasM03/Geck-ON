using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioFix : MonoBehaviour
{
    void Awake()
    {
        AudioConfiguration config = AudioSettings.GetConfiguration();
        AudioSettings.Reset(config);
    }
}
