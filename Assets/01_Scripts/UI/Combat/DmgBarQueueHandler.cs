using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgBarQueueHandler : MonoBehaviour
{
    private Queue<DmgTextData> queue = new();
    private bool isPlaying = false;

    [SerializeField] private float fastDelay = 0.25f;   // 연타 간 텀
    [SerializeField] private float slowDelay = 0.6f;    // 일반 텀
    [SerializeField] private float fastThreshold = 0.3f; // 몇 초 이내면 연타로 간주
    [SerializeField] private float verticalOffset = 1.0f;

    private float lastEnqueueTime = -999f;

    public void Enqueue(DmgTextData data)
    {
        queue.Enqueue(data);
        if (!isPlaying)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        while (queue.Count > 0)
        {
            isPlaying = true;
            var data = queue.Dequeue();

            // 출력
            var dmgText = DmgPoolManager.Instance.Get();
            dmgText.Initialize(data, transform, verticalOffset);

            // 시간 간격에 따른 delay 계산
            float currentTime = Time.time;
            float delta = currentTime - lastEnqueueTime;
            float delay = delta < fastThreshold ? fastDelay : slowDelay;
            lastEnqueueTime = currentTime;

            yield return new WaitForSeconds(delay);
        }

        isPlaying = false;
    }
}
