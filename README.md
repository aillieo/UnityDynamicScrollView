基于UGUI的动态滚动列表，主要支持以下功能：

1. 继承自UGUI的`SrollRect`，支持`ScrollRect`的所有功能；
2. 使用对象池来管理列表元素，以实现列表元素的复用；
3. 支持一行多个元素或一列多个元素；
4. 可使用不同尺寸的列表元素；
5. 列表数据变化后动态刷新列表元素；

用法详见工程中附带的示例，场景`Scene`及脚本`TestScript`。

![test.gif](test.gif)


---



A DynamicScrollView component based on UGUI that has the following features:

1. Extented from `SrollRect`(UGUI class), so it has features `ScrollRect` has;
2. Scroll items are managed by an object pool so they can be recycled and reused;
3. More than one items in one row (or column) is supported;
4. Items can have different sizes;
5. Items can be updated when scrollview data change;

For more details please run `Scene` or view `TestScript`.