using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkedLoadingProgressTracker : NetworkBehaviour {

    // The current loading progress associated with the owner of this NetworkBehaviour
    public NetworkVariable<float> progress {
        get;
    } = new NetworkVariable<float>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
}