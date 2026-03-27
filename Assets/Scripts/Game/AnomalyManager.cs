using UnityEngine;

public class AnomalyManager : MonoBehaviour
{
    private AnomalyObject chosenAnomaly;

    public void SpawnAnomaly()
    {
        var all = FindObjectsOfType<AnomalyObject>();

        if (all.Length == 0)
        {
            Debug.LogWarning("[AnomalyManager] Нет AnomalyObject в сцене!");
            return;
        }

        chosenAnomaly = all[Random.Range(0, all.Length)];
        chosenAnomaly.IsAnomalous = true;

        ApplyVisual(chosenAnomaly);
        Debug.Log($"[AnomalyManager] Аномалия: {chosenAnomaly.name}");
    }

    public bool IsAnomaly(AnomalyObject obj) => obj != null && obj.IsAnomalous;

    // ── визуальные изменения ─────────────────────────────────────
    void ApplyVisual(AnomalyObject obj)
    {
        switch (Random.Range(0, 3))
        {
            case 0: ApplyWrongColor(obj); break;
            case 1: ApplyFloating(obj);   break;
            case 2: ApplyExtraObject(obj); break;
        }
    }

    // Объект меняет цвет на ярко-красный
    void ApplyWrongColor(AnomalyObject obj)
    {
        var rend = obj.GetComponent<Renderer>();
        if (rend == null) { ApplyFloating(obj); return; }

        // Новый экземпляр материала — не затронем соседей с тем же material
        rend.material = new Material(rend.material) { color = new Color(1f, 0.08f, 0.08f) };
    }

    // Объект зависает в воздухе
    void ApplyFloating(AnomalyObject obj)
    {
        obj.transform.position += Vector3.up * 0.45f;
    }

    // Рядом с объектом появляется светящийся куб — сам куб и есть аномалия
    void ApplyExtraObject(AnomalyObject obj)
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "AnomalyExtra";
        cube.transform.position   = obj.transform.position + new Vector3(0.3f, 0.8f, 0f);
        cube.transform.localScale = Vector3.one * 0.22f;

        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var mat = new Material(shader);
        mat.color = new Color(1f, 0.55f, 0f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.4f, 0f) * 2.5f);
        cube.GetComponent<Renderer>().material = mat;

        // Аномалией является именно новый куб
        obj.IsAnomalous = false;
        chosenAnomaly   = cube.AddComponent<AnomalyObject>();
        chosenAnomaly.IsAnomalous = true;
    }
}
