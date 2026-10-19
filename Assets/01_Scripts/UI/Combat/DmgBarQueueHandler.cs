using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgBarQueueHandler : MonoBehaviour
{
    private Queue<DmgTextData> queue = new();
    [SerializeField] private DmgBarDisplay dmgDisplay;
    private bool isPlaying = false;

    [SerializeField] private float queueDelay = 0.4f;   // 연속 출력 텀
    [SerializeField] private float verticalOffset = 1.0f;

    private float lastPlayTime = -999f;  // 마지막 출력 시간

    /// <summary>
    /// 즉시 출력 데미지 프린트
    /// </summary>
    /// <param name="data"></param>
    public void InitPrint(DmgTextData data)
    {
        dmgDisplay.Initialize(data, transform, verticalOffset);
    }
    /// <summary>
    /// 버프/디버프 류의 출력 시 텀을 두고 출력
    /// </summary>
    /// <param name="data"></param>
    public void DmgEnqueue(DmgTextData data)
    {
        queue.Enqueue(data);
        if (!isPlaying)
        {
            isPlaying = true;
            StartCoroutine(PlayQueue());
        }
    }

    private IEnumerator PlayQueue()
    {
        while (queue.Count > 0)
        {
            isPlaying = true;
            var data = queue.Dequeue();
            
            // 마지막 출력 시간으로부터 0.4초 경과했는지 확인 (연속으로 버프/디버프류 출력시 0.4초씩의 텀 제공)
            float timeSinceLastPlay = Time.time - lastPlayTime;
            if (timeSinceLastPlay < queueDelay)
            {
                // 남은 시간 만큼 대기
                yield return new WaitForSeconds(queueDelay - timeSinceLastPlay);
            }
            
            // 출력
            dmgDisplay.Initialize(data, transform, verticalOffset);
            lastPlayTime = Time.time;
        }

        isPlaying = false;
    }
}
