using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DmgBarQueueHandler : MonoBehaviour
{
    private Queue<(DmgTextData data, float enqueuedTime)> queue = new();
    [SerializeField] private DmgBarDisplay dmgDisplay;
    private bool isPlaying = false;

    [SerializeField] private float fastDelay = 0.25f;   // 연타 간 텀
    [SerializeField] private float slowDelay = 0.6f;    // 일반 텀
    [SerializeField] private float fastThreshold = 0.3f; // 몇 초 이내면 연타로 간주
    [SerializeField] private float verticalOffset = 1.0f;

    private float lastPlayTime = -999f;

    public void Enqueue(DmgTextData data)
    {
        dmgDisplay.Initialize(data, transform, verticalOffset);
        // 데미지 프린트 방식 변경으로 사용 안함
        /*
        queue.Enqueue((data, Time.time)); // enqueue 시점의 시간 저장
        if (!isPlaying)
            StartCoroutine(PlayQueue());*/
    }

    private IEnumerator PlayQueue()
    {
        while (queue.Count > 0)
        {
            isPlaying = true;
            var (data, enqueuedTime) = queue.Dequeue();

            // 출력
            var dmgText = DmgPoolManager.Instance.Get();
            dmgText.Initialize(data, transform, verticalOffset);

            // 마지막 재생 시간과의 차이로 delay 계산
            float currentTime = Time.time;
            float delta = currentTime - lastPlayTime;
            float delay = delta < fastThreshold ? 0f : slowDelay; // 연타의 경우 텀 없이 바로 재생하게 변경 => 2026.01.02
            lastPlayTime = currentTime;

            yield return new WaitForSeconds(delay);
        }

        isPlaying = false;
    }
}
