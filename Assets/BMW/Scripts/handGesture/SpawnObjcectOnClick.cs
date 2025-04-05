using ManoMotion;
using UnityEngine;

public class SpawnObjectOnClick : MonoBehaviour
{
    [SerializeField]
    private GameObject[] arObject;
    private int currentIndex = 0;

    void Update()
    {
        if (ManoMotionManager.Instance == null || ManoMotionManager.Instance.HandInfos == null)
            return;

        // ��� ������ �� ��ȸ
        foreach (var handInfo in ManoMotionManager.Instance.HandInfos)
        {
            if (handInfo.gestureInfo.manoGestureTrigger == ManoGestureTrigger.CLICK)
            {
                SpawnObject(handInfo);
                Debug.Log("��ü�� �����Ǿ����ϴ�.");
            }
        }
    }

    private void SpawnObject(HandInfo targetHandInfo)
    {
        TrackingInfo trackingInfo = targetHandInfo.trackingInfo;

        // ���� ��ġ ��ȿ�� �˻�
        if (trackingInfo.skeleton.jointPositions.Length <= 8)
            return;

        Vector3 jointPosition = ManoUtils.Instance.CalculateNewPositionWithDepth(
            new Vector3(
                trackingInfo.skeleton.jointPositions[8].x,
                trackingInfo.skeleton.jointPositions[8].y,
                trackingInfo.skeleton.jointPositions[8].z),
            trackingInfo.depthEstimation);

        if (currentIndex < arObject.Length)
        {
            Instantiate(arObject[currentIndex], jointPosition, Quaternion.identity);
            Handheld.Vibrate();
        }
    }
}
