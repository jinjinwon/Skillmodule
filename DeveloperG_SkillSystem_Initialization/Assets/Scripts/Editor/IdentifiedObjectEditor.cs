using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(IdentifiedObject), true)]
public class IdentifiedObjectEditor : Editor
{
    #region 1-6
    // SerializedProperty는 내가 보고 있는 객체의 public 혹은 [SerializeField] 어트리뷰트를 통해
    // Serailize된 변수들을 조작하기 위한 class
    private SerializedProperty categoriesProperty;
    private SerializedProperty iconProperty;
    private SerializedProperty idProperty;
    private SerializedProperty codeNameProperty;
    private SerializedProperty displayNameProperty;
    private SerializedProperty descriptionProperty;

    // Inspector 상에서 순서를 편집할 수 있는 List
    private ReorderableList categories;

    // text를 넓게 보여주는 Style(=Skin) 지정을 위한 변수
    private GUIStyle textAreaStyle;
    
    // Title의 Foldout Expand 상태를 저장하는 변수
    private readonly Dictionary<string, bool> isFoldoutExpandedesByTitle = new();
    #endregion

    #region 1-7
    protected virtual void OnEnable()
    {
        // Inspector에서 description을 편집하다가 다른 Inspector View로 넘어가는 경우에,
        // 포커스가 풀리지 않고 이전에 편집하던 desription 내용이 그대로 보이는 문제를 해결하기위해 포커스를 풀어줌
        GUIUtility.keyboardControl = 0;

        // serializedObject는 현재 내가 Editor에서 보고 있는 IdentifiedObject를 뜻함
        // 객체에서 Serialize 변수들을 찾아옴
        categoriesProperty = serializedObject.FindProperty("categories");
        iconProperty = serializedObject.FindProperty("icon");
        idProperty = serializedObject.FindProperty("id");
        codeNameProperty = serializedObject.FindProperty("codeName");
        displayNameProperty = serializedObject.FindProperty("displayName");
        descriptionProperty = serializedObject.FindProperty("description");

        categories = new(serializedObject, categoriesProperty);
        // List의 Prefix Label을 어떻게 그릴지 정함
        categories.drawHeaderCallback = rect => EditorGUI.LabelField(rect, categoriesProperty.displayName);
        // List의 Element를 어떻게 그릴지 정함
        categories.drawElementCallback = (rect, index, isActive, isFocused) => {
            rect = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
            // EditorGUILayout와 EditorGUI의 차이점
            // EditorGUILayout은 GUI를 그리는 순서에 따라 위치를 자동으로 조정해줌
            // EditorGUI는 사용자가 직접 GUI를 그릴 위치를 지정해줘야함
            EditorGUI.PropertyField(rect, categoriesProperty.GetArrayElementAtIndex(index), GUIContent.none);
        };
    }

    private void StyleSetup()
    {
        if (textAreaStyle == null)
        {
            // Style의 기본 양식은 textArea.
            textAreaStyle = new(EditorStyles.textArea);
            // wordWrap : 문자열이 TextBox 밖으로 못 빠져나가게끔 자동 줄바꿈 처리
            textAreaStyle.wordWrap = true;
        }
    }

    protected bool DrawFoldoutTitle(string text)
        => CustomEditorUtility.DrawFoldoutTitle(isFoldoutExpandedesByTitle, text);
    #endregion

