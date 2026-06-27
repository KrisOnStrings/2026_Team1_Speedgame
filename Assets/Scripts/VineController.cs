using UnityEngine;
using System.Collections.Generic;

public class VineController : MonoBehaviour
{
    public GameController gc;

    public GameObject GrapePrefab;
    public float VineGrowFrequency;

    private List<GameObject> grapes;
    private List<GameObject> vines;
    private List<int> availableVines;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        grapes = new List<GameObject>();
    }

    public void StartVines(List<GameObject> vineTiles)
    {
        vines = vineTiles;
        availableVines = new List<int>();
        for (int i = 0; i < vines.Count; i++) availableVines.Add(i);

        InvokeRepeating("VineGrow", 0.5f, VineGrowFrequency);
    }

    public void Cleanup()
    {
        foreach (GameObject grape in grapes)
        {
            Destroy(grape);
        }
        grapes.Clear();
    }

    public void VineGrow()
    {
        if (availableVines.Count > 0)
        {
            int rnd = Random.Range(0, availableVines.Count);
            int vineIndex = availableVines[rnd];

            GameObject grape = Instantiate(GrapePrefab, vines[vineIndex].transform.position, Quaternion.identity);
            grape.GetComponent<CollectController>().vc = this;
            grape.name = vineIndex.ToString();
            grape.transform.position += new Vector3(0, 0, -2); // Ensure Z is closer to the camera than other colliders so it is clickable
            grapes.Add(grape);

            availableVines.RemoveAt(rnd);
        }
    }

    public void HarvestGrape(CollectController grape)
    {
        gc.AddCurrency(grape.Amount);
        grapes.Remove(grape.gameObject);

        availableVines.Add(int.Parse(grape.name));
    }
}
