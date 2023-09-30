﻿using System.Collections.Generic;
using UnityEngine;

namespace HexaBlast
{
   [ExecuteAlways]
   class ColorManager : UnityCommon.MonoBehaviourSingleton<ColorManager>
   {
      [SerializeField] Token[] m_colors;
      [SerializeField] TokenStr[] m_colorStrs;

      Dictionary<ColorLayer, Color> m_dic;
      Dictionary<string, Color> m_dicStr;

      private void OnEnable()
      {
         m_dic = new Dictionary<ColorLayer, Color>();
         foreach (Token t in m_colors)
         {
            m_dic.Add(t.Layer, t.Color);
         }

         m_dicStr = new Dictionary<string, Color>();
         foreach (TokenStr t in m_colorStrs)
         {
            m_dicStr.Add(t.Name, t.Color);
         }
      }

      private void OnDisable()
      {

      }

      public Color GetColorByString(string name)
      {
         return m_dicStr[name];
      }

      public Color GetColor(ColorLayer layer)
      {
         if (layer == ColorLayer.None)
         {
            return Color.white;
         }

         return m_dic[layer];
      }

      [System.Serializable]
      struct Token
      {
         public ColorLayer Layer;
         public Color Color;
      }

      [System.Serializable]
      struct TokenStr
      {
         public string Name;
         public Color Color;
      }
   }
}