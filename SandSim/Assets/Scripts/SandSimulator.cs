using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SandSimulator : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] RenderTexture renderTexture2;
    [SerializeField] int textureSize = 64;
    [SerializeField] int numGroups = 64;
    ComputeBuffer spawnSandBuffer;

    private float timer;
    [SerializeField] float fps = 30.0f; // Desired frame rate interval (e.g., 30 FPS)

    // Get the Renderer component of the child plane
    Renderer renderer;
    private int frameId = 0;

    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(textureSize, textureSize, 24);

        // Disable anti-aliasing
        renderTexture.antiAliasing = 1; // Set to 1x (no anti-aliasing)

        // Disable mipmapping
        renderTexture.useMipMap = false;
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Point;
        renderTexture.Create();
        shader.SetTexture(0, "Result", renderTexture);

        shader.SetInt("Resolution", textureSize);

        renderer = GetComponent<Renderer>();

        // Set the texture of the material on the Renderer
        renderer.material.mainTexture = renderTexture;

        CreateBuffers();

        shader.Dispatch(0, numGroups, numGroups, 1);

        ClearSpawnBuffer();
    }

    void ClearSpawnBuffer()
    {
        // create array of default values
        float[] initData = new float[spawnSandBuffer.count];

        // Fill the array with zeros
        for (int i = 0; i < initData.Length; i++)
        {
            initData[i] = 0;
        }

        spawnSandBuffer.SetData(initData);
    }

    void CreateBuffers()
    {
        spawnSandBuffer = new ComputeBuffer(textureSize * textureSize, sizeof(float));

        // create array of default values
        float[] initData = new float[spawnSandBuffer.count];

        // Fill the array with zeros
        for (int i = 0; i < initData.Length; i++)
        {
            initData[i] = 0.0f;
        }

        // Fill the array with zeros
        for (int i = 0; i < initData.Length*0.5; i++)
        {
            initData[i] = 1.0f;
        }

        spawnSandBuffer.SetData(initData);
        shader.SetBuffer(0, "spawnSandBuffer", spawnSandBuffer);
    }

    void SpawnPixelType(float pixelType)
    {
        // Cast a ray from the camera through the mouse position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Declare a variable to store the hit information
        RaycastHit hit;

        int mappedX = 0, mappedY = 0;
        // Check if the ray intersects with any collider
        if (Physics.Raycast(ray, out hit))
        {
            // Check if the hit collider belongs to a mesh
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                // Get the point where the ray intersects with the mesh
                UnityEngine.Vector3 hitPoint = hit.point;

                Transform quadTransform = meshCollider.transform;
                // Convert hitPoint to the local space of the quad
                if (quadTransform != null)
                {
                    UnityEngine.Vector3 localHitPoint = quadTransform.InverseTransformPoint(hitPoint);

                    // Now 'localHitPoint' contains the position on the quad where the player clicked
                    Debug.Log("Clicked on quad at: " + localHitPoint);

                    // Map the coordinates to the range [0, m_paddedImageSize]
                    mappedX = (int)((-localHitPoint.x * 0.5f) * textureSize / 5);
                    mappedY = (int)((-localHitPoint.z * 0.5f) * textureSize / 5);

                    // convert to fit the transposed images or smth
                    mappedX = mappedX + (int)(textureSize * 0.5f);
                    mappedY = mappedY + (int)(textureSize * 0.5f);

                    // Now 'mappedX' and 'mappedY' contain the position on the quad in the range [0, m_paddedImageSize]
                    Debug.Log($" {textureSize} Clicked on quad at: (" + mappedX + ", " + mappedY + ")");

                    int deleteRange = 3;

                    float[] spawnSandArray = new float[textureSize * textureSize];

                    for (int y = -deleteRange; y < deleteRange; y++)
                    {
                        for (int x = -deleteRange; x < deleteRange; x++)
                        {
                            int yval = textureSize * ((int)mappedY + y);
                            int xval = (int)mappedX + x;
                            if (yval + xval > textureSize * textureSize || yval + xval < 0)
                            {
                                continue;
                            }
                            spawnSandArray[yval + xval] = pixelType;
                        }
                    }

                    spawnSandBuffer = new ComputeBuffer(spawnSandArray.Length, sizeof(float));
                    spawnSandBuffer.SetData(spawnSandArray);

                    shader.SetBuffer(0, "spawnSandBuffer", spawnSandBuffer);
                }
            }
        }

    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer <= 1.0f/fps)
        {
            return;
        }
        timer = 0f;

        // left click - sand
        if (Input.GetMouseButton(0))
        {
            SpawnPixelType(1f);
        }

        // right click - delete
        if (Input.GetMouseButton(1))
        {
            SpawnPixelType(-1f);
        }

        // scroll click - stone
        if (Input.GetMouseButton(2))
        {
            SpawnPixelType(0.9f);
        }

        // sidebutton click - water
        if (Input.GetMouseButton(3))
        {
            SpawnPixelType(0.8f);
        }

        shader.Dispatch(0, numGroups, numGroups, 1);

        frameId++;
        shader.SetInt("frameId", frameId % 999);

        if (spawnSandBuffer != null)
        {
            spawnSandBuffer.Release();
        }
    }

    void OnDestroy()
    {
        if (spawnSandBuffer != null)
        {
            spawnSandBuffer.Release();
        }
    }
}
