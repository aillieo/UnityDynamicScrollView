using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AillieoUtils
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class ScrollItem : MonoBehaviour
    {
        RectTransform m_rectTransform;
        public RectTransform rectTransform {
            get
            {
                if(null == m_rectTransform)
                {
                    m_rectTransform = GetComponent<RectTransform>();
                }
                return m_rectTransform;
            }
        }
    }

}