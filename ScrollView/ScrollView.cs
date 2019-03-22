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
    public class ScrollView : ScrollRect
    {

        struct ScrollItemWithRect
        {
            // scroll item 身上的 RectTransform组件
            public RectTransform item;

            // scroll item 在scrollview中的位置
            public Rect rect;
        }


        // about data
        IList csData;

        int m_dataCount = 0;
        ScrollItemWithRect[] managedItems;


        // for hide and show
        public enum ItemLayoutType
        {
                                            // 最后一位表示滚动方向
            Vertical = 1,                   // 0001
            Horizontal = 2,                 // 0010
            VerticalThenHorizontal = 4,     // 0100
            HorizontalThenVertical = 5,     // 0101
        }
        const int flagScrollDirection = 1;  // 0001


        [SerializeField]
        ItemLayoutType m_layoutType = ItemLayoutType.Vertical;
        ItemLayoutType layoutType { get { return m_layoutType; } }


        // const int 代替 enum 减少 (int)和(CriticalItemType)转换
        static class CriticalItemType
        {
            public const int UpToHide = 0;
            public const int DownToHide = 1;
            public const int UpToShow = 2;
            public const int DownToShow = 3;
        }
        // 只保存4个临界index
        int[] criticalItemIndex = new int[4];
        Rect refRect;


        // 当前移动方向
        Vector2 m_prevPosition;
        Vector2 m_curDelta;


        // resource management
        SimpleObjPool<RectTransform> itemPool = null;

        [Tooltip("初始化时池内item数量")]
        public int poolSize;

        [Tooltip("同时展示的item数量")]
        public int maxShownCount;

        [Tooltip("默认item尺寸")]
        public Vector2 defaultItemSize;

        [Tooltip("item的模板")]
        public RectTransform itemTemplate;


        // callbacks for items
        public Action<RectTransform, SObject> updateFuncCS;
        public Func<int,Vector2> itemSizeFuncCS;

        public Action<RectTransform> activateFunc;
        public Action<RectTransform> recycleFunc;


        public void SetUpdateFuncCS(Action<RectTransform, SObject> func)
        {
            updateFuncCS = func;
        }

        public void SetItemSizeFuncCS(Func<int, Vector2> func)
        {
            itemSizeFuncCS = func;
        }

        public void Init(IList data)
        {
            InitScrollView();
            UpdateData(data);
        }


        public void UpdateData(IList data)
        {
            csData = data;
            UpdateManagedItems(m_dataCount != data.Count);
        }

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);

            m_curDelta = content.anchoredPosition - m_prevPosition;
            m_prevPosition = content.anchoredPosition;

            UpdateCriticalItems();
        }


        RectTransform GetCriticalItem(int type)
        {
            int index = criticalItemIndex[type];
            if(index >= 0 && index < m_dataCount)
            {
                return managedItems[index].item;
            }
            return null;
        }


        bool IsCriticalItemTypeValid(int type)
        {
            int dir = (int)layoutType & flagScrollDirection;

            if (dir == 1)
            {
                if (m_curDelta[dir] > 0)
                {
                    return type == CriticalItemType.UpToHide || type == CriticalItemType.DownToShow;
                }
                else if (m_curDelta[dir] < 0)
                {
                    return type == CriticalItemType.DownToHide || type == CriticalItemType.UpToShow;
                }
            }
            else // dir == 0
            {
                if (m_curDelta[dir] < 0)
                {
                    return type == CriticalItemType.UpToHide || type == CriticalItemType.DownToShow;
                }
                else if (m_curDelta[dir] > 0)
                {
                    return type == CriticalItemType.DownToHide || type == CriticalItemType.UpToShow;
                }
            }

            return false;
        }


        void UpdateCriticalItems()
        {
            //Debug.LogWarning((m_curDelta.y > 0 ? "↑↑" : "↓↓") + " criticalItemIndex = {" + criticalItemIndex[0] + " " + criticalItemIndex[1] + " " + criticalItemIndex[2] + " " + criticalItemIndex[3] + "}");

            for (int i = CriticalItemType.UpToHide; i <= CriticalItemType.DownToShow; i ++)
            {
                if(!IsCriticalItemTypeValid(i))
                {
                    continue;
                }

                if(i <= CriticalItemType.DownToHide) //隐藏离开可见区域的item
                {
                    CheckAndHideItem(i);
                }
                else  //显示进入可见区域的item
                {
                    CheckAndShowItem(i);
                }
            }
        }


        void CheckAndHideItem(int criticalItemType)
        {
            RectTransform item = null;
            int criticalIndex = -1;
            while (true)
            {
                item = GetCriticalItem(criticalItemType);
                criticalIndex = criticalItemIndex[criticalItemType];
                if (item != null && !ShouldItemSeenAtIndex(criticalIndex))
                {
                    itemPool.Recycle(item);
                    managedItems[criticalIndex].item = null;
                    //Debug.Log("回收了 " + criticalIndex);
                    criticalItemIndex[criticalItemType + 2] = criticalIndex;
                    if (criticalItemType == CriticalItemType.UpToHide)
                    {
                        // 最上隐藏了一个
                        criticalItemIndex[criticalItemType]++;
                    }
                    else
                    {
                        // 最下隐藏了一个
                        criticalItemIndex[criticalItemType]--;
                    }
                    criticalItemIndex[criticalItemType] = Mathf.Clamp(criticalItemIndex[criticalItemType], 0, m_dataCount - 1);
                }
                else
                {
                    break;
                }

            }
        }


        void CheckAndShowItem(int criticalItemType)
        {
            RectTransform item = null;
            int criticalIndex = -1;

            while (true)
            {
                item = GetCriticalItem(criticalItemType);
                criticalIndex = criticalItemIndex[criticalItemType];
                
                //if (item == null && ShouldItemFullySeenAtIndex(criticalItemIndex[criticalItemType - 2]))

                if (item == null && ShouldItemSeenAtIndex(criticalIndex))
                {
                    RectTransform newItem = itemPool.Get();
                    OnGetItemForDataIndex(newItem, criticalIndex);
                    //Debug.Log("创建了 " + criticalIndex);
                    managedItems[criticalIndex].item = newItem;

                    criticalItemIndex[criticalItemType - 2] = criticalIndex;

                    if (criticalItemType == CriticalItemType.UpToShow)
                    {
                        // 最上显示了一个
                        criticalItemIndex[criticalItemType]--;
                    }
                    else
                    {
                        // 最下显示了一个
                        criticalItemIndex[criticalItemType]++;
                    }
                    criticalItemIndex[criticalItemType] = Mathf.Clamp(criticalItemIndex[criticalItemType], 0, m_dataCount - 1);
                }
                else
                {
                    break;
                }
            }
        }


        bool ShouldItemSeenAtIndex(int index)
        {
            if(index < 0 || index >= m_dataCount)
            {
                return false;
            }

            return new Rect(refRect.position - content.anchoredPosition, refRect.size).Overlaps(managedItems[index].rect);
        }

        bool ShouldItemFullySeenAtIndex(int index)
        {
            if (index < 0 || index >= m_dataCount)
            {
                return false;
            }
            return IsRectContains(new Rect(refRect.position - content.anchoredPosition, refRect.size),(managedItems[index].rect));
        }

        bool IsRectContains(Rect outRect, Rect inRect, bool bothDimensions = false)
        {

            if (bothDimensions)
            {
                bool xContains = (outRect.xMax >= inRect.xMax) && (outRect.xMin <= inRect.xMin);
                bool yContains = (outRect.yMax >= inRect.yMax) && (outRect.yMin <= inRect.yMin);
                return xContains && yContains;
            }
            else
            {
                int dir = (int)layoutType & flagScrollDirection;
                if(dir == 1)
                {
                    // 垂直滚动 只计算y向
                    return (outRect.yMax >= inRect.yMax) && (outRect.yMin <= inRect.yMin);
                }
                else // = 0
                {
                    // 水平滚动 只计算x向
                    return (outRect.xMax >= inRect.xMax) && (outRect.xMin <= inRect.xMin);
                }
            }
        }


        void InitPool()
        {
            GameObject poolNode = new GameObject("POOL");
            poolNode.SetActive(false);
            poolNode.transform.SetParent(transform,false);
            itemPool = new SimpleObjPool<RectTransform>(
                poolSize,
                (RectTransform item) => {
                    if(recycleFunc != null)
                        recycleFunc(item);
                    item.transform.SetParent(poolNode.transform,false);
                },
                () => {
                    GameObject itemObj = Instantiate(itemTemplate.gameObject);
                    RectTransform item = itemObj.GetComponent<RectTransform>();
                    itemObj.transform.SetParent(poolNode.transform,false);

                    item.anchorMin = Vector2.up;
                    item.anchorMax = Vector2.up;
                    item.pivot = Vector2.zero;

                    //rectTrans.pivot = Vector2.up;
                    return item;
                });
        }

        void UpdateManagedItems(bool resize)
        {
            if(resize)
            {
                int newDataCount = csData.Count;

                if (newDataCount > managedItems.Length)  //增加
                {
                    Array.Resize(ref managedItems,newDataCount);
                }
                else //减少
                {
                    for (int i = newDataCount; i < m_dataCount; i++)
                    {
                        if(managedItems[i].item != null)
                        {
                            itemPool.Recycle(managedItems[i].item);
                            managedItems[i].item = null;
                        }
                    }
                    //Array.Resize(ref managedItems, newDataCount); //减少时保留之前多余的空位
                }
                m_dataCount = newDataCount;
            }

            CacheRect();
            
            int showCount = Mathf.Min(maxShownCount,m_dataCount);
            int restCount = showCount;

            bool hasItem, shouldShow;
            int firstIndex = -1, lastIndex = -1;
            for (int i = 0; i < m_dataCount && restCount > 0; i++)
            {
                hasItem = managedItems[i].item != null;
                shouldShow = ShouldItemSeenAtIndex(i);

                if (shouldShow)
                {
                    restCount--;
                    if (firstIndex == -1)
                    {
                        firstIndex = i;
                    }
                    lastIndex = i;
                }

                if (hasItem && shouldShow)
                {
                    // 应显示且已显示
                    SetDataForItemAtIndex(managedItems[i].item, i);
                    continue;
                }

                if (hasItem == shouldShow)
                {
                    // 不应显示且未显示
                    continue;
                }

                if (hasItem && !shouldShow)
                {
                    // 不该显示 但是有
                    itemPool.Recycle(managedItems[i].item);
                    managedItems[criticalItemIndex[i]].item = null;
                    continue;
                }

                if(shouldShow && !hasItem)
                {
                    // 需要显示 但是没有
                    RectTransform item = itemPool.Get();
                    OnGetItemForDataIndex(item, i);
                    managedItems[i].item = item;
                    continue;
                }

            }

            // content.localPosition = Vector2.zero;
            criticalItemIndex[CriticalItemType.UpToHide] = firstIndex;
            criticalItemIndex[CriticalItemType.DownToHide] = lastIndex;
            criticalItemIndex[CriticalItemType.UpToShow] = Mathf.Max(firstIndex - 1, 0);
            criticalItemIndex[CriticalItemType.DownToShow] = Mathf.Min(lastIndex + 1, m_dataCount - 1);

        }


        void OnGetItemForDataIndex(RectTransform item, int index)
        {
            if (activateFunc != null)
                activateFunc(item);
            SetDataForItemAtIndex(item, index);
            item.transform.SetParent(content, false);
        }


        void SetDataForItemAtIndex(RectTransform item, int index)
        {
            if (updateFuncCS != null && csData != null)
                updateFuncCS(item, csData[index]);

            SetPosForItemAtIndex(item,index);
        }


        void SetPosForItemAtIndex(RectTransform item, int index)
        {
            Rect r = managedItems[index].rect;
            item.localPosition = r.position;
            item.sizeDelta = r.size;
        }


        Vector2 GetItemSize(int index)
        {
            if(index >= 0 && index <= m_dataCount)
            {
                        if (itemSizeFuncCS != null)
                        {
                            return itemSizeFuncCS(index);
                        }
            }
            return defaultItemSize;
        }


        void InitScrollView()
        {
            // 根据设置来控制原ScrollRect的滚动方向
            int dir = (int)layoutType & flagScrollDirection;
            vertical = (dir == 1);
            horizontal = (dir == 0);


            content.pivot = Vector2.up;
            InitPool();
            managedItems = new ScrollItemWithRect[0];
            UpdateRefRect();


            m_curDelta = content.anchoredPosition - m_prevPosition;
            m_prevPosition = content.anchoredPosition;

        }

        void UpdateRefRect()
        {
            /*
             *  WorldCorners
             * 
             *    1 ------- 2     
             *    |         |
             *    |         |
             *    0 ------- 3
             * 
             */

            // refRect是在Content节点下的 viewport的 rect

            Vector3[] viewWorldConers = new Vector3[4];
            Vector3[] rectCorners = new Vector3[2];
            viewRect.GetWorldCorners(viewWorldConers);

            rectCorners[0] = content.transform.InverseTransformPoint(viewWorldConers[0]);
            rectCorners[1] = content.transform.InverseTransformPoint(viewWorldConers[2]);
            refRect = new Rect((Vector2)rectCorners[0] - content.anchoredPosition, rectCorners[1] - rectCorners[0]);
        }

        void CacheRect()
        {
            Vector2 curPos = Vector2.zero, size = Vector2.zero;
            for(int i = 0; i < m_dataCount; i ++)
            {
                size = GetItemSize(i);
                managedItems[i].rect = new Rect(curPos.x,curPos.y-size.y,size.x,size.y);
                MovePos(ref curPos, size);
            }
            Vector2 range = new Vector2(Mathf.Abs(curPos.x), Mathf.Abs(curPos.y));
            switch (layoutType)
            {
                case ItemLayoutType.VerticalThenHorizontal:
                    range.x += size.x;
                    range.y = refRect.height;
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    range.x = refRect.width;
                    if (curPos.x != 0)
                    {
                        range.y += size.y;
                    }
                    break;
                default:
                    break;
            }
            content.sizeDelta = range;
        }

        void MovePos(ref Vector2 pos, Vector2 size)
        {
            // 注意 所有的rect都是左下角为基准
            switch (layoutType)
            {
                case ItemLayoutType.Vertical:
                    // 垂直方向 向下移动
                    pos.y -= size.y;
                    break;
                case ItemLayoutType.Horizontal:
                    // 水平方向 向右移动
                    pos.x += size.x;
                    break;
                case ItemLayoutType.VerticalThenHorizontal:
                    pos.y -= size.y;
                    if (pos.y <= - refRect.height)
                    {
                        pos.y = 0;
                        pos.x += size.x;
                    }
                    break;
                case ItemLayoutType.HorizontalThenVertical:
                    pos.x += size.x;
                    if(pos.x >= refRect.width)
                    {
                        pos.x = 0;
                        pos.y -= size.y;
                    }
                    break;
                default:
                    break;
            }
        }

        protected override void OnDestroy()
        {
            if (itemPool != null)
            {
                itemPool.Purge();
            }
        }


    }

}