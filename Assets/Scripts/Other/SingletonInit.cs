using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonInit : MonoBehaviour
{
    public TransitionManager OriginalTransitionManager;

    // Start is called before the first frame update
    void Start()
    {
        bool dummy = TransitionManager.Instance.isActiveAndEnabled;
        TransitionManager.Instance.LastSceneRenderTexture = OriginalTransitionManager.LastSceneRenderTexture;
        TransitionManager.Instance.renderMaterial = OriginalTransitionManager.renderMaterial;
        TransitionManager.Instance.transitionTime = OriginalTransitionManager.transitionTime;

        TransitionManager.Instance.ComputeShader = OriginalTransitionManager.ComputeShader;
        TransitionManager.Instance.ParticelMaterial = OriginalTransitionManager.ParticelMaterial;
        TransitionManager.Instance.NowSceneRenderTexture = OriginalTransitionManager.NowSceneRenderTexture;
        //TransitionManager.Instance.transitionMode = OriginalTransitionManager.transitionMode;
        TransitionManager.Instance.particleLayer = OriginalTransitionManager.particleLayer;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
