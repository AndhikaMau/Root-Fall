using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class TutorialStageLayoutApplier : MonoBehaviour
{
    [Header("Auto Layout")]
    public bool applyOnEnable;
    public bool cleanupGeneratedOnEnable = true;
    public bool createPlaceholders;

    [Header("Generated Object Names")]
    public string generatedParentName = "TutorialStage_LayoutGenerated";
    public string citizenHousePrefix = "RumahWarga_Placeholder";
    public string buildingPlatformPrefix = "BuildingPlatform_Placeholder";

    [Header("Platform Collider")]
    public Vector2 platformColliderSize = new Vector2(1.45f, 0.18f);
    public Vector2 platformColliderOffset = new Vector2(0f, 0.39f);

    private void OnEnable()
    {
        if (cleanupGeneratedOnEnable)
            ClearGeneratedLayout();

        if (applyOnEnable)
            ApplyLayout();
    }

    [ContextMenu("Clear Generated Layout")]
    public void ClearGeneratedLayout()
    {
        GameObject generatedParent = GameObject.Find(generatedParentName);
        if (generatedParent == null)
            return;

        if (Application.isPlaying)
            Destroy(generatedParent);
        else
            DestroyImmediate(generatedParent);
    }

    [ContextMenu("Apply Tutorial Stage Layout")]
    public void ApplyLayout()
    {
        ArrangePurpleBuildingsAsBackground();

        if (!createPlaceholders)
            return;

        Sprite houseSprite = FindSprite("ground2_dirt");
        Sprite platformSprite = FindSprite("ground1-stonesteps");

        Transform parent = EnsureParent();
        CreateCitizenHouses(parent, houseSprite);
        CreateBuildingPlatforms(parent, platformSprite);
    }

    private void ArrangePurpleBuildingsAsBackground()
    {
        List<SpriteRenderer> buildings = FindRenderers("building2-");
        float[] xPositions = { -12.5f, -7.5f, -2.5f, 2.5f, 7.5f, 12.5f, 17.5f, 22.5f, 27.5f, 32.5f, 37.5f, 42.5f };

        for (int i = 0; i < buildings.Count; i++)
        {
            SpriteRenderer renderer = buildings[i];
            if (renderer == null)
                continue;

            Transform target = renderer.transform;
            float yOffset = i % 3 == 1 ? 0.35f : 0f;
            target.position = new Vector3(xPositions[i % xPositions.Length], 1.45f + yOffset, 8f);
            target.localScale = Vector3.one * (i % 3 == 1 ? 1.65f : 1.45f);
            renderer.sortingOrder = -25;
        }
    }

    private void CreateCitizenHouses(Transform parent, Sprite sprite)
    {
        Vector3[] positions =
        {
            new Vector3(-8.6f, -0.22f, 4f),
            new Vector3(-5.4f, -0.22f, 4f),
            new Vector3(-2.2f, -0.22f, 4f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject house = EnsureChild(parent, $"{citizenHousePrefix}_{i + 1}");
            ConfigureSpriteObject(house, sprite, positions[i], new Vector3(0.45f, 0.38f, 1f), -8);
        }
    }

    private void CreateBuildingPlatforms(Transform parent, Sprite sprite)
    {
        Vector3[] positions =
        {
            new Vector3(4.5f, -0.08f, 2f),
            new Vector3(7.0f, 0.18f, 2f),
            new Vector3(9.6f, -0.08f, 2f),
            new Vector3(12.2f, 0.28f, 2f),
            new Vector3(15.0f, 0.55f, 2f)
        };

        Vector3[] scales =
        {
            new Vector3(0.42f, 0.32f, 1f),
            new Vector3(0.36f, 0.28f, 1f),
            new Vector3(0.36f, 0.26f, 1f),
            new Vector3(0.42f, 0.30f, 1f),
            new Vector3(0.48f, 0.36f, 1f)
        };

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject platform = EnsureChild(parent, $"{buildingPlatformPrefix}_{i + 1}");
            ConfigureSpriteObject(platform, sprite, positions[i], scales[i], -2);
            SetLayerRecursively(platform, LayerMask.NameToLayer("Ground"));
            ConfigurePlatformCollider(platform);
        }
    }

    private void ConfigureSpriteObject(GameObject target, Sprite sprite, Vector3 position, Vector3 scale, int sortingOrder)
    {
        target.transform.position = position;
        target.transform.localScale = scale;

        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer == null)
            renderer = target.AddComponent<SpriteRenderer>();

        renderer.sprite = sprite;
        renderer.sortingOrder = sortingOrder;
    }

    private void ConfigurePlatformCollider(GameObject target)
    {
        BoxCollider2D collider = target.GetComponent<BoxCollider2D>();
        if (collider == null)
            collider = target.AddComponent<BoxCollider2D>();

        collider.isTrigger = false;
        collider.size = platformColliderSize;
        collider.offset = platformColliderOffset;
    }

    private Transform EnsureParent()
    {
        GameObject parent = GameObject.Find(generatedParentName);
        if (parent == null)
            parent = new GameObject(generatedParentName);

        return parent.transform;
    }

    private GameObject EnsureChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child != null)
            return child.gameObject;

        GameObject created = new GameObject(childName);
        created.transform.SetParent(parent);
        return created;
    }

    private void SetLayerRecursively(GameObject target, int layer)
    {
        if (target == null || layer < 0)
            return;

        target.layer = layer;

        foreach (Transform child in target.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    private Sprite FindSprite(string objectNamePart)
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && renderer.sprite != null && renderer.name.Contains(objectNamePart))
                return renderer.sprite;
        }

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && renderer.sprite != null && renderer.sprite.name.Contains(objectNamePart))
                return renderer.sprite;
        }

        return null;
    }

    private List<SpriteRenderer> FindRenderers(string namePrefix)
    {
        SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include);
        List<SpriteRenderer> result = new List<SpriteRenderer>();

        foreach (SpriteRenderer renderer in renderers)
        {
            if (renderer != null && renderer.name.StartsWith(namePrefix))
                result.Add(renderer);
        }

        result.Sort((left, right) => string.CompareOrdinal(left.name, right.name));
        return result;
    }
}
