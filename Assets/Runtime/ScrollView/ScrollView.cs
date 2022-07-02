using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace AillieoUtils
{
    [RequireComponent(typeof(RectTransform))]
    [DisallowMultipleComponent]
    public class ScrollView : ScrollRect
    {
        private class ScrollItemWithRect
        {
            // scroll item 身上的 RectTransform组件
            public RectTransform item;

            // scroll item 在scrollview中的位置
            public Rect rect;

            // rect 是否需要更新
            public bool rectDirty = true;
        }

        private int dataCount = 0;
        private readonly List<ScrollItemWithRect> managedItems = new List<ScrollItemWithRect>();

        // for hide and show
        public enum ItemLayoutType
        {
                                            // 最后一位表示滚动方向
            Vertical = 0b0001,                   // 0001
            Horizontal = 0b0010,                 // 0010
            VerticalThenHorizontal = 0b0100,     // 0100
            HorizontalThenVertical = 0b0101,     // 0101
        }

        protected const int flagScrollDirection = 1;  // 0001

        [SerializeField][FormerlySerializedAs("m_layoutType")]
        protected ItemLayoutType layoutType = ItemLayoutType.Vertical;

        // const int 代替 enum 减少 (int)和(CriticalItemType)转换
        protected static class CriticalItemType
        {
            public static byte UpToHide = 0;
            public static byte DownToHide = 1;
            public static byte UpToShow = 2;
            public static byte DownToShow = 3;
        }

        // 只保存4个临界index
        protected int[] criticalItemIndex = new int[4];
        private Rect refRect;

        // resource management
        private SimpleObjPool<RectTransform> itemPool = null;

        [Tooltip("初始化时池内item数量")]
        public int poolSize;

        [Tooltip("默认item尺寸")]
        public Vector2 defaultItemSize;

        [Tooltip("item的模板")]
        public RectTransform itemTemplate;

        // callbacks for items
        protected Action<int, RectTransform> updateFunc;
        protected Func<int, Vector2> itemSizeFunc;
        protected Func<int> itemCountFunc;
        protected Func<int, RectTransform> itemGetFunc;
        protected Action<RectTransform> itemRecycleFunc;

        // status
        private bool initialized = false;
        private int willUpdateData = 0;

        public virtual void SetUpdateFunc(Action<int, RectTransform> func)
        {
            updateFunc = func;
        }

        public virtual void SetItemSizeFunc(Func<int, Vector2> func)
        {
            itemSizeFunc = func;
        }

        public virtual void SetItemCountFunc(Func<int> func)
        {
            itemCountFunc = func;
        }

        public void SetItemGetAndRecycleFunc(Func<int, RectTransform> getFunc, Action<RectTransform> recycleFunc)
        {
            if(getFunc != null && recycleFunc != null)
            {
                itemGetFunc = getFunc;
                itemRecycleFunc = recycleFunc;
            }
            else
            {
                itemGetFunc = null;
                itemRecycleFunc = null;
            }
        }

        public void ResetAllDelegates()
        {
            SetUpdateFunc(null);
            SetItemSizeFunc(null);
            SetItemCountFunc(null);
            SetItemGetAndRecycleFunc(null, null);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (willUpdateData != 0)
            {
                StartCoroutine(DelayUpdateData());
            }
        }

        protected override void OnDisable()
        {
            initialized = false;
            base.OnDisable();
        }

        public void UpdateData(bool immediately = true)
        {
            if (immediately)
            {
                willUpdateData |= 3; // 0011
                InternalUpdateData();
            }
            else
            {
                if(willUpdateData == 0 && IsActive())
                {
                    StartCoroutine(DelayUpdateData());
                }
                willUpdateData |= 3;
            }
        }

        public void UpdateDataIncrementally(bool immediately = true)
        {
            if (immediately)
            {
                willUpdateData |= 1; // 0001
                InternalUpdateData();
            }
            else
            {
                if (willUpdateData == 0)
                {
                    StartCoroutine(DelayUpdateData());
                }
                willUpdateData |= 1;
            }
        }

        public void ScrollTo(int index)
        {
            InternalScrollTo(index);
        }

        protected virtual void InternalScrollTo(int index)
        {
            index = Mathf.Clamp(index, 0, dataCount - 1);
            EnsureItemRect(index);
            Rect r = managedItems[index].rect;
            int dir = (int)layoutType & flagScrollDirection;
            if (dir == 1)
            {
                // vertical
                float value = 1 - (-r.yMax / (content.sizeDelta.y - refRect.height));
                //value = Mathf.Clamp01(value);
                SetNormalizedPosition(value, 1);
            }
            else
            {
                // horizontal
                float value = r.xMin / (content.sizeDelta.x - refRect.width);
                //value = Mathf.Clamp01(value);
                SetNormalizedPosition(value, 0);
            }
        }

        private IEnumerator DelayUpdateData()
        {
            yield return new WaitForEndOfFrame();
            InternalUpdateData();
        }

        private void InternalUpdateData()
        {
            if (!IsActive())
            {
                willUpdateData |= 3;
                return;
            }

            if (!initialized)
            {
                InitScrollView();
            }

            int newDataCount = 0;
            bool keepOldItems = ((willUpdateData & 2) == 0);

            if (itemCountFunc != null)
            {
                newDataCount = itemCountFunc();
            }

            if (newDataCount != managedItems.Count)
            {
                if (managedItems.Count < newDataCount) //增加
                {
                    if(!keepOldItems)
                    {
                        foreach (var itemWithRect in managedItems)
                        {
                            // 重置所有rect
                            itemWithRect.rectDirty = true;
                        }
                    }

                    while (managedItems.Count < newDataCount)
                    {
                        managedItems.Add(new ScrollItemWithRect());
                    }
                }
                else //减少 保留空位 避免GC
                {
                    for (int i = 0, count = managedItems.Count; i < count; ++i)
                    {
                        if(i < newDataCount)
                        {
                            // 重置所有rect
                            if(!keepOldItems)
                            {
                                managedItems[i].rectDirty = true;
                            }

                            if(i == newDataCount - 1)
                            {
                                managedItems[i].rectDirty = true;
                            }
                        }

                        // 超出部分 清理回收item
                        if (i >= newDataCount)
                        {
                            managedItems[i].rectDirty = true;
                            if (managedItems[i].item != null)
                            {
                                RecycleOldItem(managedItems[i].item);
                                managedItems[i].item = null;
                            }
                        }
                    }
                }
            }
            else
            {
                if(!keepOldItems)
                {
                    for (int i = 0, count = managedItems.Count; i < count; ++i)
                    {
                        // 重置所有rect
                        managedItems[i].rectDirty = true;
                    }
                }
            }

            dataCount = newDataCount;

            ResetCriticalItems();

            willUpdateData = 0;
        }

        private void ResetCriticalItems()
        {
            bool hasItem, shouldShow;
            int firstIndex = -1, lastIndex = -1;

            for (int i = 0; i < dataCount; i++)
            {
                hasItem = managedItems[i].item != null;
                shouldShow = ShouldItemSeenAtIndex(i);

                if (shouldShow)
                {
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
                    //if (firstIndex != -1)
                    //{
                    //    // 已经遍历完所有要显示的了 后边的先跳过
                    //    break;
                    //}
                    continue;
                }

                if (hasItem && !shouldShow)
                {
                    // 不该显示 但是有
                    RecycleOldItem(managedItems[i].item);
                    managedItems[i].item = null;
                    continue;
                }

                if (shouldShow && !hasItem)
                {
                    // 需要显示 但是没有
                    RectTransform item = GetNewItem(i);
                    OnGetItemForDataIndex(item, i);
                    managedItems[i].item = item;
                    continue;
                }

            }

            // content.localPosition = Vector2.zero;
            criticalItemIndex[CriticalItemType.UpToHide] = firstIndex;
            criticalItemIndex[CriticalItemType.DownToHide] = lastIndex;
            criticalItemIndex[CriticalItemType.UpToShow] = Mathf.Max(firstIndex - 1, 0);
            criticalItemIndex[CriticalItemType.DownToShow] = Mathf.Min(lastIndex + 1, dataCount - 1);

        }

        protected override void SetContentAnchoredPosition(Vector2 position)
        {
            base.SetContentAnchoredPosition(position);
            UpdateCriticalItems();
        }

        protected override void SetNormalizedPosition(float value, int axis)
        {
            base.SetNormalizedPosition(value, axis);
            ResetCriticalItems();
        }

        private RectTransform GetCriticalItem(int type)
        {
            int index = criticalItemIndex[type];
            if(index >= 0 && index < dataCount)
            {
                return managedItems[index].item;
            }
            return null;
        }

        private void UpdateCriticalItems()
        {
            bool dirty = true;

            while (dirty)
            {
                dirty = false;

                for (int i = CriticalItemType.UpToHide; i <= CriticalItemType.DownToShow; i ++)
                {
                    if(i <= CriticalItemType.DownToHide) //隐藏离开可见区域的item
                    {
                        dirty = dirty || CheckAndHideItem(i);
                    }
                    else  //显示进入可见区域的item
                    {
                        dirty = dirty || CheckAndShowItem(i);
                    }
                }
            }
        }

        private bool CheckAndHideItem(int criticalItemType)
        {
            RectTransform item = GetCriticalItem(criticalItemType);
            int criticalIndex = criticalItemIndex[criticalItemType];
            if (item != null && !ShouldItemSeenAtIndex(criticalIndex))
            {
                RecycleOldItem(item);
                managedItems[criticalIndex].item = null;
                //Debug.Log("回收了 " + criticalIndex);

                if (criticalItemType == CriticalItemType.UpToHide)
                {
                    // 最上隐藏了一个
                    criticalItemIndex[criticalItemType + 2] = Mathf.Max(criticalIndex, criticalItemIndex[criticalItemType + 2]);
                    criticalItemIndex[criticalItemType]++;
                }
                else
                {
                    // 最下隐藏了一个
                    criticalItemIndex[criticalItemType + 2] = Mathf.Min(criticalIndex, criticalItemIndex[criticalItemType + 2]);
                    criticalItemIndex[criticalItemType]--;
                }
                criticalItemIndex[criticalItemType] = Mathf.Clamp(criticalItemIndex[criticalItemType], 0, dataCount - 1);

                if (criticalItemIndex[CriticalItemType.UpToHide] > criticalItemIndex[CriticalItemType.DownToHide])
                {
                    // 偶然的情况 拖拽超出一屏
                    ResetCriticalItems();
                    return false;
                }

                return true;
            }

            return false;
        }

        private bool CheckAndShowItem(int criticalItemType)
        {
            RectTransform item = GetCriticalItem(criticalItemType);
            int criticalIndex = criticalItemIndex[criticalItemType];
            //if (item == null && ShouldItemFullySeenAtIndex(criticalItemIndex[criticalItemType - 2]))

            if (item == null && ShouldItemSeenAtIndex(criticalIndex))
            {
                RectTransform newItem = GetNewItem(criticalIndex);
                OnGetItemForDataIndex(newItem, criticalIndex);
                //Debug.Log("创建了 " + criticalIndex);
                managedItems[criticalIndex].item = newItem;


                if (criticalItemType == CriticalItemType.UpToShow)
                {
                    // 最上显示了一个
                    criticalItemIndex[criticalItemType - 2] = Mathf.Min(criticalIndex, criticalItemIndex[criticalItemType - 2]);
                    criticalItemIndex[criticalItemType]--;
                }
                else
                {
                    // 最下显示了一个
                    criticalItemIndex[criticalItemType - 2] = Mathf.Max(criticalIndex, criticalItemIndex[criticalItemType - 2]);
                    criticalItemIndex[criticalItemType]++;
                }
                criticalItemIndex[criticalItemType] = Mathf.Clamp(criticalItemIndex[criticalItemType], 0, dataCount - 1);

                if (criticalItemIndex[CriticalItemType.UpToShow] >= criticalItemIndex[CriticalItemType.DownToShow])
                {
                    // 偶然的情况 拖拽超出一屏
                    ResetCriticalItems();
                    return false;
                }

                return true;
            }
            return false;
        }

        private bool ShouldItemSeenAtIndex(int index)
        {
            if(index < 0 || index >= dataCount)
            {
                return false;
            }
            EnsureItemRect(index);
            return new Rect(refRect.position - content.anchoredPosition, refRect.size).Overlaps(managedItems[index].rect);
        }

        private bool ShouldItemFullySeenAtIndex(int index)
        {
            if (index < 0 || index >= dataCount)
            {
                return false;
            }
            EnsureItemRect(index);
            return IsRectContains(new Rect(refRect.position - content.anchoredPosition, refRect.size),(managedItems[index].rect));
        }

        private bool IsRectContains(Rect outRect, Rect inRect, bool bothDimensions = false)
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

        private void InitPool()
        {
            GameObject poolNode = new GameObject("POOL");
            poolNode.SetActive(false);
            poolNode.transform.SetParent(transform,false);
            itemPool = new SimpleObjPool<RectTransform>(
                poolSize,
                (RectTransform item) => {
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

                    itemObj.SetActive(true);
                    return item;
                });
        }

        private void OnGetItemForDataIndex(RectTransform item, int index)
        {
            SetDataForItemAtIndex(item, index);
            item.transform.SetParent(content, false);
        }

        private void SetDataForItemAtIndex(RectTransform item, int index)
        {
            if (updateFunc != null)
                updateFunc(index,item);

            SetPosForItemAtIndex(item,index);
        }

        private void SetPosForItemAtIndex(RectTransform item, int index)
        {
            EnsureItemRect(index);
            Rect r = managedItems[index].rect;
            item.localPosition = r.position;
            item.sizeDelta = r.size;
        }

        private Vector2 GetItemSize(int index)
        {
            if(index >= 0 && index <= dataCount)
            {
                if (itemSizeFunc != null)
                {
                    return itemSizeFunc(index);
                }
            }
            return defaultItemSize;
        }

        private RectTransform GetNewItem(int index)
        {
            RectTransform item;
            if(itemGetFunc != null)
            {
                item = itemGetFunc(index);
            }
            else
            {
                item = itemPool.Get();
            }
            return item;
        }

        private void RecycleOldItem(RectTransform item)
        {
            if (itemRecycleFunc != null)
            {
                itemRecycleFunc(item);
            }
            else
            {
                itemPool.Recycle(item);
            }
        }

        private void InitScrollView()
        {
            initialized = true;

            // 根据设置来控制原ScrollRect的滚动方向
            int dir = (int)layoutType & flagScrollDirection;
            vertical = (dir == 1);
            horizontal = (dir == 0);

            content.pivot = Vector2.up;
            content.anchorMin = Vector2.up;
            content.anchorMax = Vector2.up;
            content.anchoredPosition = Vector2.zero;

            InitPool();
            UpdateRefRect();
        }


        private Vector3[] viewWorldConers = new Vector3[4];
        private Vector3[] rectCorners = new Vector3[2];
        private void UpdateRefRect()
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

            if (!CanvasUpdateRegistry.IsRebuildingLayout())
            {
                Canvas.ForceUpdateCanvases();
            }

            viewRect.GetWorldCorners(viewWorldConers);
            rectCorners[0] = content.transform.InverseTransformPoint(viewWorldConers[0]);
            rectCorners[1] = content.transform.InverseTransformPoint(viewWorldConers[2]);
            refRect = new Rect((Vector2)rectCorners[0] - content.anchoredPosition, rectCorners[1] - rectCorners[0]);
        }

        private void MovePos(ref Vector2 pos, Vector2 size)
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

        protected void EnsureItemRect(int index)
        {
            if (!managedItems[index].rectDirty)
            {
                // 已经是干净的了
                return;
            }

            ScrollItemWithRect firstItem = managedItems[0];
            if(firstItem.rectDirty)
            {
                Vector2 firstSize = GetItemSize(0);
                firstItem.rect = CreateWithLeftTopAndSize(Vector2.zero, firstSize);
                firstItem.rectDirty = false;
            }

            // 当前item之前的最近的已更新的rect
            int nearestClean = 0;
            for (int i = index; i >= 0; --i)
            {
                if(!managedItems[i].rectDirty)
                {
                    nearestClean = i;
                    break;
                }
            }

            // 需要更新 从 nearestClean 到 index 的尺寸
            Rect nearestCleanRect = managedItems[nearestClean].rect;
            Vector2 curPos = GetLeftTop(nearestCleanRect);
            Vector2 size = nearestCleanRect.size;
            MovePos(ref curPos, size);

            for (int i = nearestClean + 1; i <= index; i++)
            {
                size = GetItemSize(i);
                managedItems[i].rect = CreateWithLeftTopAndSize(curPos,size);
                managedItems[i].rectDirty = false;
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

        private static Vector2 GetLeftTop(Rect rect)
        {
            Vector2 ret = rect.position;
            ret.y += rect.size.y;
            return ret;
        }

        private static Rect CreateWithLeftTopAndSize(Vector2 leftTop, Vector2 size)
        {
            Vector2 leftBottom = leftTop - new Vector2(0,size.y);
            return new Rect(leftBottom,size);
        }

        protected override void OnDestroy()
        {
            if (itemPool != null)
            {
                itemPool.Purge();
            }
        }

        protected Rect GetItemLocalRect(int index)
        {
            if(index >= 0 && index < dataCount)
            {
                EnsureItemRect(index);
                return managedItems[index].rect;
            }
            return new Rect();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            int dir = (int)layoutType & flagScrollDirection;
            if (dir == 1)
            {
                // vertical
                if (horizontalScrollbar != null)
                {
                    horizontalScrollbar.gameObject.SetActive(false);
                    horizontalScrollbar = null;
                }
            }
            else
            {
                // horizontal
                if (verticalScrollbar != null)
                {
                    verticalScrollbar.gameObject.SetActive(false);
                    verticalScrollbar = null;
                }
            }

            base.OnValidate();
        }
#endif
    }
}
