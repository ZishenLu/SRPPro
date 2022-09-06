using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSM
{
    public float[] plits = {0.07f,0.13f,0.25f,0.55f};
    Vector3[] farPlanes = new Vector3[4];
    Vector3[] nearPlanes = new Vector3[4];
    Vector3[] f0_near = new Vector3[4], f0_far = new Vector3[4];
    Vector3[] f1_near = new Vector3[4], f1_far = new Vector3[4];
    Vector3[] f2_near = new Vector3[4], f2_far = new Vector3[4];
    Vector3[] f3_near = new Vector3[4], f3_far = new Vector3[4];
    Vector3[] box0,box1,box2,box3;

    struct MainCameraSettings
    {
        public Vector3 position;
        public Quaternion rotation;
        public float nearClipPlane;
        public float farClipPlane;
        public float aspect;
    };
    MainCameraSettings settings;


    // 保存相机参数, 更改为正交投影
    public void SaveMainCameraSettings(ref Camera camera)
    {
        settings.position = camera.transform.position;
        settings.rotation = camera.transform.rotation;
        settings.farClipPlane = camera.farClipPlane;
        settings.nearClipPlane = camera.nearClipPlane;
        settings.aspect = camera.aspect;
        camera.orthographic = true;
    }

    // 还原相机参数, 更改为透视投影
    public void RevertMainCameraSettings(ref Camera camera)
    {
        camera.transform.position = settings.position;
        camera.transform.rotation = settings.rotation;
        camera.farClipPlane = settings.farClipPlane;
        camera.nearClipPlane = settings.nearClipPlane;
        camera.aspect = settings.aspect;
        camera.orthographic = false;
    }
    Vector3 TransformMat(Matrix4x4 m,Vector3 v,float w = 1.0f)
    {
        Vector4 v4 = new Vector4(v.x,v.y,v.z,w);
        v4 = m * v4;
        return new Vector3(v4.x,v4.y,v4.z);
    }
    Vector3[] LightAABBWS(Vector3[] nearPlanes,Vector3[] farPlanes,Vector3 lightDir)
    {
        Matrix4x4 toShadowViewInv = Matrix4x4.LookAt(Vector3.zero,lightDir,Vector3.up);
        Matrix4x4 toShadowView = toShadowViewInv.inverse;
        for (int i = 0; i < 4; i++)
        {
            nearPlanes[i] = TransformMat(toShadowView,nearPlanes[i]);
            farPlanes[i] = TransformMat(toShadowView,farPlanes[i]);
        }
        float[] x = new float[8];
        float[] y = new float[8];
        float[] z = new float[8];
        for (int i = 0; i < 4; i++)
        {
            x[i] = nearPlanes[i].x; x[i+4] = farPlanes[i].x;
            y[i] = nearPlanes[i].y; y[i+4] = farPlanes[i].y;
            z[i] = nearPlanes[i].z; z[i+4] = farPlanes[i].z;
        }
        float xmin = Mathf.Min(x); float xmax = Mathf.Max(x);
        float ymin = Mathf.Min(y); float ymax = Mathf.Max(y);
        float zmin = Mathf.Min(z); float zmax = Mathf.Max(z);
        Vector3[] points = {
            new Vector3(xmin, ymin, zmin), new Vector3(xmin, ymin, zmax), new Vector3(xmin, ymax, zmin), new Vector3(xmin, ymax, zmax),
            new Vector3(xmax, ymin, zmin), new Vector3(xmax, ymin, zmax), new Vector3(xmax, ymax, zmin), new Vector3(xmax, ymax, zmax)
        };
        for (int i = 0; i < 8; i++)
        {
            points[i] = TransformMat(toShadowViewInv,points[i]);
        }
        for (int i = 0; i < 4; i++)
        {
            farPlanes[i] = TransformMat(toShadowViewInv,farPlanes[i]);
            nearPlanes[i] = TransformMat(toShadowViewInv,nearPlanes[i]);
        }
        return points;
    }
    public void Update(Camera mainCam,Vector3 lightDir) 
    {
        mainCam.CalculateFrustumCorners(new Rect(0,0,1,1),mainCam.farClipPlane,Camera.MonoOrStereoscopicEye.Mono,farPlanes);
        mainCam.CalculateFrustumCorners(new Rect(0,0,1,1),mainCam.nearClipPlane,Camera.MonoOrStereoscopicEye.Mono,nearPlanes);
        
        for (int i = 0; i < 4; i++)
        {
            farPlanes[i] = mainCam.transform.TransformVector(farPlanes[i]) + mainCam.transform.position;
            nearPlanes[i] = mainCam.transform.TransformVector(nearPlanes[i]) + mainCam.transform.position;
        }

        for (int i = 0; i < 4; i++)
        {
            Vector3 dir = farPlanes[i] - nearPlanes[i];
            f0_near[i] = nearPlanes[i]; f0_far[i] = f0_near[i] + dir * plits[0];
            f1_near[i] = f0_far[i]; f1_far[i] = f1_near[i] + dir * plits[1]; 
            f2_near[i] = f1_far[i]; f2_far[i] = f2_near[i] + dir * plits[2];
            f3_near[i] = f2_far[i]; f3_far[i] = f3_near[i] + dir * plits[3];
        }
        box0 = LightAABBWS(f0_near,f0_far,lightDir);
        box1 = LightAABBWS(f1_near,f1_far,lightDir);
        box2 = LightAABBWS(f2_near,f2_far,lightDir);
        box3 = LightAABBWS(f3_near,f3_far,lightDir);

    }
        // 将相机配置为第 level 级阴影贴图的绘制模式
    public void ConfigCameraToShadowSpace(ref Camera camera, Vector3 lightDir, int level, float distance)
    {
        // 选择第 level 级视锥划分
        var box = new Vector3[8];
        if(level==0) box=box0; if(level==1) box=box1; 
        if(level==2) box=box2; if(level==3) box=box3;

        // 计算 Box 中点, 宽高比
        Vector3 center = (box[3] + box[4]) / 2; 
        float w = Vector3.Magnitude(box[0] - box[4]);
        float h = Vector3.Magnitude(box[0] - box[2]);

        // 配置相机
        camera.transform.rotation = Quaternion.LookRotation(lightDir);
        camera.transform.position = center; 
        camera.nearClipPlane = -distance;
        camera.farClipPlane = distance;
        camera.aspect = w / h;
        camera.orthographicSize = h * 0.5f;
    }
    void DrawFrustum(Vector3[] nearCorners, Vector3[] farCorners, Color color)
    {
        for (int i = 0; i < 4; i++)
            Debug.DrawLine(nearCorners[i], farCorners[i], color);

        Debug.DrawLine(farCorners[0], farCorners[1], color);
        Debug.DrawLine(farCorners[0], farCorners[3], color);
        Debug.DrawLine(farCorners[2], farCorners[1], color);
        Debug.DrawLine(farCorners[2], farCorners[3], color);
        Debug.DrawLine(nearCorners[0], nearCorners[1], color);
        Debug.DrawLine(nearCorners[0], nearCorners[3], color);
        Debug.DrawLine(nearCorners[2], nearCorners[1], color);
        Debug.DrawLine(nearCorners[2], nearCorners[3], color);
    }

    // 画光源方向的 AABB 包围盒
    void DrawAABB(Vector3[] points, Color color)
    {
        // 画线
        Debug.DrawLine(points[0], points[1], color);
        Debug.DrawLine(points[0], points[2], color);
        Debug.DrawLine(points[0], points[4], color);
        
        Debug.DrawLine(points[6], points[2], color);
        Debug.DrawLine(points[6], points[7], color);
        Debug.DrawLine(points[6], points[4], color);

        Debug.DrawLine(points[5], points[1], color);
        Debug.DrawLine(points[5], points[7], color);
        Debug.DrawLine(points[5], points[4], color);

        Debug.DrawLine(points[3], points[1], color);
        Debug.DrawLine(points[3], points[2], color);
        Debug.DrawLine(points[3], points[7], color);
    }

    public void DebugDraw()
    {
        DrawFrustum(nearPlanes, farPlanes, Color.white);
        DrawAABB(box0, Color.yellow);  
        DrawAABB(box1, Color.magenta);
        DrawAABB(box2, Color.green);
        DrawAABB(box3, Color.cyan);
        //DrawFrustum(f0_near, f0_far, Color.white);
        //DrawFrustum(f1_near, f1_far, Color.white);
        //DrawFrustum(f2_near, f2_far, Color.white);
        //DrawFrustum(f3_near, f3_far, Color.white);
    }
}
