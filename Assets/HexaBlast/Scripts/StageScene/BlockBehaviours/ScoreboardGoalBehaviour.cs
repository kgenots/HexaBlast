﻿using System.Threading.Tasks;
using UnityEngine;

namespace HexaBlast
{
   class ScoreboardGoalBehaviour : MonoBehaviour, IHPResponse
   {
      [SerializeField] GoalEntity m_goalEntity;
      [SerializeField] Sprite m_flying;
      [SerializeField] float m_aniDuration = 0.5f;
      [SerializeField] string m_curveName = "GoalFly";
      [SerializeField] string m_spriteOrderName = "GoalFly";

      public void OnHpChanged(int value)
      {
         if (value <= 0 && Stage.Instance.ContainGoalEntity(m_goalEntity))
         {
            StartGoalAnimationAsync().Watch();
         }
      }

      public async Task StartGoalAnimationAsync()
      {
         Stage.Instance.AddRemainGoalDelta(-1, false, m_goalEntity);

         GameObject flying = new GameObject();
         var render = flying.AddComponent<SpriteRenderer>();
         render.sprite = !m_flying ? GetComponentInChildren<SpriteRenderer>().sprite : m_flying;
         render.sortingOrder = SpriteOrderManager.Instance.GetOrder(m_spriteOrderName);
         flying.transform.position = transform.position;
         render.color = GetComponentInChildren<SpriteRenderer>().color;

         var dst = Stage.Instance.GoalImageTransform.position;
         var curve = AnimationCurveManager.Instance.GetCurve(m_curveName);
         var sm = new Simation(transform.position, dst, flying.transform, curve, m_aniDuration);
         await sm.StartAsync();

         Destroy(flying);
         Stage.Instance.UpdateGoalUIs();
      }
   }
}