using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class SandSimulator : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] RenderTexture renderTexture;
    [SerializeField] int textureSize = 64;
    [SerializeField] int numGroups = 64;
    ComputeBuffer spawnSandBuffer;
    ComputeBuffer sandBuffer;

    private float timer;
    [SerializeField] float fps = 30.0f; // Desired frame rate interval (e.g., 30 FPS)

    // Start is called before the first frame update
    void Start()
    {
        renderTexture = new RenderTexture(textureSize, textureSize, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        shader.SetInt("Resolution", textureSize);
        shader.SetTexture(0, "Result", renderTexture);

        // Get the Renderer component of the child plane
        Renderer renderer = GetComponentInChildren<Renderer>();

        // Check if the Renderer component exists
        if (renderer != null)
        {
            // Set the texture of the material on the Renderer
            renderer.material.mainTexture = renderTexture;
        }
        else
        {
            Debug.LogError("Renderer component not found on the child plane object.");
        }

        CreateBuffers();

    }

    void CreateBuffers()
    {
        sandBuffer = new ComputeBuffer(textureSize * textureSize, sizeof(int));

        // create array of default values
        int[] initData = new int[sandBuffer.count];

        // Fill the array with zeros
        for (int i = 0; i < initData.Length; i++)
        {
            initData[i] = 0;
        }

        // Fill the array with zeros
        for (int i = 0; i < initData.Length*0.7; i++)
        {
            initData[i] = 1;
        }

        sandBuffer.SetData(initData);
        shader.SetBuffer(0, "sandBuffer", sandBuffer);
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer <= fps/60.0f)
        {
            return;
        }
        timer = 0f;

        if (Input.GetMouseButton(0))
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
                        //Debug.Log("Clicked on quad at: " + localHitPoint);

                        // Map the coordinates to the range [0, m_paddedImageSize]
                        mappedX = (int)((localHitPoint.x * 0.5f + 0.5f) * textureSize);
                        mappedY = (int)((localHitPoint.y * 0.5f + 0.5f) * textureSize);


                        // convert to fit the transposed images or smth
                        mappedY = textureSize - mappedY;

                        // Now 'mappedX' and 'mappedY' contain the position on the quad in the range [0, m_paddedImageSize]
                        Debug.Log($" {textureSize} Clicked on quad at: (" + mappedX + ", " + mappedY + ")");
                    }
                }
            }

            int deleteRange = 3;

            int[] spawnSandArray = new int[textureSize * textureSize];

            for (int y = -deleteRange; y < deleteRange; y++)
            {
                for (int x = -deleteRange; x < deleteRange; x++)
                {
                    int yval = textureSize * ((int)mappedY + y);
                    int xval = (int)mappedX + x;
                    spawnSandArray[yval + xval] = 1;
                }
            }

            spawnSandBuffer = new ComputeBuffer(spawnSandArray.Length, sizeof(int));
            spawnSandBuffer.SetData(spawnSandArray);

            shader.SetBuffer(0, "spawnSand", spawnSandBuffer);
        }

        shader.Dispatch(0, numGroups, numGroups, 1);

        if(spawnSandBuffer != null)
        {
            spawnSandBuffer.Release();
        }
    }

    void OnDestroy()
    {
        if (spawnSandBuffer != null)
        {
            spawnSandBuffer.Release();
            sandBuffer.Release();
        }
    }
}
