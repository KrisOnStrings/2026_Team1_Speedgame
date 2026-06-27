using UnityEngine;

public class Map : MonoBehaviour
{
    [TextArea(10, 10)] public string MapText;
    public int[] WavePoints;
    public int MinibossWave;
    public int BossWave;
}
