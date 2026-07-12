using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class MeletupHazardStrip : MonoBehaviour
{
    [Header("Layout")]
    public float startX = 194f;
    public float spacing = 3.4f;
    public int hazardCount = 6;
    public float groundY = -4.35f;

    [Header("Visual")]
    public Sprite baseSprite;
    public Sprite warningSprite;
    public Sprite activeSprite;
    public RuntimeAnimatorController activeAnimatorController;
    public Vector3 baseOffset = new Vector3(-0.62f, -0.2f, 0f);
    public Vector3 baseScale = new Vector3(0.28f, 0.28f, 1f);
    public Vector3 flameOffset = new Vector3(-0.68f, 0.45f, 0f);
    public Vector3 flameScale = new Vector3(0.5f, 0.5f, 1f);

    [Header("Timing")]
    public float warningTime = 0.35f;
    public float activeTime = 0.85f;
    public float inactiveTime = 1.4f;
    public float staggerDelay = 0.18f;

    [Header("Damage")]
    public int damage = 1;
    public string playerLayerName = "Player";

    [Header("Hazard Collider")]
    public Vector2 colliderOffset = new Vector2(-0.62f, 0.62f);
    public Vector2 colliderSize = new Vector2(1f, 1.35f);

    private const string GeneratedPrefix = "MeletupVent_";
    private bool rebuildQueued;

    private void OnEnable()
    {
        QueueRebuild();
    }

    private void OnValidate()
    {
        hazardCount = Mathf.Max(1, hazardCount);
        spacing = Mathf.Max(0.5f, spacing);
        QueueRebuild();
    }

    private void QueueRebuild()
    {
        if (!isActiveAndEnabled)
            return;

        if (Application.isPlaying)
        {
            RebuildStrip();
            return;
        }

#if UNITY_EDITOR
        if (rebuildQueued)
            return;

        rebuildQueued = true;
        EditorApplication.delayCall += DelayedEditorRebuild;
#endif
    }

#if UNITY_EDITOR
    private void DelayedEditorRebuild()
    {
        rebuildQueued = false;

        if (this == null || !isActiveAndEnabled)
            return;

        RebuildStrip();
    }
#endif

    [ContextMenu("Rebuild Meletup Hazard Strip")]
    public void RebuildStrip()
    {
        if (!isActiveAndEnabled)
            return;

        ClearGeneratedChildren();

        for (int i = 0; i < hazardCount; i++)
        {
            float x = startX + spacing * i;
            CreateHazard(i + 1, new Vector3(x, groundY, 0f), i * staggerDelay);
        }
    }

    private void CreateHazard(int index, Vector3 position, float startDelay)
    {
        GameObject hazard = new GameObject($"{GeneratedPrefix}{index:00}");
        hazard.transform.SetParent(transform, false);
        hazard.transform.position = position;

        CreateVisual("WatuApi", hazard.transform, baseSprite, baseOffset, baseScale, 5);

        GameObject warning = null;
        if (warningSprite != null)
            warning = CreateVisual("Warning", hazard.transform, warningSprite, flameOffset, flameScale, 6);

        GameObject active = CreateVisual("Active", hazard.transform, activeSprite, flameOffset, flameScale, 7);
        if (Application.isPlaying && activeAnimatorController != null)
        {
            Animator animator = active.AddComponent<Animator>();
            animator.runtimeAnimatorController = activeAnimatorController;
        }

        if (warning != null)
            warning.SetActive(false);

        active.SetActive(false);

        BoxCollider2D collider = hazard.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.offset = colliderOffset;
        collider.size = colliderSize;

        PeriodicHazard2D periodic = hazard.AddComponent<PeriodicHazard2D>();
        periodic.enabled = false;
        periodic.startDelay = startDelay;
        periodic.warningTime = warningTime;
        periodic.activeTime = activeTime;
        periodic.inactiveTime = inactiveTime;
        periodic.damage = damage;
        periodic.playerLayerName = playerLayerName;
        periodic.warningVisual = warning;
        periodic.activeVisual = active;
        periodic.enabled = true;
    }

    private GameObject CreateVisual(string name, Transform parent, Sprite sprite, Vector3 localPosition, Vector3 scale, int sortingOrder)
    {
        GameObject visual = new GameObject(name);
        visual.transform.SetParent(parent, false);
        visual.transform.localPosition = localPosition;
        visual.transform.localScale = scale;

        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;

        return visual;
    }

    private void ClearGeneratedChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (!child.name.StartsWith(GeneratedPrefix))
                continue;

            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }
    }
}
