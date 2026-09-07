using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UIGridRenderer : Graphic
{
    [Header("--- Grid Settings ---")]
    public int gridColumns = 8;  // 가로 8칸
    public int gridRows = 4;     // 세로 4칸
    public float thickness = 2f; // 선 두께

    protected override void OnEnable()
    {
        base.OnEnable();
        // 🟢 오브젝트가 켜질 때 UI 갱신 강제 수행
        SetVerticesDirty();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        // 🟢 해상도나 부모 패널 크기가 바뀔 때 실시간으로 그리드 다시 그리기
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        // 🟢 rectTransform.rect 대신 GetLocalCorners로 실제 스크린 상의 정확한 폭과 높이 가져오기
        Vector3[] corners = new Vector3[4];
        rectTransform.GetLocalCorners(corners);

        float width = corners[2].x - corners[0].x;   // 우측 상단 X - 좌측 하단 X
        float height = corners[2].y - corners[0].y;  // 우측 상단 Y - 좌측 하단 Y

        if (width <= 0 || height <= 0 || gridColumns <= 0 || gridRows <= 0) return;

        float cellWidth = width / (float)gridColumns;
        float cellHeight = height / (float)gridRows;

        int count = 0;

        // 세로선 9개 (0 ~ 8)
        for (int i = 0; i <= gridColumns; i++)
        {
            float x = corners[0].x + (i * cellWidth);
            DrawLine(vh, new Vector2(x, corners[0].y), new Vector2(x, corners[2].y), ref count);
        }

        // 가로선 5개 (0 ~ 4)
        for (int j = 0; j <= gridRows; j++)
        {
            float y = corners[0].y + (j * cellHeight);
            DrawLine(vh, new Vector2(corners[0].x, y), new Vector2(corners[2].x, y), ref count);
        }
    }

    private void DrawLine(VertexHelper vh, Vector2 point1, Vector2 point2, ref int count)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        Vector2 dir = (point2 - point1).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (thickness * 0.5f);

        vertex.position = point1 - normal; vh.AddVert(vertex);
        vertex.position = point1 + normal; vh.AddVert(vertex);
        vertex.position = point2 + normal; vh.AddVert(vertex);
        vertex.position = point2 - normal; vh.AddVert(vertex);

        vh.AddTriangle(count, count + 1, count + 2);
        vh.AddTriangle(count + 2, count + 3, count);
        count += 4;
    }
}