using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SObject = System.Object;

namespace AillieoUtils
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class ScrollViewEx : ScrollView
    {
        protected override void Awake()
        {
            base.Awake();
            onValueChanged.AddListener(OnValueChanged);
        }

        [SerializeField]
        private int m_pageSize = 50;

        public int pageSize => m_pageSize;

        private int startOffset = 0;

        private Func<int> realItemCountFunc;

        public override void SetUpdateFunc(Action<int, RectTransform> func)
        {
            if(func != null)
            {
                var f = func;
                func = (index, rect) => {
                    f(index + startOffset, rect);
                };
            }
            base.SetUpdateFunc(func);
        }

        public override void SetItemSizeFunc(Func<int, Vector2> func)
        {
            if (func != null)
            {
                var f = func;
                func = (index) => {
                    return f(index + startOffset);
                };
            }
            base.SetItemSizeFunc(func);
        }

        public override void SetItemCountFunc(Func<int> func)
        {
            realItemCountFunc = func;
            if(func != null)
            {
                var f = func;
                func = () => Mathf.Min(f(), pageSize);
            }
            base.SetItemCountFunc(func);
        }

        protected override void InternalScrollTo(int index)
        {
            int count = 0;
            if (realItemCountFunc != null)
            {
                count = realItemCountFunc();
            }
            index = Mathf.Clamp(index, 0, count - 1);
            startOffset = Mathf.Clamp(index - pageSize / 2, 0, count - itemCountFunc());
            UpdateData(true);
            //Debug.LogError($"index={index} startOffset={startOffset} first={index - startOffset}");
            base.InternalScrollTo(index - startOffset);
        }

        private void OnValueChanged(Vector2 position)
        {
            int toShow;
            int critical;
            bool downward;
            int pin;
            if (((int)layoutType & flagScrollDirection) == 1)
            {
                // 垂直滚动 只计算y向
                if (velocity.y > 0)
                {
                    // 向上
                    toShow = criticalItemIndex[CriticalItemType.DownToShow];
                    critical = pageSize - 1;
                    if (toShow < critical)
                    {
                        return;
                    }
                    pin = critical - 1;
                    downward = false;
                }
                else
                {
                    // 向下
                    toShow = criticalItemIndex[CriticalItemType.UpToShow];
                    critical = 0;
                    if(toShow > critical)
                    {
                        return;
                    }
                    pin = critical + 1;
                    downward = true;
                }
            }
            else // = 0
            {
                // 水平滚动 只计算x向
                if (velocity.x > 0)
                {
                    // 向右
                    toShow = criticalItemIndex[CriticalItemType.UpToShow];
                    critical = 0;
                    if (toShow > critical)
                    {
                        return;
                    }
                    pin = critical + 1;
                    downward = true;
                }
                else
                {
                    // 向左
                    toShow = criticalItemIndex[CriticalItemType.DownToShow];
                    critical = pageSize - 1;
                    if (toShow < critical)
                    {
                        return;
                    }
                    pin = critical - 1;
                    downward = false;
                }
            }

            // 该翻页了 翻半页吧
            int old = startOffset;
            if (downward)
            {
                startOffset -= pageSize / 2;
            }
            else
            {
                startOffset += pageSize / 2;
            }

            int realDataCount = 0;
            if (realItemCountFunc != null)
            {
                realDataCount = realItemCountFunc();
            }
            startOffset = Mathf.Clamp(startOffset, 0, Mathf.Max(realDataCount - pageSize, 0));

            if (old != startOffset)
            {
                // 记录 原先的速度
                Vector2 oldVelocity = velocity;
                // 计算 pin元素的世界坐标
                Rect rect = GetItemLocalRect(pin);
                Vector2 oldWorld = content.TransformPoint(rect.position);
                UpdateData(true);
                int dataCount = 0;
                if(itemCountFunc != null)
                {
                    dataCount = itemCountFunc();
                }
                if(dataCount > 0)
                {
                    EnsureItemRect(0);
                    if(dataCount > 1)
                    {
                        EnsureItemRect(dataCount - 1);
                    }
                }
                // 根据 pin元素的世界坐标 计算出content的position
                int pin2 = pin + old - startOffset;
                Rect rect2 = GetItemLocalRect(pin2);
                Vector2 newWorld = content.TransformPoint(rect2.position);
                Vector2 deltaWorld = newWorld - oldWorld;
                Vector2 deltaLocal = content.InverseTransformVector(deltaWorld);
                // Debug.LogError($"critical={critical} toShow={toShow} pin={pin} pin2={pin2} pinpos={rect.position} pin2pos={rect2.position} pinworld={oldWorld} pin2world={newWorld} deltaLocal = {deltaLocal}");
                SetContentAnchoredPosition(content.anchoredPosition - deltaLocal);
                UpdateData(true);
                UpdateData(false);
                // Debug.LogError($"critical={critical} toShow={toShow} pin={pin} pin2={pin2} pinpos={rect.position} pin2pos={GetItemLocalRect(pin2).position} pinworld={oldWorld} pin2world={content.TransformPoint(GetItemLocalRect(pin2).position)} ===");
                // 取回速度
                velocity = oldVelocity;
            }

        }
    }
}
