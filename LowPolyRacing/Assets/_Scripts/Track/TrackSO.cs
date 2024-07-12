using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Track")]
public class TrackSO : ScriptableObject {

    public string trackName = "Testtrack01";
    public Sprite trackImage;
    public int trackLapCount = 3;

}
