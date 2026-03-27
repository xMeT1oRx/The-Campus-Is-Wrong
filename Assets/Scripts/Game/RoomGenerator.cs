using UnityEngine;

/// <summary>
/// Процедурно строит классную комнату из примитивов Unity.
/// Вызывается GameManager.Start() до SpawnAnomaly().
/// </summary>
public class RoomGenerator : MonoBehaviour
{
    // ── размеры комнаты ──────────────────────────────────────────
    const float W = 10f, H = 3f, D = 12f;
    const float HW = W / 2f, HD = D / 2f;

    public void Generate()
    {
        BuildShell();
        BuildFurniture();
        PlacePlayer();
    }

    // ── оболочка ─────────────────────────────────────────────────
    void BuildShell()
    {
        // Пол
        Make("Floor",   new Vector3(0, -0.05f, 0),       new Vector3(W, 0.1f, D),    new Color(0.35f, 0.25f, 0.15f));
        // Потолок
        Make("Ceiling", new Vector3(0, H + 0.05f, 0),    new Vector3(W, 0.1f, D),    new Color(0.92f, 0.90f, 0.86f));
        // Стены
        Make("WallFront", new Vector3(0,   H/2f, -HD),   new Vector3(W, H, 0.15f),   new Color(0.86f, 0.81f, 0.73f));
        Make("WallBack",  new Vector3(0,   H/2f,  HD),   new Vector3(W, H, 0.15f),   new Color(0.86f, 0.81f, 0.73f));
        Make("WallLeft",  new Vector3(-HW, H/2f,  0),    new Vector3(0.15f, H, D),   new Color(0.86f, 0.81f, 0.73f));
        Make("WallRight", new Vector3( HW, H/2f,  0),    new Vector3(0.15f, H, D),   new Color(0.86f, 0.81f, 0.73f));

        // Окна (правая стена) — просто светло-голубые прямоугольники
        for (int i = 0; i < 3; i++)
            Make($"Window_{i}", new Vector3(HW - 0.07f, H * 0.55f, -3f + i * 3f),
                new Vector3(0.08f, H * 0.5f, 1.6f), new Color(0.55f, 0.72f, 0.95f));
    }

    // ── мебель и реквизит ────────────────────────────────────────
    void BuildFurniture()
    {
        // Доска на передней стене
        Anomalize(
            Make("Chalkboard",
                new Vector3(0, H * 0.55f, -HD + 0.12f),
                new Vector3(W * 0.55f, H * 0.45f, 0.1f),
                new Color(0.10f, 0.26f, 0.15f)));

        // Стол учителя
        Make("TeacherDesk",
            new Vector3(0, 0.38f, -HD + 1.5f),
            new Vector3(2.0f, 0.76f, 0.7f),
            new Color(0.48f, 0.33f, 0.16f));

        // Ученические парты — 3×3
        float[] cols = { -2.8f, 0f, 2.8f };
        float[] rows = { -1.5f, 0.5f, 2.5f };

        foreach (var z in rows)
        {
            foreach (var x in cols)
            {
                // Крышка парты
                Anomalize(
                    Make($"Desk_{x}_{z}",
                        new Vector3(x, 0.40f, z),
                        new Vector3(1.0f, 0.05f, 0.6f),
                        new Color(0.55f, 0.38f, 0.20f)));

                // Ножка
                Make($"Leg_{x}_{z}",
                    new Vector3(x, 0.20f, z),
                    new Vector3(0.06f, 0.40f, 0.06f),
                    new Color(0.40f, 0.28f, 0.14f));

                // Стул
                Make($"Chair_{x}_{z}",
                    new Vector3(x, 0.22f, z + 0.55f),
                    new Vector3(0.55f, 0.04f, 0.45f),
                    new Color(0.22f, 0.28f, 0.52f));
            }
        }

        // Часы на левой стене
        Anomalize(
            Make("Clock",
                new Vector3(-HW + 0.12f, H * 0.72f, -0.5f),
                new Vector3(0.1f, 0.38f, 0.38f),
                new Color(0.82f, 0.80f, 0.77f)));

        // Цветок в правом переднем углу
        Make("FlowerPot",
            new Vector3(HW - 0.55f, 0.28f, -HD + 0.55f),
            new Vector3(0.26f, 0.56f, 0.26f),
            new Color(0.62f, 0.32f, 0.18f));

        Anomalize(
            Make("Flower",
                new Vector3(HW - 0.55f, 0.72f, -HD + 0.55f),
                new Vector3(0.30f, 0.42f, 0.30f),
                new Color(0.18f, 0.60f, 0.18f)));

        // Полка с книгами у задней стены
        Make("Shelf",
            new Vector3(-3f, 1.2f, HD - 0.15f),
            new Vector3(2f, 0.1f, 0.3f),
            new Color(0.55f, 0.38f, 0.20f));

        for (int i = 0; i < 4; i++)
        {
            Anomalize(
                Make($"Book_{i}",
                    new Vector3(-3.7f + i * 0.5f, 1.38f, HD - 0.18f),
                    new Vector3(0.12f, 0.28f, 0.22f),
                    new Color(0.55f + i * 0.1f, 0.20f, 0.20f)));
        }
    }

    // ── позиция игрока ───────────────────────────────────────────
    void PlacePlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(0f, 1.1f, HD - 0.8f);
    }

    // ── утилиты ─────────────────────────────────────────────────
    // Создаёт куб с URP-материалом нужного цвета
    GameObject Make(string name, Vector3 pos, Vector3 scale, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(transform);
        go.transform.position   = pos;
        go.transform.localScale = scale;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        go.GetComponent<Renderer>().material = new Material(shader) { color = color };
        return go;
    }

    // Добавляет AnomalyObject — этот объект может стать аномалией
    static void Anomalize(GameObject go)
    {
        go.AddComponent<AnomalyObject>();
    }
}
