using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class CubeWallGenerator : MonoBehaviour
{
    public GameObject cubePlatformPrefab;
    public int numRows = 10;
    public int numColumns = 10;

    private GameObject cubePlatform;
	public void Start ()
	{
	    var firstCubePosition = new Vector3(-(numRows / 2) + 0.5f, numColumns / 2 - 0.5f, 0f);

	    for (var i = 0; i < numRows; i++)
	    {
	        for (var j = 0; j < numColumns; j++)
	        {
	            cubePlatform = Instantiate(cubePlatformPrefab, transform);
	            cubePlatform.transform.localPosition = firstCubePosition + new Vector3(i, -j, 0);
	        }
	    }
	}
}
