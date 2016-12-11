using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class TileGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int numRows = 10;
    public int numColumns = 10;

    private GameObject tile;

    public Tile[] GenerateTiles()
	{
	    var tiles = new Tile[numColumns * numRows];

	    var firstCubePosition = new Vector3(-(numRows / 2) + 0.5f, numColumns / 2 - 0.5f, 0f);

	    for (var i = 0; i < numRows; i++)
	    {
	        for (var j = 0; j < numColumns; j++)
	        {
	            tile = Instantiate(tilePrefab, transform);
	            tile.name = "Tile (" + i + "," + j + ")";
	            tile.transform.GetChild(0).name = "Collider (" + i + "," + j + ")";
	            tile.transform.localPosition = firstCubePosition + new Vector3(j, -i, 0);
	            tiles[i * numColumns + j] = tile.GetComponent<Tile>();
	        }
	    }

	    return tiles;
	}
}
