using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<UiManager>();
            }
            return _instance;
        }
    }

    static UiManager _instance;

    public RawImage steminaRawImage;
    public Text speedText;
    public Image crosshairImage;

    public GameObject thermalTargetCursor;

    public Material crackOverlayMat;
    public Material strippedUiMat;

    public Text bulletAmmoText;
    public Text bulletWeaponCountText;
    public Text missileAmmoText;
    public Text missileWeaponCountText;

    public Text systemMessageText;

    const float defaultSystemMessageTime = 3;
    float systemMessageRemainingTime;

    public Camera bloomCanvasCamera;
    public RenderTexture bloomCanvasRenderTexture { get; private set; } = null;
    public Canvas bloomCanvas;
    CanvasScaler bloomCanvasScaler;

    public RawImage bloomCanvasRawImage;

    List<GameObject> thermalTargetCursors = new List<GameObject>();

    const float bloomCanvasAlpha = 0.75f;

    public Image darkenImage;

    public GameObject ingameBloomRoot;
    public GameObject pauseBloomRoot;
    public GameObject pauseButtonRoot;
    public Button pauseContinueButton;
    public Button pauseRestartButton;
    public Button pauseBackToTitleButton;

    void Awake() {
        thermalTargetCursor.SetActive(false);

        bloomCanvasCamera.enabled = true;

        bloomCanvasRawImage.color = new Color(1, 1, 1, bloomCanvasAlpha);

        bloomCanvasScaler = bloomCanvas.GetComponent<CanvasScaler>();

        pauseContinueButton.onClick.AddListener(
            () => GameManager.Instance.SetPause(false)
        );
        pauseRestartButton.onClick.AddListener(
            () => GameManager.Instance.Restart()
        );
        pauseBackToTitleButton.onClick.AddListener(
            () => SceneManager.LoadScene("Title")
        );
    }

    void Start() {
        SetCockpitHealth(1);
    }

    void Update() {
        if (bloomCanvasRenderTexture == null || bloomCanvasRenderTexture.width != Screen.width || bloomCanvasRenderTexture.height != Screen.height) {
            if (bloomCanvasRenderTexture != null) {
                bloomCanvasRenderTexture.Release();
            }

            bloomCanvasRenderTexture = new RenderTexture(Screen.width, Screen.height, 16, RenderTextureFormat.DefaultHDR);
            bloomCanvasRenderTexture.Create();

            Debug.Log("Created RT for bloom canvas: " + bloomCanvasCamera.pixelWidth + "x" + bloomCanvasCamera.pixelHeight);

            bloomCanvasCamera.targetTexture = bloomCanvasRenderTexture;
            bloomCanvasRawImage.texture = bloomCanvasRenderTexture;
        }

        RectTransform steminaRect = steminaRawImage.GetComponent<RectTransform>();
        strippedUiMat.SetVector("_Size", new Vector4(steminaRect.sizeDelta.x, steminaRect.sizeDelta.y, 1, 1));

        if (systemMessageRemainingTime <= 0) {
            systemMessageText.text = "";
        }
        systemMessageRemainingTime = Mathf.Max(systemMessageRemainingTime - Time.deltaTime, 0);
    }

    public void SetStemina(float maxStemina, float stemina, float requiredToBoost) {
        float full = Mathf.Max((stemina - requiredToBoost) / maxStemina, 0);
        float stripped = Mathf.Max(stemina / maxStemina);

        strippedUiMat.SetFloat("_Full", full);
        strippedUiMat.SetFloat("_Stripe", stripped);
    }

    public void SetSpeed(float speedMeterPerSec) {
        speedText.text = $"{Mathf.Round(speedMeterPerSec * (3600f / 1000f))} km/h";
    }

    public void SetCrosshairPos(Vector2 viewport) {
        RectTransform canvasRect = bloomCanvas.GetComponent<RectTransform>();

        Vector2 anchored = new Vector2(
            ((viewport.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
            ((viewport.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f))
        );

        crosshairImage.GetComponent<RectTransform>().anchoredPosition = anchored;
    }

    public void SetTargets(List<Transform> targets, Camera cam) {
        RectTransform canvasRect = bloomCanvas.GetComponent<RectTransform>();

        for (int i = 0; i < targets.Count; i++) {
            if (thermalTargetCursors.Count <= i) {
                GameObject cloned = Instantiate(thermalTargetCursor, thermalTargetCursor.transform.parent);

                // cloned.transform.parent = bloomCanvas.transform;
                // cloned.transform.localPosition = Vector3.zero;
                // cloned.transform.localScale = Vector3.one;

                thermalTargetCursors.Add(cloned);
            }

            thermalTargetCursors[i].SetActive(true);

            // @Hardcoded
            // Color color;
            // if (targets[i].type == TargetType.THERMAL) color = Color.red;
            // else if (targets[i].type == TargetType.VITAL) color = Color.blue;
            // else if (targets[i].type == TargetType.MISSILE) color = Color.green;
            // else color = Color.gray;

            // thermalTargetCursors[i].GetComponentInChildren<Image>().color = color;

            RectTransform rect = thermalTargetCursors[i].GetComponent<RectTransform>();

            Vector2 viewport = cam.WorldToViewportPoint(targets[i].position);

            Vector2 anchored = new Vector2(
                ((viewport.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
                ((viewport.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f))
            );

            rect.anchoredPosition = anchored;
        }

        // Disable remainders.
        for (int i = targets.Count; i < thermalTargetCursors.Count; i++) {
            thermalTargetCursors[i].SetActive(false);
        }
    }

    public void SetCockpitHealth(float healthRate) {
        if (healthRate >= 1f) {
            // Hide effects.
            crackOverlayMat.SetFloat("_Health", 1f);
        }
        else {
            crackOverlayMat.SetFloat("_Health", Mathf.Lerp(0f, 1f, healthRate));
        }
    }

    public void SetShowAmmo(bool show) {
        bulletAmmoText.gameObject.SetActive(show);
        bulletWeaponCountText.gameObject.SetActive(show);
        missileAmmoText.gameObject.SetActive(show);
        missileWeaponCountText.gameObject.SetActive(show);
    }

    public void SetBulletAmmo(int ammo) {
        bulletAmmoText.text = ammo.ToString();
    }

    public void SetBulletWeaponCount(int count) {
        bulletWeaponCountText.text = count.ToString();
    }

    public void SetMissileAmmo(int ammo) {
        missileAmmoText.text = ammo.ToString();
    }

    public void SetMissileWeaponCount(int count) {
        missileWeaponCountText.text = count.ToString();
    }

    public void ShowSystemMessage(string text, float time = defaultSystemMessageTime) {
        systemMessageText.text = text;
        systemMessageRemainingTime = time;
    }

    public void SetPause(bool isPaused) {
        darkenImage.enabled = isPaused;

        ingameBloomRoot.SetActive(!isPaused);
        pauseBloomRoot.SetActive(isPaused);
        pauseButtonRoot.SetActive(isPaused);
    }
}
