using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UISineWave : Graphic
{
    public float frequency = 5f;
    public float amplitude = 30f;
    public float phaseOffset = 0f;
    public float scrollSpeed = 5f;
    public float thickness = 3f;
    public int resolution = 150;

    void Update()
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        int safeResolution = Mathf.Max(2, resolution);
        float width = rectTransform.rect.width;
        float step = width / (float)safeResolution;

        for (int i = 0; i < safeResolution; i++)
        {
            float x1 = (i * step) - (width / 2f);
            float x2 = ((i + 1) * step) - (width / 2f);

            float normX1 = (float)i / safeResolution;
            float normX2 = (float)(i + 1) / safeResolution;

            float timeOffset = Time.time * scrollSpeed;

            float y1 = Mathf.Sin((normX1 * frequency * Mathf.PI * 2f) + phaseOffset + timeOffset) * amplitude;
            float y2 = Mathf.Sin((normX2 * frequency * Mathf.PI * 2f) + phaseOffset + timeOffset) * amplitude;

            DrawLineSegment(vh, new Vector2(x1, y1), new Vector2(x2, y2));
        }
    }

    private void DrawLineSegment(VertexHelper vh, Vector2 start, Vector2 end)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness / 2f);

        vertex.position = start - normal; vh.AddVert(vertex);
        vertex.position = start + normal; vh.AddVert(vertex);
        vertex.position = end + normal; vh.AddVert(vertex);
        vertex.position = end - normal; vh.AddVert(vertex);

        int index = vh.currentVertCount;
        vh.AddTriangle(index - 4, index - 3, index - 2);
        vh.AddTriangle(index - 4, index - 2, index - 1);
    }
}