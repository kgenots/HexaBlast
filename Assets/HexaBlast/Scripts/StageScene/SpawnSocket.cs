using UnityEngine;

namespace HexaBlast
{
   class SpawnSocket : BlockSocket
   {
#if UNITY_EDITOR
      private void OnDrawGizmos()
      {
         Gizmos.color = Color.green;
         Gizmos.DrawSphere(transform.position, 0.2f);
      }
#endif
   }
}