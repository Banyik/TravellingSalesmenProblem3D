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
        //??n. 'Keret' ciklus. Ennek seg??ts??g??vel az ??sszes csom??pontr??l indulhatunk
        for (int k = 0; k < cubeList.Count; k++)
        {
            float distance = 0.0f;
            currentDistance = 0.0f;
            //currentCube: a cubeList-b??l kiv??lasztott csom??pontr??l val?? indul??s
            currentCube = k;
            routedRoutes.Add(cubeList[currentCube]);
            //??n. 'Core' ciklus. Iter??l?? ciklus hogy a lista ??sszes elem??n v??gigfusson az algoritmus
            for (int j = 0; j < cubeList.Count; j++)
            {
                distance = 0.0f;
                float tempDistance = float.MaxValue;
                //??j ??rt??k, az oldCube az indul?? (de nem kezdeti) csom??pont, mivel a currentCube ??t lesz ??rva
                //a k??vetkez?? csom??pontra
                oldCube = currentCube;
                oldCubesInCalc.Add(oldCube);
                for (int i = 0; i < cubeList.Count; i++)
                {
                    //Legr??videbb csom?? megkeres??se ism??tl??d??s n??lk??l.
                    distance = Vector3.Distance(cubeList[oldCube].transform.position, cubeList[i].transform.position);
                    if(distance < tempDistance && distance != 0 && !routedRoutes.Contains(cubeList[i])){
                        currentCube = i;
                        tempDistance = distance;
                    }
                }
                //T??vols??g ??s csom??pont ??rt??k hozz??ad??s
                currentDistance += distance;
                if(!routedRoutes.Contains(cubeList[currentCube]))
                    routedRoutes.Add(cubeList[currentCube]);
                newCubesInCalc.Add(currentCube);
            }
            //Az utols?? elem ??sszek??t??se az els?? elemmel, ahhoz a t??vols??g lem??r??se ??s sz??m??t??si list??ba be??r??s
            distance = Vector3.Distance(cubeList[currentCube].transform.position, cubeList[0].transform.position);
            currentDistance += distance;
            oldCubesInCalc.Add(currentCube);
            newCubesInCalc.Add(k);
            //Debug.Log("Distance no. " + k + " : " + currentDistance);
            if(currentDistance<maxDistance){
                //Debug.Log("Smaller Distance: " + currentDistance);
                //??sszeadott t??vols??gok lem??r??se, ha nagyobb mint az el??z?? ??rt??k akkor ??t??r??sok
                //??s (majdnem) v??gleges list??k adatainak t??rl??se, ??t??r??sa a sz??m??t?? lista ??rt??keib??l
                maxDistance = currentDistance;
                oldCubes.Clear();
                newCubes.Clear();
                oldCubes.AddRange(oldCubesInCalc);
                newCubes.AddRange(newCubesInCalc);
            }
            //Sz??m??t??si list??k t??rl??se az ??jboli sz??m??t??shoz
            oldCubesInCalc.Clear();
            newCubesInCalc.Clear();
            routedRoutes.Clear();
        }
        //Debugol??sra val?? ki??r??sok/??rt??kad??sok
        currentDistance = maxDistance;
        watch.Stop();
        Debug.Log("Execution Time: " + watch.Elapsed + " (" + watch.ElapsedMilliseconds + " ms)");
        //Ismert lefut??si id??k:
        //4x4 csom??pont (16): 5 ms
        //15x15 csom??pont (255): 7000 ms (7s)
        //30x30 csom??pont (900): 826687 ms (13m 46s)
        //
        //Sufni Algoritmus sok szeretettel Banyik N??ndor-t??l
    }
}
