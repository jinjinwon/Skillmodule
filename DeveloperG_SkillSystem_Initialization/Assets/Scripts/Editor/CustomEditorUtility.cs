using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public static class CustomEditorUtility
{
    #region 1-4
    private readonly static GUIStyle titleStyle;

    static CustomEditorUtility()
    {
        // 유니티 내부에 정의되어있는 ShurikenModuleTitle Style을 Base로 함
        titleStyle = new GUIStyle("ShurikenModuleTitle")
        {
            // 유니티 Default Label의 font를 가져옴
            font = new GUIStyle(EditorStyles.label).font,
            fontStyle = FontStyle.Bold,
            fontSize = 14,
            // title을 그릴 공간에 여유를 줌
            border = new RectOffset(15, 7, 4, 4),
            // 높이는 26
            fixedHeight = 26f,
            // 내부 Text의 위치를 조절함
            contentOffset = new Vector2(20f, -2f)
        };
    }
    #endregion

    #region 1-5
    public static bool DrawFoldoutTitle(string title, bool isExpanded, float space = 15f)
    {
        // space만큼 윗 줄을 띄움
        EditorGUILayout.Space(space);

        // titleStyle의 정보를 가지고 Inspector상에서 옳바른 위치를 가져옴
        var rect = GUILayoutUtility.GetRect(16f, titleStyle.fixedHeight, titleStyle);
        // TitleStyle을 적용시킨 Box를 그려줌
        GUI.Box(rect, title, titleStyle);

        // 현재 Editor의 Event를 가져옴
        // Editor Event는 마우스 입력, GUI 새로 그리기(Repaint), 키보드 입력 등 Editor 상에서 일어나는 일임
        var currentEvent = Event.current;
        // Toggle Button의 위치와 크기를 정함
        // 위치는 방금 그린 박스의 좌표에서 살짝 오른쪽 아래, 즉 Button이 좌, 가운데 정렬이 된 형태가 됨.
        var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);

        // Event가 Repaint(=GUI를 그린다 혹은 다시 그린다)면 단순히 foldout button을 보여줌
        if (currentEvent.type == EventType.Repaint)
            EditorStyles.foldout.Draw(toggleRect, false, false, isExpanded, false);
        // Event가 MouseDown이고 mousePosition이 rect와 겹쳐있으면(=Mouse Pointer가 위에서 그려준 Box안에 있음) Click 판정
        else if (currentEvent.type == EventType.MouseDown && rect.Contains(currentEvent.mousePosition))
        {
            isExpanded = !isExpanded;
            // Use 함수를 사용하지 않으면 아직 Event가 처리되지 않은 것으로 판단되어 같은 위치에 있는 다른 GUI도 같이 동작될 수 있음.
            // event 처리를 했으면 항상 Use를 통해 event에 대한 처리를 했음을 Unity에 알려주는게 좋음
            currentEvent.Use();
        }

        return isExpanded;
    }

    // FoldoutTitle을 그림과 동시에 인자로 받은 Dictionary에 Expand 상황을 저장까지 해줌
    public static bool DrawFoldoutTitle(IDictionary<string, bool> isFoldoutExpandedesByTitle, string title, float space = 15f)
    {
        if (!isFoldoutExpandedesByTitle.ContainsKey(title))
            isFoldoutExpandedesByTitle[title] = true;
        
        isFoldoutExpandedesByTitle[title] = DrawFoldoutTitle(title, isFoldoutExpandedesByTitle[title], space);
        return isFoldoutExpandedesByTitle[title];
    }
    #endregion
}
