using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{

    public Image weaponImageHolder;
    public Text ammoStats;


    Player player;
    Gun gunEquippedByPlayer;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();

    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            gunEquippedByPlayer = player.GetComponent<GunController>().equippedGun;
        }

        if (gunEquippedByPlayer != null)
        {
            ammoStats.text = (gunEquippedByPlayer.projectilesRemainingInMag / gunEquippedByPlayer.projectilesPerShot) + " / " + gunEquippedByPlayer.projectilesPerMag / gunEquippedByPlayer.projectilesPerShot;
            weaponImageHolder.sprite = gunEquippedByPlayer.gunImage;
        }
    }
}
