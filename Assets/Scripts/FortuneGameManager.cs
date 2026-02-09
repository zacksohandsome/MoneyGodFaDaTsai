using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FortuneGameManager : MonoBehaviour
{
    private enum FortuneState
    {
        Normal,
        Excited,   // 60% 以上
        Frenzy     // 85% 以上
    }

    [Header("Fortune Settings")]
    [SerializeField] private float fortuneMax = 300;
    [SerializeField] private float clickValue = 8f;
    [SerializeField] private float holdValue = 2f;

    [Header("Hold Setting")]
    [SerializeField] private float holdThreshold = 1f;
    [SerializeField] private float holdSpawnInterval = 0.2f;
    [SerializeField] private float holdValuePerSecond = 40f;
    [SerializeField] private int holdMoneyAmount = 1;

    [Header("Score Setting")]
    [SerializeField] private int clickScorePerMoney = 10;
    [SerializeField] private int holdScorePerMoney = 5;
    [SerializeField] private int burstScorePerMoney = 50;

    [Header("Spawn Amount")]
    [SerializeField] private int clickSpawnAmount = 5;
    [SerializeField] private int holdSpawnAmount = 1;
    [SerializeField] private int burstSpawnAmount = 50;

    [Header("Burst Settings")]
    [SerializeField] private int burstReward = 1000;
    [SerializeField] private float burstRemainRatio = 0.15f; // 爆發後保留多少財氣

    [Header("Visual")]
    [SerializeField] private RectTransform moneySpawnPoint;
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private int poolSize = 100;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private CanvasGroup flashCanvas;
    [SerializeField] private float flyHeight = 80f;

    [Header("Blessing Setting")]
    [SerializeField] private TextMeshProUGUI blessCostText;
    [SerializeField] private int blessingCost = 10000;
    [SerializeField] private BlessingUI blessingUI; // 拖UI進來
    [SerializeField] private float blessingDuration = 2f;
    [SerializeField]
    private List<string> horseYearBlessings = new List<string>()
    {
        "馬到成功",
        "龍馬精神",
        "一馬當先",
        "馬上發財",
        "萬馬奔騰",
        "天馬行空",
        "馬運亨通",
        "馬年行大運",
        "駿馬騰飛",
        "馬上如意"
    };

    private Queue<RectTransform> moneyPool = new Queue<RectTransform>();

    private float fortune = 0f;
    private int score = 0;
    private int scoreMax = 100000000;

    private bool isHolding = false;
    private bool isBursting = false;

    private FortuneState currentState;

    private RectTransform scoreTarget;

    private Coroutine scorePunchCoroutine;
    private Coroutine blessingCoroutine;
    private Vector3 scoreBaseScale;
    private float lastPunchTime;
    private float punchCooldown = 0.05f;
    private float holdSpawnTimer;

    private bool isPointerDown;
    private float pointerDownTime;
    private float holdTimer;

    private void Start()
    {
        InitPool();
        scoreTarget = scoreText.GetComponent<RectTransform>();
        scoreBaseScale = scoreTarget.localScale;
        blessCostText.text = blessingCost.ToString("N0");
        UpdateScoreUI();
    }

    

    private void Update()
    {
        HandleHoldCheck();
        HandleHoldSpawn();

        CheckBurst();
        UpdateFortuneState();
    }

    // ====== UI Button 點擊 ======
    public void OnClick()
    {
        float ratio = fortune / fortuneMax;
        float dynamicValue = Mathf.Lerp(clickValue, clickValue * 2f, ratio);
        AddFortune(dynamicValue);

        SpawnMoney(clickSpawnAmount, clickScorePerMoney);
    }

    // ====== 長按支援 ======
    public void OnPointerDown()
    {
        isPointerDown = true;
        pointerDownTime = Time.time;
    }

    public void OnPointerUp()
    {
        if (!isPointerDown) return;

        float heldTime = Time.time - pointerDownTime;

        if (heldTime < holdThreshold)
        {
            // 點擊
            OnClick();
        }

        ResetHoldState();
    }

    private void HandleHoldCheck()
    {
        if (!isPointerDown || isHolding) return;

        if (Time.time - pointerDownTime >= holdThreshold)
        {
            isHolding = true;
            holdSpawnTimer = 0f;
        }
    }

    private void HandleHoldSpawn()
    {
        if (!isHolding) return;

        holdSpawnTimer += Time.deltaTime;

        if (holdSpawnTimer >= holdSpawnInterval)
        {
            holdSpawnTimer = 0f;
            SpawnMoney(holdSpawnAmount, holdScorePerMoney);

            float ratio = fortune / fortuneMax;
            float dynamicValue = Mathf.Lerp(holdValue, holdValue * 2f, ratio);
            AddFortune(dynamicValue);
        }
    }

    private void ResetHoldState()
    {
        isPointerDown = false;
        isHolding = false;
    }

    // ====== 財氣處理 ======
    private void AddFortune(float amount)
    {
        fortune += amount;
        fortune = Mathf.Clamp(fortune, 0, fortuneMax);

        float ratio = fortune / fortuneMax;

        int barCount = Mathf.RoundToInt(ratio * 20);
        string bar = new string('|', barCount);

        Debug.Log($"財氣 [{bar}] {fortune:0}/{fortuneMax}");
    }

    private void CheckBurst()
    {
        if (!isBursting && fortune >= fortuneMax)
        {
            StartCoroutine(BurstRoutine());
        }
    }

    private IEnumerator BurstRoutine()
    {
        isBursting = true;

        Debug.Log("財氣凝聚中...");
        yield return new WaitForSeconds(0.2f);

        AddScore(burstReward);

        Debug.Log("天降橫財！！！");

        yield return StartCoroutine(FlashRoutine());
        SpawnMoney(50, burstScorePerMoney); // 大量錢飛

        fortune = fortuneMax * burstRemainRatio;

        isBursting = false;
    }

    private void UpdateFortuneState()
    {
        float ratio = fortune / fortuneMax;

        if (ratio >= 0.85f)
        {
            SetState(FortuneState.Frenzy);
        }
        else if (ratio >= 0.6f)
        {
            SetState(FortuneState.Excited);
        }
        else
        {
            SetState(FortuneState.Normal);
        }
    }

    private void SetState(FortuneState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case FortuneState.Normal:
                Debug.Log("財氣累積中");
                break;

            case FortuneState.Excited:
                Debug.Log("財氣開始躁動...");
                break;

            case FortuneState.Frenzy:
                Debug.Log("財氣即將爆發！！");
                break;
        }
    }

    private void SpawnMoney(int amount, int scorePerMoney)
    {
        for (int i = 0; i < amount; i++)
        {
            RectTransform rt = GetMoney();
            rt.anchoredPosition = Vector2.zero;

            // rotate money randomly
            rt.localEulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));

            Vector2 randomDir = new Vector2(
                Random.Range(-200f, 200f),
                Random.Range(100f, 400f)
            );

            StartCoroutine(FlyMoney(rt, randomDir, scorePerMoney));
        }
    }

    private IEnumerator FlyMoney(RectTransform rt, Vector2 targetOffset, int score)
    {
        Vector2 startPos = Vector2.zero;
        Vector2 midPos = startPos + targetOffset;

        float duration = 0.6f;
        float time = 0f;

        // 第一段：拋物線
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            float height = Mathf.Sin(t * Mathf.PI) * flyHeight;

            rt.anchoredPosition =
                Vector2.Lerp(startPos, midPos, t) +
                Vector2.up * height;

            yield return null;
        }

        // 第二段：吸附到分數
        Vector2 worldTarget = scoreTarget.position;
        duration = 0.3f;
        time = 0f;

        Vector2 startWorld = rt.position;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            rt.position = Vector2.Lerp(startWorld, worldTarget, t);
            yield return null;
        }

        // ⭐ 在這裡加分
        AddScore(score);

        // ⭐ 分數 UI 小彈一下
        PlayScorePunch();

        ReturnMoney(rt);
    }

    private void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(moneyPrefab, moneySpawnPoint);
            obj.SetActive(false);
            moneyPool.Enqueue(obj.GetComponent<RectTransform>());
        }
    }

    private RectTransform GetMoney()
    {
        if (moneyPool.Count > 0)
        {
            RectTransform rt = moneyPool.Dequeue();
            rt.gameObject.SetActive(true);
            return rt;
        }

        GameObject obj = Instantiate(moneyPrefab, moneySpawnPoint);
        return obj.GetComponent<RectTransform>();
    }

    private void ReturnMoney(RectTransform rt)
    {
        rt.gameObject.SetActive(false);
        moneyPool.Enqueue(rt);
    }

    private IEnumerator FlashRoutine()
    {
        float time = 0f;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            flashCanvas.alpha = Mathf.Lerp(0f, 0.8f, t);
            yield return null;
        }

        time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            flashCanvas.alpha = Mathf.Lerp(0.8f, 0f, t);
            yield return null;
        }
    }

    private void AddScore(int amount)
    {
        score += amount;
        score = Mathf.Clamp(score, 0, scoreMax);
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        scoreText.text = score.ToString("N0");
    }

    private void PlayScorePunch()
    {
        if (Time.time - lastPunchTime < punchCooldown)
            return;

        lastPunchTime = Time.time;

        if (scorePunchCoroutine != null)
            StopCoroutine(scorePunchCoroutine);

        scorePunchCoroutine = StartCoroutine(ScorePunchRoutine());
    }

    private IEnumerator ScorePunchRoutine()
    {
        scoreTarget.localScale = scoreBaseScale;

        Vector3 punchScale = scoreBaseScale * 1.2f;

        float duration = 0.08f;
        float time = 0f;

        // 放大
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            scoreTarget.localScale =
                Vector3.Lerp(scoreBaseScale, punchScale, t);

            yield return null;
        }

        time = 0f;

        // 縮回
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            scoreTarget.localScale =
                Vector3.Lerp(punchScale, scoreBaseScale, t);

            yield return null;
        }

        scoreTarget.localScale = scoreBaseScale;
    }

    public void TryShowBlessing()
    {
        if (score < blessingCost)
        {
            Debug.Log("分數不足");
            return;
        }

        score -= blessingCost;

        string blessing = GetRandomBlessing();
        PlayBlessing(blessing);

        Debug.Log("剩餘分數: " + score);

        UpdateScoreUI();
    }

    private string GetRandomBlessing()
    {
        int index = Random.Range(0, horseYearBlessings.Count);
        return horseYearBlessings[index];
    }

    void PlayBlessing(string blessing)
    {
        if(blessingCoroutine != null)
        {
            StopCoroutine(blessingCoroutine);
        }
        StartCoroutine(ShowBlessingRoutine(blessing));
    }

    IEnumerator ShowBlessingRoutine(string blessing)
    {
        blessingUI.ShowBlessing(blessing);
        yield return new WaitForSeconds(2f);
        blessingUI.HideBlessing();
    }
}
