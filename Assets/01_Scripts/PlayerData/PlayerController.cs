using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;

    private float currentHP;

    // Start is called before the first frame update
    void Start()
    {
        currentHP = playerData.MaxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
