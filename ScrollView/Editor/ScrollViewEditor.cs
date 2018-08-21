using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AillieoUtils
{

    [CustomEditor(typeof(ScrollView))]
    public class ScrollViewEditor : ScrollRectEditor
    {

        SerializedProperty itemTemplate;
        SerializedProperty poolSize;
        SerializedProperty maxShownCount;
        SerializedProperty defaultItemSize;
        SerializedProperty layoutType;


        protected override void OnEnable()
        {
            base.OnEnable();

            itemTemplate = serializedObject.FindProperty("itemTemplate");
            poolSize = serializedObject.FindProperty("poolSize");
            maxShownCount = serializedObject.FindProperty("maxShownCount");
            defaultItemSize = serializedObject.FindProperty("defaultItemSize");
            layoutType = serializedObject.FindProperty("m_layoutType");
        }
        

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Separator();

            EditorGUILayout.LabelField("<color=#0000FF><b>Additional configs</b></color>", new GUIStyle { richText = true });

            EditorGUI.indentLevel++;

            EditorGUILayout.PropertyField(itemTemplate);
            EditorGUILayout.PropertyField(poolSize);
            EditorGUILayout.PropertyField(maxShownCount);
            EditorGUILayout.PropertyField(defaultItemSize);
            layoutType.intValue = (int)(ScrollView.ItemLayoutType)EditorGUILayout.EnumPopup("layoutType", (ScrollView.ItemLayoutType)layoutType.intValue);


            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Separator();
            
            EditorGUI.indentLevel--;

            EditorGUILayout.LabelField("<color=#0000FF><b>For original ScrollRect</b></color>", new GUIStyle { richText = true });

            EditorGUI.indentLevel++;

            base.OnInspectorGUI();

            EditorGUI.indentLevel--;

        }
        







        const string bgPath = "UI/Skin/Background.psd";
        const string spritePath = "UI/Skin/UISprite.psd";
        const string maskPath = "UI/Skin/UIMask.psd";
        static Color panelColor = new Color(1f, 1f, 1f, 0.392f);
        static Color defaultSelectableColor = new Color(1f, 1f, 1f, 1f);
        static Vector2 thinElementSize = new Vector2(160f, 20f);



        [MenuItem("GameObject/UI/Dynamic Scroll View", false, 90)]
        static public void AddScrollView(MenuCommand menuCommand)
        {
            GameObject root = CreateUIElementRoot("Dynamic Scroll View", new Vector2(200, 200));

            GameObject viewport = CreateUIObject("Viewport", root);
            GameObject content = CreateUIObject("Content", viewport);

            GameObject parent = menuCommand.context as GameObject;
            if (parent != null)
            {
                root.transform.SetParent(parent.transform, false);
            }
            Selection.activeGameObject = root;



            GameObject hScrollbar = CreateScrollbar();
            hScrollbar.name = "Scrollbar Horizontal";
            hScrollbar.transform.SetParent(root.transform, false);
            RectTransform hScrollbarRT = hScrollbar.GetComponent<RectTransform>();
            hScrollbarRT.anchorMin = Vector2.zero;
            hScrollbarRT.anchorMax = Vector2.right;
            hScrollbarRT.pivot = Vector2.zero;
            hScrollbarRT.sizeDelta = new Vector2(0, hScrollbarRT.sizeDelta.y);

            GameObject vScrollbar = CreateScrollbar();
            vScrollbar.name = "Scrollbar Vertical";
            vScrollbar.transform.SetParent(root.transform, false);
            vScrollbar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);
            RectTransform vScrollbarRT = vScrollbar.GetComponent<RectTransform>();
            vScrollbarRT.anchorMin = Vector2.right;
            vScrollbarRT.anchorMax = Vector2.one;
            vScrollbarRT.pivot = Vector2.one;
            vScrollbarRT.sizeDelta = new Vector2(vScrollbarRT.sizeDelta.x, 0);



            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.pivot = Vector2.up;

            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.up;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = new Vector2(0, 300);
            contentRect.pivot = Vector2.up;

            ScrollView scrollRect = root.AddComponent<ScrollView>();
            scrollRect.content = contentRect;
            scrollRect.viewport = viewportRect;
            scrollRect.horizontalScrollbar = hScrollbar.GetComponent<Scrollbar>();
            scrollRect.verticalScrollbar = vScrollbar.GetComponent<Scrollbar>();
            scrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollRect.horizontalScrollbarSpacing = -3;
            scrollRect.verticalScrollbarSpacing = -3;

            Image rootImage = root.AddComponent<Image>();
            rootImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(bgPath);
            rootImage.type = Image.Type.Sliced;
            rootImage.color = panelColor;

            Mask viewportMask = viewport.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;

            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(maskPath);
            viewportImage.type = Image.Type.Sliced;
        }



        static GameObject CreateScrollbar()
        {
            // Create GOs Hierarchy
            GameObject scrollbarRoot = CreateUIElementRoot("Scrollbar", thinElementSize);
            GameObject sliderArea = CreateUIObject("Sliding Area", scrollbarRoot);
            GameObject handle = CreateUIObject("Handle", sliderArea);

            Image bgImage = scrollbarRoot.AddComponent<Image>();
            bgImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(bgPath);
            bgImage.type = Image.Type.Sliced;
            bgImage.color = defaultSelectableColor;

            Image handleImage = handle.AddComponent<Image>();
            handleImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>(spritePath);
            handleImage.type = Image.Type.Sliced;
            handleImage.color = defaultSelectableColor;

            RectTransform sliderAreaRect = sliderArea.GetComponent<RectTransform>();
            sliderAreaRect.sizeDelta = new Vector2(-20, -20);
            sliderAreaRect.anchorMin = Vector2.zero;
            sliderAreaRect.anchorMax = Vector2.one;

            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);

            Scrollbar scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
            scrollbar.handleRect = handleRect;
            scrollbar.targetGraphic = handleImage;
            SetDefaultColorTransitionValues(scrollbar);

            return scrollbarRoot;
        }


        static GameObject CreateUIElementRoot(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetParentAndAlign(go, parent);
            return go;
        }

        private static void SetParentAndAlign(GameObject child, GameObject parent)
        {
            if (parent == null)
                return;

            child.transform.SetParent(parent.transform, false);
            SetLayerRecursively(child, parent.layer);
        }

        static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

        static void SetDefaultColorTransitionValues(Selectable slider)
        {
            ColorBlock colors = slider.colors;
            colors.highlightedColor = new Color(0.882f, 0.882f, 0.882f);
            colors.pressedColor = new Color(0.698f, 0.698f, 0.698f);
            colors.disabledColor = new Color(0.521f, 0.521f, 0.521f);
        }
    }
}