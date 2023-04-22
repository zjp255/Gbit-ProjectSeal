using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SightField : MonoBehaviour
{
    private float radius;//半径
    private float angleDegree;//扇形度数
    public int partitionCount;//分割数

    private MeshFilter meshFilter;
    private Mesh mesh;
    public float y;

    private Ray[] rays;//射线
    private float[] rs;//每条射线的长度
    private int isNeedUpdateMesh = 0;
    private int isUpdatedMesh = 0;
    private Human parent;
    // Start is called before the first frame update
    void Start()
    {
        //Human human = transform.parent.GetComponent<Human>();
        //if (human.isShowSight == true)
        //{
        //    radius = human.sightR;
        //    angleDegree = human.sightAngle;
        //    meshFilter = GetComponent<MeshFilter>();
        //    mesh = new Mesh();
        //    meshFilter.mesh = CreateMesh();
        //    createRay();
        //    rs = new float[partitionCount + 1];
        //    parent = transform.parent.GetComponent<Human>();
        //}
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //createRay();
        //myRayCast();
        //if (isNeedUpdateMesh != 0)
        //{
        //    meshFilter.mesh = updateMesh();
        //}
        //if (isNeedUpdateMesh == 0 && isUpdatedMesh == partitionCount + 1)
        //{
        //    meshFilter.mesh = CreateMesh();
        //    parent.setPlayerIsInRange(false);

        //}
    }
    /// <summary>
    /// 创建初始的sight field
    /// </summary>
    /// <returns></returns>
    Mesh CreateMesh()
    {
        //顶点
        int vertices_count = partitionCount + 2;
        Vector3[] vertices = new Vector3[vertices_count];
        float angleRad = Mathf.Deg2Rad * angleDegree;
        float angleStart = Mathf.PI * ((180 - angleDegree) / 2 / 180);
        float angleDelta = angleRad / partitionCount;
        vertices[0] = new Vector3(0, y, 0);
        for (int i = 1; i < vertices_count; i++)
        {
            float cosA = Mathf.Cos(angleStart);
            float sinA = Mathf.Sin(angleStart);
            vertices[i] = new Vector3(radius * cosA, y, radius * sinA);
            angleStart += angleDelta;
        }

        //triangles
        int triangle_count = partitionCount * 3;
        int[] triangles = new int[triangle_count];
        for (int i = 0, vi = 1; i < triangle_count; i += 3, vi++)
        {
            triangles[i] = 0;
            triangles[i + 1] = vi + 1;
            triangles[i + 2] = vi;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }
    /// <summary>
    /// 更新sight Field
    /// </summary>
    /// <returns></returns>
    Mesh updateMesh()
    {
      
        
        //顶点
        int vertices_count = partitionCount + 2;
        Vector3[] vertices = new Vector3[vertices_count];
        float angleRad = Mathf.Deg2Rad * angleDegree;
        float angleStart = Mathf.PI * ((180 - angleDegree) / 2 / 180);
        float angleDelta = angleRad / partitionCount;
        vertices[0] = new Vector3(0, y, 0);
        for (int i = 1; i < vertices_count; i++)
        {
            float cosA = Mathf.Cos(angleStart);
            float sinA = Mathf.Sin(angleStart);
            vertices[i] = new Vector3(rs[i - 1] * cosA, y, rs[i - 1] * sinA);
            angleStart += angleDelta;
        }

        //triangles
        int triangle_count = partitionCount * 3;
        int[] triangles = new int[triangle_count];
        for (int i = 0, vi = 1; i < triangle_count; i += 3, vi++)
        {
            triangles[i] = 0;
            triangles[i + 1] = vi + 1;
            triangles[i + 2] = vi;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        return mesh;
    }
    /// <summary>
    /// 创造射线
    /// </summary>
    void createRay()
    {
        int ray_count = partitionCount + 1;
        rays = new Ray[ray_count];
        float angleRad = Mathf.Deg2Rad * angleDegree;
        float angleStart = Mathf.PI * ((180 - angleDegree) / 2 / 180);
        float angleDelta = angleRad / partitionCount;
        for (int i = 0; i < ray_count; i++)
        {
            float cosA = Mathf.Cos(angleStart);
            float sinA = Mathf.Sin(angleStart);
            rays[i] = new Ray(transform.parent.position, transform.TransformDirection(new Vector3(cosA, 0, sinA)));      
            angleStart += angleDelta;
        }
    }

    /// <summary>
    /// 进行射线检测
    /// </summary>
    void myRayCast()
    {
        isNeedUpdateMesh = 0;
        isUpdatedMesh = 0;
        for (int i = 0; i < rays.Length; i++)
        {
            
            RaycastHit hit;
            if (Physics.Raycast(rays[i], out hit, radius, 1 << 0))
            {
                if (rs[i] != hit.distance)
                {
                    isNeedUpdateMesh++;
                    rs[i] = hit.distance;
                }
                if (hit.transform.tag == "Player")
                {
                    //parent.setPlayerIsInRange(true);
                }
            }
            else
            {
                rs[i] = radius;
                isUpdatedMesh++;
            }
            //
            //Debug.DrawRay(rays[i].origin,rays[i].direction * radius, Color.red, Time.deltaTime);
        }
    }
}

