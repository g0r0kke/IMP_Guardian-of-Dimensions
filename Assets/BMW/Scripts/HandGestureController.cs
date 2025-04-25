using ManoMotion;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandGestureController : MonoBehaviour
{

    [Header("Hand gesture status checker")]
    public bool isBasicAttackGesture = false;
    public bool isUltimateAttackGesture = false;
    public bool isDefenseGesture = false;
    public bool isHealingGesture = false;

    [Header("Ultimate Attack Skill - Hand Gesture Status checker")]
    [SerializeField] private bool isGrabbing = false;
    [SerializeField] private float grabTime = 0;

    [Header("Heal Skill - Hand Gesture Status checker")]
    [SerializeField] private bool isPinching = false;
    [SerializeField] private float pinchTime = 0;

    // Setting up an external script connection
    private PlayerDataManager playerDataManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerDataManager = PlayerDataManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDataManager.isControlPlayer) return;

        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null || ManoMotionManager.Instance.HandInfos.ToList().Count == 0)
        {
            ResetGrab();
            return;
        }
        ManoMotionManager.Instance.ShouldCalculateGestures(true);

        foreach (var handInfo in ManoMotionManager.Instance.HandInfos)
        {

            if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.CLICK)
            {
                isBasicAttackGesture = true;
                ResetGrab();
                ResetPinch();
            }
            else if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.RELEASE_GESTURE)
            {
                ResetGrab();
                ResetPinch();
            }
            else if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.GRAB_GESTURE)
            {
                isGrabbing = true;
                ResetPinch();
            }
            else if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.PICK)
            {
                isPinching = true;
                ResetGrab();
            }
            else if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.DROP)
            {
                if (pinchTime >= 2f) isHealingGesture = true;
                ResetGrab();
                ResetPinch();
            }
            else if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.SWIPE_RIGHT ||
                     handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.SWIPE_LEFT ||
                     handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.SWIPE_UP ||
                     handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.SWIPE_DOWN)
            {
                isDefenseGesture = true;
                ResetGrab();
                ResetPinch();
            }
        }
        if (isGrabbing) {
            grabTime += Time.deltaTime;
            if (grabTime >= 2f) isUltimateAttackGesture = true;
        }
        if (isPinching) pinchTime += Time.deltaTime;
    }

    private void ResetGrab()
    {
        isGrabbing = false;
        grabTime = 0;
    }
    private void ResetPinch()
    {
        isPinching = false;
        pinchTime = 0;
    }
}