    #region 1-8
    public override void OnInspectorGUI()
    {
        StyleSetup();

        // 객체의 Serialize 변수들의 값을 업데이트함.
        serializedObject.Update();

        // List를 그려줌
        categories.DoLayoutList();

        if(DrawFoldoutTitle("Infomation"))
        {
            // (1) 지금부터 그릴 객체를 가로로 정렬하며, 배경을 테두리 있는 회색으로 채움(=HelpBox는 유니티 내부에 정의되어 있는 Skin임)
            // 중괄호는 작성할 필요는 없지만 명확한 구분을 위해 넣어준 것이기 때문에 스타일에 따라 중괄호는 안넣어도 됨.
            EditorGUILayout.BeginHorizontal("HelpBox");
            {
                //Sprite를 Preview로 볼 수 있게 변수를 그려줌
                iconProperty.objectReferenceValue = EditorGUILayout.ObjectField(GUIContent.none, iconProperty.objectReferenceValue,
                    typeof(Sprite), false, GUILayout.Width(65));

                // (2) 지금부터 그릴 객체는 세로로 정렬한다.
                // 위 icon 변수는 왼쪽에 그려지고, 지금부터 그릴 변수들은 오른쪽에 세로로 그려짐.
                EditorGUILayout.BeginVertical();
                {
                    // (3) 지금부터 그릴 객체는 가로로 정렬한다.
                    // id 변수의 prefix(= inspector에서 보이는 변수의 이름)을 따로 지정해주기 위해 변수 Line을 직접 만듬.
                    EditorGUILayout.BeginHorizontal();
                    {
                        // 변수 편집 Disable, ID는 Database에서 직접 Set해줄 것이기 때문에 사용자가 직접 편집하지 못하도록 함.
                        GUI.enabled = false;
                        // 변수의 선행 명칭(Prefix) 지정
                        EditorGUILayout.PrefixLabel("ID");
                        // id 변수를 그리되 Prefix는 그리지않음(=GUIContent.none); 
                        EditorGUILayout.PropertyField(idProperty, GUIContent.none);
                        // 변수 편집 Enable
                        GUI.enabled = true;
                    }
                    // (3) 가로 정렬 종료
                    EditorGUILayout.EndHorizontal();

                    // 지금부터 변수가 수정되었는지 검사한다.
                    EditorGUI.BeginChangeCheck();
                    var prevCodeName = codeNameProperty.stringValue;
                    // codeName 변수를 그리되, 사용자가 Enter 키를 누를 때까지 값 변경은 보류함.
                    EditorGUILayout.DelayedTextField(codeNameProperty);
                    // 변수가 수정되었는지 확인, codeName 변수가 수정되었다면 수정된 값으로 현재 객체의 이름을 바꿔줌.
                    if (EditorGUI.EndChangeCheck())
                    {
                        // 현재 객체의 유니티 프로젝트상의 주소를 가져옴.
                        // target == IdentifiedObject, var identifiedObject = target as IdentifiecObject 이런 식으로 사용할 수 있음.
                        // serializeObject.targetObject == target
                        var assetPath = AssetDatabase.GetAssetPath(target);
                        // 새로운 이름은 '(변수의 Type)_(codeName)'
                        var newName = $"{target.GetType().Name.ToUpper()}_{codeNameProperty.stringValue}";

                        // Serialize 변수들의 값 변화를 적용함(=디스크에 저장함)
                        // 이 작업을 해주지 않으면 바뀐 값이 적용되지 않아서 이전 값으로 돌아감
                        serializedObject.ApplyModifiedProperties();

                        // 객체의 Project View에서 보이는 이름을 수정함. 만약 같은 이름을 가진 객체가 있을 경우 실패함.
                        var message = AssetDatabase.RenameAsset(assetPath, newName);
                        // 성공했을 경우 객체의 내부 이름도 바꿔줌. 외부 이름과 내부 이름이 다를 시 유니티에서 경고를 띄우고,
                        // 실제 프로젝트에서도 문제를 일으킬 가능성이 높기에 항상 이름을 일치시켜줘야함
                        if (string.IsNullOrEmpty(message))
                            target.name = newName;
                        else
                            codeNameProperty.stringValue = prevCodeName;

                        // 2023.2 버전 및 2022 버전에서 발생하는 Engine Bug 굳이 수정 할 필요는 없음
                        //  Skill System Window에서 제어할 때는 문제가 없어지기 때문에 ㅇㅇ..
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    // displayName 변수를 그려줌
                    EditorGUILayout.PropertyField(displayNameProperty);
                }
                // (2) 세로 정렬 종료
                EditorGUILayout.EndVertical();
            }
            // (1) 가로 정렬 종료
            EditorGUILayout.EndHorizontal();

            // 세로 정렬 시작, 기본적으로 세로 정렬이 Default 정렬이기 때문에 가로 정렬 내부에 사용하는게 아니라면
            // 직접 세로 정렬을 해줄 필요가 없지만 이 경우에는 HelpBox로 내부를 회색으로 채우기위해 직접 세로 정렬을 함
            EditorGUILayout.BeginVertical("HelpBox");
            {
                // Description이라는 Lebel을 띄워줌
                EditorGUILayout.LabelField("Description");
                // TextField를 넓은 형태(TextArea)로 그려줌
                descriptionProperty.stringValue = EditorGUILayout.TextArea(descriptionProperty.stringValue,
                    textAreaStyle, GUILayout.Height(60));
            }
            EditorGUILayout.EndVertical();
            // 세로 정렬 종료
        }

        // Serialize 변수들의 값 변화를 적용함(=디스크에 저장함)
        // 이 작업을 해주지 않으면 바뀐 값이 적용되지 않아서 이전 값으로 돌아감
        serializedObject.ApplyModifiedProperties();
    }
    #endregion
}
