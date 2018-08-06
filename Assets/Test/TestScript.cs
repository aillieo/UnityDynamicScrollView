using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using AillieoUtils;
using UnityEngine.UI;

public class TestScript : MonoBehaviour {

    void updateFunc(ScrollItem item, IScrollItemData data)
    {
        item.gameObject.SetActive(true);
        DefaultScrollItemData sd = (DefaultScrollItemData)data;
        item.transform.Find("Text").GetComponent<Text>().text = string.Format("{0}_{1}", sd.name, sd.id);
    }

    Vector2 itemSizeFunc(int index)
    {
        DefaultScrollItemData sd = (DefaultScrollItemData)testData[index];
        if(sd.name == "XXL")
            return new Vector2(300, 120);
        else if (sd.name == "XL")
            return new Vector2(250, 110);
        else if (sd.name == "L")
            return new Vector2(200, 100);
        else if (sd.name == "M")
            return new Vector2(150, 90);
        else // "S"
            return new Vector2(100, 80);
    }

    IScrollItemData[] testData = new IScrollItemData[] 
    {
        new DefaultScrollItemData { name =   "XL",     id = 0 },
        new DefaultScrollItemData { name =   "XL",     id = 1 },
        new DefaultScrollItemData { name =   "S",      id = 2 },
        new DefaultScrollItemData { name =   "M",      id = 3 },
        new DefaultScrollItemData { name =   "XXL",    id = 4 },
        new DefaultScrollItemData { name =   "S",      id = 5 },
        new DefaultScrollItemData { name =   "M",      id = 6 },
        new DefaultScrollItemData { name =   "L",      id = 7 },
        new DefaultScrollItemData { name =   "XL",     id = 8 },
        new DefaultScrollItemData { name =   "XXL",    id = 9 },
        new DefaultScrollItemData { name =   "S",      id = 10 },
        new DefaultScrollItemData { name =   "M",      id = 11 },
        new DefaultScrollItemData { name =   "XXL",    id = 12 },
        new DefaultScrollItemData { name =   "S",      id = 13 },
        new DefaultScrollItemData { name =   "M",      id = 14 },
        new DefaultScrollItemData { name =   "L",      id = 15 },
        new DefaultScrollItemData { name =   "M",      id = 16 },
        new DefaultScrollItemData { name =   "L",      id = 17 },
        new DefaultScrollItemData { name =   "XL",     id = 18 },
        new DefaultScrollItemData { name =   "XXL",    id = 19 },
        new DefaultScrollItemData { name =   "S",      id = 20 },
        new DefaultScrollItemData { name =   "M",      id = 21 },
        new DefaultScrollItemData { name =   "XXL",    id = 22 },
        new DefaultScrollItemData { name =   "S",      id = 23 },
        new DefaultScrollItemData { name =   "M",      id = 24 },
        new DefaultScrollItemData { name =   "L",      id = 25 },
    };

    void Start () {

        ScrollView sv_1 = GameObject.Find("ScrollView_1").GetComponent<ScrollView>();
        ScrollView sv_2 = GameObject.Find("ScrollView_2").GetComponent<ScrollView>();

        sv_1.SetUpdateFuncCS(updateFunc);
        sv_2.SetUpdateFuncCS(updateFunc);

        sv_2.SetItemSizeFuncCS(itemSizeFunc);

        sv_1.Init(testData);
        sv_2.Init(testData);
    }
}
