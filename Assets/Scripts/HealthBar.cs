using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image foreground;
    public Image delayed;
    public float delayTime = 0.5f;

    // Text fields (assign in prefab Inspector)
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI levelText;

    private Transform target;
    private float anchorOffsetY = 2f;
    private float anchorSideOffset = 0.5f; // luôn dương (bên phải)
    [Range(0f, 0.2f)] public float screenMargin = 0.02f;

    private Canvas parentCanvas;
    private RectTransform canvasRect;
    private RectTransform rect;
    private Coroutine delayCoroutine;

    // fixes for jitter:
    private float lastSideDir = 0f;
    private float sideSwitchThreshold = 30f; // pixels of hysteresis to avoid flip-flop

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        // tìm Canvas nếu chưa được gán (tên Canvas: "HealthCanvas" hoặc tự tìm Canvas đầu tiên)
        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            var c = GameObject.Find("HealthCanvas");
            if (c != null) parentCanvas = c.GetComponent<Canvas>();
        }
        if (parentCanvas != null) canvasRect = parentCanvas.GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (target == null || parentCanvas == null || canvasRect == null) return;

        // compute camera used by canvas
        Camera cam = parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera ?? Camera.main;

        // base world position (without side offset) to decide which side to put the bar on
        Vector3 worldBase = target.position + Vector3.up * anchorOffsetY;
        Vector3 screenBase = (cam != null) ? cam.WorldToScreenPoint(worldBase) : Camera.main.WorldToScreenPoint(worldBase);

        // nếu ra sau camera, ẩn
        if (screenBase.z <= 0f)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }
        else if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // decide side with hysteresis to avoid flip-flop jitter
        float half = Screen.width * 0.5f;
        float sideDir;
        if (lastSideDir == 0f)
        {
            sideDir = (screenBase.x < half) ? Mathf.Abs(anchorSideOffset) : -Mathf.Abs(anchorSideOffset);
        }
        else
        {
            if (screenBase.x < half - sideSwitchThreshold) sideDir = Mathf.Abs(anchorSideOffset);
            else if (screenBase.x > half + sideSwitchThreshold) sideDir = -Mathf.Abs(anchorSideOffset);
            else sideDir = lastSideDir; // keep previous if within hysteresis band
        }
        lastSideDir = sideDir;

        Vector3 worldPos = worldBase + Vector3.right * sideDir;
        Vector3 screenPos = (cam != null) ? cam.WorldToScreenPoint(worldPos) : Camera.main.WorldToScreenPoint(worldPos);

        // convert screen -> local point in canvas (this is the pivot anchored position we want)
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 localPoint);

        // clamp theo màn hình (theo kích thước canvas) BUT ensure ALL children (ví dụ ô level bên trái) không bị cắt
        float marginX = canvasRect.rect.width * screenMargin;
        float marginY = canvasRect.rect.height * screenMargin;

        // compute children bounds assuming we will place parent at 'localPoint' (avoid reading current world corners then moving -> removes feedback loop)
        Vector2 childrenMin, childrenMax;
        GetChildrenLocalBoundsInCanvas(localPoint, out childrenMin, out childrenMax, cam);

        float minAllowedX = canvasRect.rect.xMin + marginX;
        float maxAllowedX = canvasRect.rect.xMax - marginX;
        float minAllowedY = canvasRect.rect.yMin + marginY;
        float maxAllowedY = canvasRect.rect.yMax - marginY;

        float shiftX = 0f;
        if (childrenMin.x < minAllowedX) shiftX = minAllowedX - childrenMin.x;
        else if (childrenMax.x > maxAllowedX) shiftX = maxAllowedX - childrenMax.x;

        float shiftY = 0f;
        if (childrenMin.y < minAllowedY) shiftY = minAllowedY - childrenMin.y;
        else if (childrenMax.y > maxAllowedY) shiftY = maxAllowedY - childrenMax.y;

        // apply shift to anchored position
        localPoint.x += shiftX;
        localPoint.y += shiftY;

        rect.anchoredPosition = localPoint;

        // Không quay theo camera, không lật
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
    }

    // lấy bounds của tất cả RectTransform con (world corners -> canvas local)
    // important: giả sử parent sẽ ở 'proposedAnchored' (we apply delta to current corners) to avoid transform feedback
    private void GetChildrenLocalBoundsInCanvas(Vector2 proposedAnchored, out Vector2 minLocal, out Vector2 maxLocal, Camera cam)
    {
        minLocal = new Vector2(float.MaxValue, float.MaxValue);
        maxLocal = new Vector2(float.MinValue, float.MinValue);

        // delta in canvas local between proposed anchored position và current anchoredPosition
        Vector2 parentDelta = proposedAnchored - rect.anchoredPosition;

        RectTransform[] children = GetComponentsInChildren<RectTransform>(true);
        Camera camForScreen = cam;

        foreach (var child in children)
        {
            // get world corners of child at its current transform
            Vector3[] corners = new Vector3[4];
            child.GetWorldCorners(corners);
            for (int i = 0; i < 4; i++)
            {
                Vector3 cornerWorld = corners[i];
                // world -> screen
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(camForScreen, cornerWorld);
                // screen -> canvas local
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint);
                // apply parent shift (we assume moving parent by parentDelta simply shifts children's canvas-local points by same delta)
                localPoint += parentDelta;
                if (localPoint.x < minLocal.x) minLocal.x = localPoint.x;
                if (localPoint.y < minLocal.y) minLocal.y = localPoint.y;
                if (localPoint.x > maxLocal.x) maxLocal.x = localPoint.x;
                if (localPoint.y > maxLocal.y) maxLocal.y = localPoint.y;
            }
        }

        // fallback nếu không có gì
        if (minLocal.x == float.MaxValue)
        {
            Vector2 half = new Vector2(rect.rect.width * 0.5f, rect.rect.height * 0.5f);
            minLocal = proposedAnchored - new Vector2(rect.rect.width * rect.pivot.x, rect.rect.height * rect.pivot.y);
            maxLocal = proposedAnchored + new Vector2(rect.rect.width * (1f - rect.pivot.x), rect.rect.height * (1f - rect.pivot.y));
        }
    }

    // gọi từ Health when instantiate: truyền target transform và offsets
    public void SetTarget(Transform t, float offsetY, float sideOffset, Canvas canvas = null)
    {
        target = t;
        anchorOffsetY = offsetY;
        anchorSideOffset = Mathf.Abs(sideOffset);

        if (canvas != null)
        {
            parentCanvas = canvas;
            canvasRect = parentCanvas.GetComponent<RectTransform>();
            // nếu cần, đưa this under canvas hierarchy
            transform.SetParent(parentCanvas.transform, worldPositionStays: false);
        }
    }

    public void SetHealth(float current, float max)
    {
        float targetFill = (max <= 0f) ? 0f : Mathf.Clamp01(current / max);

        if (foreground != null) foreground.fillAmount = targetFill;

        if (delayed != null)
        {
            if (delayCoroutine != null) StopCoroutine(delayCoroutine);
            delayCoroutine = StartCoroutine(AnimateDelayed(delayed.fillAmount, targetFill));
        }
    }

    private IEnumerator AnimateDelayed(float from, float to)
    {
        float t = 0f;
        float duration = delayTime;
        while (t < duration)
        {
            t += Time.deltaTime;
            delayed.fillAmount = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        delayed.fillAmount = to;
        delayCoroutine = null;
    }

    // New: set level text (call from Health)
    public void SetLevel(int level)
    {
        if (levelText != null)
        {
            levelText.text = level.ToString();
            levelText.gameObject.SetActive(true);
        }
    }

    // New: set health text (call from Health)
    public void SetHealthText(string s)
    {
        if (healthText != null)
        {
            healthText.text = s;
            healthText.gameObject.SetActive(true);
        }
    }
}
