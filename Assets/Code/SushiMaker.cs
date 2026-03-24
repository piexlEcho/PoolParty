using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SushiMaker : MonoBehaviour
{
    public RiceWraper sushi;
    public GameObject FPrefab;
    // Start is called before the first frame update
    void Start()
    {
        // 创建寿司
        sushi.BuildSushi(
            position: transform.position,
            shapeType: RiceWraper.ShapeType.Box,
            shapeSize: new Vector3(1.5f, 0.4f, 0.8f),
            fishPrefab: FPrefab,
            riceCountOverride: 160,
            fishHeightOffset: 0.15f
        );

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("w"))
        {
            sushi.Scatter(transform.position);
        }
    }
}
