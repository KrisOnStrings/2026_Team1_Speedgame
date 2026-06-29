using UnityEngine;
using System.Collections.Generic;

public class TowerMenuController : MonoBehaviour
{
    public GameController gc;
    public GameObject[] TowerLocations;
    public GameObject[] TowerPrefabs;

    private List<GameObject> towerList;

    private void Start()
    {
        Camera cam = Camera.main;

        float leftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        transform.position = new Vector3(leftEdge, transform.position.y, transform.position.z);
    }

    public void StartGame()
    {
        towerList = new List<GameObject>();
        for (int i = 0; i < TowerPrefabs.Length; i++)
        {
            GenerateTower(i);
        }
    }

    public void AddFirstTower()
    {
        towerList = new List<GameObject>();
        GenerateTower(0);
    }

    public void AddSecondTower()
    {
        GenerateTower(1);
    }

    public void GenerateTower(int index)
    {
        GameObject tower = Instantiate(TowerPrefabs[index], transform);
        tower.transform.position = TowerLocations[index].transform.position;
        tower.name = TowerPrefabs[index].name;
        tower.GetComponent<TowerController>().gc = gc;
        tower.GetComponent<TowerController>().tmc = this;
        tower.GetComponent<TowerController>().towerIndex = index;
        towerList.Add(tower);
    }

    public void StartPlacingTower()
    {
        foreach(GameObject tower in towerList)
        {
            tower.GetComponent<TowerController>().DisableCollider();
        }
    }

    public void DonePlacingTower()
    {
        foreach (GameObject tower in towerList)
        {
            tower.GetComponent<TowerController>().EnableCollider();
        }
    }

    public void TowerPlaced(int towerIndex)
    {
        GenerateTower(towerIndex);
    }

    public void CleanUp()
    {
        foreach (GameObject tower in towerList)
        {
            Destroy(tower);
        }
        towerList.Clear();
    }
}
