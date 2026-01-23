using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

public enum CameraState {
    AttachedToPlayer,
    IsDetaching, //The camera is moving to a static location
    Detached, //The camera in a static location
    ManualControl // or any other states you need
}

//Controls the different camera states
public class CameraController : MonoBehaviour {
    private CameraState state = CameraState.AttachedToPlayer;
    private Camera cam;
    private Tween tweenMove, tweenOrtho, tweenShake;
    private const float ReattachDuration = 0.5f;
    public float defaultOrthoSize = 3.5f;

    private void Start() {
        cam = GetComponent<Camera>();
    }

    void Update() {
        if (state == CameraState.Detached && Globals.PlayerController.IsPlayerMoving()) {
            ReattachCamera();
        }
    }

    public void DetachCamera(Transform cameraFocus, float cameraFocusSize, bool reattachOnMove, float duration = ReattachDuration) {
        state = CameraState.IsDetaching;
        cam.transform.SetParent(null);
        tweenMove?.Kill();
        tweenOrtho?.Kill();

        if (duration == 0) {
            cam.transform.position = new Vector3(cameraFocus.position.x, cameraFocus.position.y, -10f);
            cam.orthographicSize = cameraFocusSize;
            state = reattachOnMove ? CameraState.Detached : CameraState.ManualControl;
        } else { //Instantly detach
            tweenMove = cam.transform.DOMove(new Vector3(cameraFocus.position.x, cameraFocus.position.y, -10f), duration).OnComplete(() => {
                state = reattachOnMove ? CameraState.Detached : CameraState.ManualControl;
            }).SetEase(Ease.OutCubic);
            tweenOrtho = cam.DOOrthoSize(cameraFocusSize, duration).SetEase(Ease.OutCubic);
        }
    }

    public void ReattachCamera(float duration = ReattachDuration) {
        if (state == CameraState.ManualControl || state == CameraState.Detached || state == CameraState.IsDetaching) {
            state = CameraState.AttachedToPlayer;
            tweenMove?.Kill();
            tweenOrtho?.Kill();
            if (duration == 0) { //Instantly attach
                cam.transform.SetParent(Globals.PlayerController.transform);
                cam.transform.localPosition = new Vector3(0, 0, -10);
                cam.orthographicSize = defaultOrthoSize;
            } else {
                tweenMove = Utils.DOMoveInTargetLocalSpace(cam.transform, Globals.PlayerController.transform, new Vector3(0, 0, -10), duration).OnComplete(() => {
                    cam.transform.SetParent(Globals.PlayerController.transform);
                    cam.transform.localPosition = new Vector3(0, 0, -10);
                }).SetEase(Ease.OutCubic);
                tweenOrtho = cam.DOOrthoSize(defaultOrthoSize, duration).SetEase(Ease.OutCubic);
            }
        }
    }
    
    public void CameraShake(float duration = 1f, float strength = 0.2f, int vibrato = 10, float randomness = 90f, bool fadeOut = true, bool ignoreTimeScale = true)
    {
        // stop any existing shake
        tweenShake?.Kill();

        if (cam == null) cam = GetComponent<Camera>();

        // 2D shake: only X/Y
        var shakeStrength = new Vector3(strength, strength, 0f);

        // DOShakePosition will return the transform to its original position after finishing
        tweenShake = cam.transform.DOShakePosition(duration, shakeStrength, vibrato, randomness, false, fadeOut).SetEase(Ease.OutQuad);

        // ignore Unity timeScale (useful when game is paused)
        if (ignoreTimeScale) tweenShake.SetUpdate(true);
    }
}
