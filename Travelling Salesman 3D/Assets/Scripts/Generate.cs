using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generate : MonoBehaviour
{
    public GameObject Cube;
    public int X = 10;
    public int Y = 10;
    public float xOffset = 100;
    public float yOffset = 100;
    public float MapMultiply = 2;
    float maxOffset = 100000.0f;
    public Material lineMat;
    public List<GameObject> cubeList = new List<GameObject>();
    public List<GameObject> routedRoutes = new List<GameObject>();
    public List<int> oldCubes = new List<int>();
    public List<int> newCubes = new List<int>();
    public List<int> newCubesInCalc = new List<int>();
    public List<int> oldCubesInCalc = new List<int>();
    public int currentCube = 0;
    public int oldCube = 0;
    public float currentDistance = 0.0f;
    void Start()
    {
        xOffset = UnityEngine.Random.Range(0.0f, maxOffset);
        yOffset = UnityEngine.Random.Range(0.0f, maxOffset);
        for (int i = 1; i <= Y; i++)
        {
            for (int j = 1; j <= X; j++)
            {
                SpawnRoute(j * 2, i*2);
            }
        }
        Destroy(Cube);
        FindPath();
    }

    void DrawConnectingLines(){
        for (int i = 0; i < newCubes.Count; i++)
        {
            GL.Begin(GL.LINES);
            lineMat.SetPass(0);
            GL.Color(new Color(lineMat.color.r, lineMat.color.g, lineMat.color.b, lineMat.color.a));   
            GL.Vertex3(cubeList[oldCubes[i]].transform.position.x, cubeList[oldCubes[i]].transform.position.y, cubeList[oldCubes[i]].transform.position.z);
            GL.Vertex3(cubeList[newCubes[i]].transform.position.x, cubeList[newCubes[i]].transform.position.y, cubeList[newCubes[i]].transform.position.z);
            GL.End();
        }
        
    }

    void OnPostRender()
    {
        DrawConnectingLines();
    }
    void OnDrawGizmos() {
        DrawConnectingLines();
    }

    void SpawnRoute(float x, float z){
        float xc = (x + xOffset) / x * MapMultiply;
        float zc = (z + yOffset) / z * MapMultiply;
        float y = Mathf.PerlinNoise(xc,zc);
        var clone = Instantiate(Cube, new Vector3(x + UnityEngine.Random.Range(1,5),y + UnityEngine.Random.Range(1,10), z + UnityEngine.Random.Range(1,5)*3), Quaternion.Euler(0,0,0));
        cubeList.Add(clone);
    }

    void FindPath (){
        var watch = new System.Diagnostics.Stopwatch();
        watch.Start();
        float maxDistance = float.MaxValue;   
        //Ún. 'Keret' ciklus. Ennek segítségével az összes csomópontról indulhatunk
        for (int k = 0; k < cubeList.Count; k++)
        {
            float distance = 0.0f;
            currentDistance = 0.0f;
            //currentCube: a cubeList-ből kiválasztott csomópontról való indulás
            currentCube = k;
            routedRoutes.Add(cubeList[currentCube]);
            //Ún. 'Core' ciklus. Iteráló ciklus hogy a lista összes elemén végigfusson az algoritmus
            for (int j = 0; j < cubeList.Count; j++)
            {
                distance = 0.0f;
                float tempDistance = float.MaxValue;
                //új érték, az oldCube az induló (de nem kezdeti) csomópont, mivel a currentCube át lesz írva
                //a következő csomópontra
                oldCube = currentCube;
                oldCubesInCalc.Add(oldCube);
                for (int i = 0; i < cubeList.Count; i++)
                {
                    //Legrövidebb csomó megkeresése ismétlődés nélkül.
                    distance = Vector3.Distance(cubeList[oldCube].transform.position, cubeList[i].transform.position);
                    if(distance < tempDistance && distance != 0 && !routedRoutes.Contains(cubeList[i])){
                        currentCube = i;
                        tempDistance = distance;
                    }
                }
                //Távolság és csomópont érték hozzáadás
                currentDistance += distance;
                if(!routedRoutes.Contains(cubeList[currentCube]))
                    routedRoutes.Add(cubeList[currentCube]);
                newCubesInCalc.Add(currentCube);
            }
            //Az utolsó elem összekötése az első elemmel, ahhoz a távolság lemérése és számítási listába beírás
            distance = Vector3.Distance(cubeList[currentCube].transform.position, cubeList[0].transform.position);
            currentDistance += distance;
            oldCubesInCalc.Add(currentCube);
            newCubesInCalc.Add(k);
            //Debug.Log("Distance no. " + k + " : " + currentDistance);
            if(currentDistance<maxDistance){
                //Debug.Log("Smaller Distance: " + currentDistance);
                //Összeadott távolságok lemérése, ha nagyobb mint az előző érték akkor átírások
                //És (majdnem) végleges listák adatainak törlése, átírása a számító lista értékeiből
                maxDistance = currentDistance;
                oldCubes.Clear();
                newCubes.Clear();
                oldCubes.AddRange(oldCubesInCalc);
                newCubes.AddRange(newCubesInCalc);
            }
            //Számítási listák törlése az újboli számításhoz
            oldCubesInCalc.Clear();
            newCubesInCalc.Clear();
            routedRoutes.Clear();
        }
        //Debugolásra való kiírások/értékadások
        currentDistance = maxDistance;
        watch.Stop();
        Debug.Log("Execution Time: " + watch.Elapsed + " (" + watch.ElapsedMilliseconds + " ms)");
        //Ismert lefutási idők:
        //4x4 csomópont (16): 5 ms
        //15x15 csomópont (255): 7000 ms (7s)
        //30x30 csomópont (900): 826687 ms (13m 46s)
        //
        //Sufni Algoritmus sok szeretettel Banyik Nándor-tól
    }
}
