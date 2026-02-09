using UnityEngine;

public class BonusObject : MonoBehaviour
{
    public FortuneGameManager fortuneGameManager;

    [Header("移動設定")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float destroyX = 10f; // 超過畫面右側就停止

    [Header("獎勵設定")]
    [SerializeField] private int amount;
    [SerializeField] private int score;

    RectTransform rectTransform;

    private bool isMoving = true;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        StopAndDisable();
    }

    public void Init(int amount, int score)
    {
        this.amount = amount;
        this.score = score;
    }

    private void Update()
    {
        if (!isMoving) return;

        // 向右移動
        rectTransform.anchoredPosition += Vector2.right * moveSpeed * Time.deltaTime;

        if (rectTransform.anchoredPosition.x > destroyX)
        {
            StopAndDisable();
        }
    }

    public void OnClick()
    {
        fortuneGameManager.SpawnMoneyAtPosition(rectTransform.anchoredPosition, amount, score);
        StopAndDisable();
    }

    private void StopAndDisable()
    {
        isMoving = false;
        gameObject.SetActive(false);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        isMoving = true;
    }

    public void SetPosition(Vector2 position)
    {
        rectTransform.anchoredPosition = position;
    }
}
