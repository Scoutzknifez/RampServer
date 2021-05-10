using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.ProBuilder;
using System;

public class StoreLevel : MonoBehaviour
{
    public static List<LevelPiece> levelPieces = new List<LevelPiece>();

    public Material fallbackLevelMaterial;
    public MaterialMapping[] levelMaterials;

    [Header("Testing")]
    public bool isTesting;
    public Vector3 offset;

    private void Start()
    {
        if (fallbackLevelMaterial == null)
        {
            Debug.LogError("Fallback Level Material CAN NOT BE NULL!");
            return;
        }

        foreach (Transform child in transform)
        {
            GameObject go = child.gameObject;
            ProBuilderMesh mainMesh = go.GetComponent<ProBuilderMesh>();

            ArrayPacker[] faces = new ArrayPacker[mainMesh.faces.Count];
            for (int i = 0; i < faces.Length; i++)
            {
                int[] faceVerts = new int[mainMesh.faces[i].indexes.Count];
                mainMesh.faces[i].indexes.CopyTo(faceVerts, 0);

                faces[i] = new ArrayPacker(faceVerts);
            }

            Vector3[] vertices = new Vector3[mainMesh.positions.Count];
            for (int i = 0; i < mainMesh.positions.Count; i++)
            {
                vertices[i] = mainMesh.positions[i];
            }

            ArrayPacker[] sharedVertices = new ArrayPacker[mainMesh.sharedVertices.Count];
            for (int i = 0; i < mainMesh.sharedVertices.Count; i++)
            {
                int[] sharedVertex = new int[mainMesh.sharedVertices[i].Count];
                mainMesh.sharedVertices[i].CopyTo(sharedVertex, 0);

                sharedVertices[i] = new ArrayPacker(sharedVertex);
            }

            LevelPiece piece = new LevelPiece(go.transform.position, go.transform.rotation, vertices, faces, sharedVertices);

            Renderer renderer = go.GetComponent<Renderer>();
            MaterialMapping mapping = MaterialMapping.GetMaterialMappingFromMaterial(levelMaterials, renderer.sharedMaterial);

            if (mapping != null)
            {
                piece.materialName = mapping.materialName;
            } 
            else
            {
                piece.materialName = Constants.ERROR_NO_MATERIAL;
            }

            levelPieces.Add(piece);
        }

        // Was for testing to see duplicated data
        if (isTesting)
        {
            StartCoroutine(spawnBack());
        }
    }

    /*
     * Method was made for testing the data that was stored
     * in each individual level piece (gameObject)
     */
    IEnumerator spawnBack()
    {
        yield return new WaitForSeconds(0.1f);        

        GameObject level = new GameObject("[LOADED LEVEL]");

        foreach (LevelPiece data in levelPieces)
        {
            List<Vertex> vertices = new List<Vertex>();
            foreach (Vector3 position in data.vertices)
            {
                Vertex newPoint = new Vertex();
                newPoint.position = position;
                vertices.Add(newPoint);
            }

            List<Face> faces = new List<Face>();
            foreach (ArrayPacker face in data.faces)
            {
                faces.Add(new Face(face.array));
            }

            List<SharedVertex> sharedVertices = new List<SharedVertex>();
            foreach (ArrayPacker shared in data.sharedVertices)
            {
                sharedVertices.Add(new SharedVertex(shared.array));
            }

            ProBuilderMesh pbMesh = ProBuilderMesh.Create(vertices, faces, sharedVertices);
            GameObject generatedObject = pbMesh.gameObject;

            generatedObject.transform.position = offset + data.position;
            generatedObject.transform.rotation = data.rotation;

            MaterialMapping mapping = MaterialMapping.GetMaterialMappingFromName(levelMaterials, data.materialName);

            if (mapping != null)
            {
                generatedObject.GetComponent<Renderer>().sharedMaterial = mapping.material;
            }
            else
            {
                generatedObject.GetComponent<Renderer>().sharedMaterial = fallbackLevelMaterial;
            }

            generatedObject.AddComponent<MeshCollider>();

            generatedObject.transform.parent = level.transform;
        }
    }
}

public class LevelPiece
{
    public Vector3 position;
    public Quaternion rotation;

    public Vector3[] vertices;
    public ArrayPacker[] faces;
    public ArrayPacker[] sharedVertices;

    public string materialName;

    public LevelPiece(Vector3 _pos, Quaternion _rot, Vector3[] _vertices, ArrayPacker[] _faces, ArrayPacker[] _sharedVertices)
    {
        position = _pos;
        rotation = _rot;
        vertices = _vertices;
        faces = _faces;
        sharedVertices = _sharedVertices;
    }
}

public class ArrayPacker
{
    public int length;
    public int[] array;

    public ArrayPacker(int[] _elements)
    {
        array = _elements;
        length = array.Length;
    }
}

[System.Serializable]
public class MaterialMapping
{
    public string materialName;
    public Material material;

    public static MaterialMapping GetMaterialMappingFromName(MaterialMapping[] map, string matName)
    {        
        return Array.Find(map, mapping => mapping.materialName.Equals(matName));
    }

    public static MaterialMapping GetMaterialMappingFromMaterial(MaterialMapping[] map, Material material)
    {
        return Array.Find(map, mapping => mapping.material.Equals(material));
    }
}