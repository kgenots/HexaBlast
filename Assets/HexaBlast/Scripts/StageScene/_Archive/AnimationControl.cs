using UnityEngine;

namespace HexaBlast
{
   [RequireComponent(typeof(Animation))]
   class AnimationControl : MonoBehaviour
   {
      [SerializeField] Animation m_animation;

      private void Reset()
      {
         m_animation = gameObject.AddComponent<Animation>();
      }

      private void OnValidate()
      {
      }
   }

}