using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Proyectile proyectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;

    float nextShotTime;

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShots / 1000;
            Proyectile newProyectile = Instantiate(proyectile, muzzle.position, muzzle.rotation) as Proyectile;
            newProyectile.SetSpeed(muzzleVelocity);
        }
        
    }
}
