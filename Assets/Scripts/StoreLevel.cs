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
            Quaternion beforeRot = go.transform.rotation;

            // START Change pieces to allow correct calculations to send
            go.transform.rotation = Quaternion.identity;
            // END

            Renderer renderer = go.GetComponent<Renderer>();

            LevelPiece piece = new LevelPiece(go.transform.position, renderer.bounds.size, beforeRot);

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

            // Reset piece back to prestart
            go.transform.rotation = beforeRot;
        }

        // Was for testing to see duplicated data
        //StartCoroutine(spawnBack());
    }

    /*
     * Method was made for testing the data that was stored
     * in each individual level piece (gameObject)
     */
    IEnumerator spawnBack()
    {
        yield return new WaitForSeconds(1f);

        Vector3 offset = new Vector3(30, 0, 0);

        GameObject level = new GameObject("[LOADED LEVEL]");

        foreach (LevelPiece data in levelPieces)
        {
            ProBuilderMesh mesh = ShapeGenerator.GenerateCube(PivotLocation.Center, data.size);
            GameObject generatedObject = mesh.gameObject;

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
    public Vector3 size;
    public Quaternion rotation;

    public string materialName;

    public LevelPiece(Vector3 _pos, Vector3 _size, Quaternion _rot)
    {
        position = _pos;
        size = _size;
        rotation = _rot;
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