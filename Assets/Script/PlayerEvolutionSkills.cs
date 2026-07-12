using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class PlayerEvolutionSkills : MonoBehaviour
{
    [Header("Evolution Visual")]
    public SpriteRenderer spriteRenderer;
    public Sprite evolvedSprite;
    public RuntimeAnimatorController evolvedAnimatorController;
    public Color normalColor = Color.white;
    public Color evolvedColor = new Color(1f, 0.95f, 0.55f, 1f);

    [Header("Stage 2 Darkness")]
    public bool dimStage2OnStart = true;
    public string dimSceneName = "Stage 2";
    public Color dimAmbientColor = new Color(0.18f, 0.18f, 0.22f, 1f);
    public Light2D stageGlobalLight;
    public float dimGlobalLightIntensity = 0.22f;
    public Color dimGlobalLightColor = new Color(0.45f, 0.5f, 0.65f, 1f);

    [Header("Light Skill")]
    public KeyCode lightKey = KeyCode.Q;
    public bool hasLightSkill;
    public Light2D playerLight;
    public float lightIntensity = 1.8f;
    public float lightOuterRadius = 6f;
    public float lightInnerRadius = 1.5f;
    public Color lightColor = new Color(1f, 0.9f, 0.45f, 1f);

    [Header("Radar Skill")]
    public bool hasRadarSkill;
    public GameObject radarObject;

    private Animator animator;
    private RuntimeAnimatorController normalAnimatorController;
    private Sprite normalSprite;
    private bool lightOn;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();

        if (animator != null)
            normalAnimatorController = animator.runtimeAnimatorController;

        if (spriteRenderer != null)
            normalSprite = spriteRenderer.sprite;
        EnsurePlayerLight();
        ApplyStageLighting();
        ApplySavedEvolutionState();
    }

    private void Update()
    {
        if (!hasLightSkill || Time.timeScale == 0f)
            return;

        if (Input.GetKeyDown(lightKey))
            SetLightActive(!lightOn);
    }

    public void UnlockEvolution()
    {
        hasLightSkill = true;
        hasRadarSkill = true;
        ApplyEvolutionVisual(true);
        SetLightActive(false);

        if (radarObject != null)
            radarObject.SetActive(true);
    }

    public bool HasLightSkill()
    {
        return hasLightSkill;
    }

    public bool HasRadarSkill()
    {
        return hasRadarSkill;
    }

    private void ApplySavedEvolutionState()
    {
        if (!ExpProgress.HasEvolved)
        {
            hasLightSkill = false;
            hasRadarSkill = false;
            ApplyEvolutionVisual(false);
            SetLightActive(false);

            if (radarObject != null)
                radarObject.SetActive(false);

            return;
        }

        UnlockEvolution();
    }

    private void ApplyEvolutionVisual(bool evolved)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = evolved ? evolvedColor : normalColor;

            if (evolved && evolvedSprite != null)
                spriteRenderer.sprite = evolvedSprite;
            else if (!evolved && normalSprite != null)
                spriteRenderer.sprite = normalSprite;
        }

        if (animator == null)
            return;

        if (evolved && evolvedAnimatorController != null)
            animator.runtimeAnimatorController = evolvedAnimatorController;
        else if (!evolved && normalAnimatorController != null)
            animator.runtimeAnimatorController = normalAnimatorController;
    }

    private void EnsurePlayerLight()
    {
        if (playerLight == null)
            playerLight = GetComponentInChildren<Light2D>(true);

        if (playerLight == null)
        {
            GameObject lightObject = new GameObject("SkillLight_Q");
            lightObject.transform.SetParent(transform);
            lightObject.transform.localPosition = Vector3.zero;
            playerLight = lightObject.AddComponent<Light2D>();
            playerLight.lightType = Light2D.LightType.Point;
        }

        playerLight.color = lightColor;
        playerLight.intensity = lightIntensity;
        playerLight.pointLightOuterRadius = lightOuterRadius;
        playerLight.pointLightInnerRadius = lightInnerRadius;
        SetLightActive(false);
    }

    private void SetLightActive(bool active)
    {
        lightOn = active && hasLightSkill;

        if (playerLight != null)
            playerLight.enabled = lightOn;
    }

    private void ApplyStageLighting()
    {
        if (!dimStage2OnStart)
            return;

        if (SceneManager.GetActiveScene().name != dimSceneName)
            return;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = dimAmbientColor;
        EnsureDimGlobalLight();
    }

    private void EnsureDimGlobalLight()
    {
        if (stageGlobalLight == null)
        {
            Light2D[] lights = FindObjectsByType<Light2D>(FindObjectsInactive.Exclude);

            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] != null && lights[i].lightType == Light2D.LightType.Global)
                {
                    stageGlobalLight = lights[i];
                    break;
                }
            }
        }

        if (stageGlobalLight == null)
        {
            GameObject lightObject = new GameObject("Stage2_DimGlobalLight");
            stageGlobalLight = lightObject.AddComponent<Light2D>();
            stageGlobalLight.lightType = Light2D.LightType.Global;
        }

        stageGlobalLight.color = dimGlobalLightColor;
        stageGlobalLight.intensity = dimGlobalLightIntensity;
    }
}
