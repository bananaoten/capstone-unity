using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeTrigger : MonoBehaviour
{
   public AudioSource trigSource;
   public AudioClip sound;
   public bool played1x = false;

   void OnTriggerEnter()
   {
        if(!played1x)
         {
            trigSource.PlayOneShot(sound);
            played1x = true;
          }
    
   }
}
