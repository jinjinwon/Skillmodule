using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using static UnityEditor.LightingExplorerTableColumn;
using System.Diagnostics;
using Unity.VisualScripting;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using System.IO;
using System.ComponentModel.Composition.Primitives;

public class GoogleSheetEditor : EditorWindow
{
    #region 3-2

    // static ���� ����Ǿ� �ִ� ���� : â�� ���� �ѵ� ������ �����ϰ� �ְ� �ϱ� ����

    // ���� ���� �ִ� database�� index
    private static int toolbarIndex = 0;
    // Database List�� Scroll Position
    private static Dictionary<Type, Vector2> scrollPositionsByType = new();
    // ���� �����ְ� �ִ� data�� Scroll Posiiton
    private static Vector2 drawingEditorScrollPosition;
    // ���� ������ Data
    private static Dictionary<Type, IdentifiedObject> selectedObjectsByType = new();

    // Type�� Database(Category, Stat, Skill ���...)
    private readonly Dictionary<Type, IODatabase> databasesByType = new();
    // Database Data���� Type��
    private Type[] databaseTypes;
    // �� Type���� string �̸�
    private string[] databaseTypeNames;

    // ���� �����ְ� �ִ� data�� Editor class
    private Editor cachedEditor;

    // Database List�� Selected Background Texture
    private Texture2D selectedBoxTexture;
    // Database List�� Selected Style
    private GUIStyle selectedBoxStyle;

    // ����Ʈ�� ���� ��
    private readonly float listHeight = 40f;

    // ȭ�鿡 ���̴� ����Ʈ���� ���� ��
    private float visibleTotalHeight = 0f;
    #endregion

    #region 3-3
    // Editor Tools �ǿ� Skill System �׸��� �߰��ǰ�, Click�� Window�� ����
    [MenuItem("Tools/Google Sheat System")]
    private static void OpenWindow()
    {
        // Skill System�̶� ��Ī�� ���� Window�� ����
        var window = GetWindow<GoogleSheetEditor>("Google Sheat System");
        // Window�� �ּ� ������� 800x700
        window.minSize = new Vector2(800, 700);
        // Window�� ������
        window.Show();
    }

    private void SetupStyle()
    {
        // 1x1 Pixel�� Texture�� ����
        selectedBoxTexture = new Texture2D(1, 1);
        // Pixel�� Color(=û��)�� ��������
        selectedBoxTexture.SetPixel(0, 0, new UnityEngine.Color(0.31f, 0.40f, 0.50f));
        // ������ ������ Color���� ������ ������
        selectedBoxTexture.Apply();
        // �� Texture�� Window���� ������ ���̱� ������ Unity���� �ڵ� ������������(DontSave) Flag�� ��������
        // �� flag�� ���ٸ� Editor���� Play�� ����ä�� SetupStyle �Լ��� ����Ǹ�
        // texture�� Play ���¿� ���ӵǾ� Play�� �����ϸ� texture�� �ڵ� Destroy�ǹ���
        selectedBoxTexture.hideFlags = HideFlags.DontSave;

        selectedBoxStyle = new GUIStyle();
        // Normal ������ Backgorund Texture�� �� Texture�� �����������ν� �� Style�� ���� GUI�� Background�� û������ ���� ����
        // ��, Select�� Data�� Background�� û������ ���ͼ� ������
        selectedBoxStyle.normal.background = selectedBoxTexture;
    }

    private void SetupDatabases(Type[] dataTypes)
    {
        if (databasesByType.Count == 0)
        {
            // Resources Folder�� Database Folder�� �ִ��� Ȯ��
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Database"))
            {
                // ���ٸ� Database Folder�� �������
                AssetDatabase.CreateFolder("Assets/Resources", "Database");
            }

            foreach (var type in dataTypes)
            {
                var database = AssetDatabase.LoadAssetAtPath<IODatabase>($"Assets/Resources/Database/{type.Name}Database.asset");
                if (database == null)
                {
                    database = CreateInstance<IODatabase>();
                    // ������ �ּҿ� IODatabase�� ����
                    AssetDatabase.CreateAsset(database, $"Assets/Resources/Database/{type.Name}Database.asset");
                    // ������ �ּ��� ���� Folder�� ����, �� Folder�� Window�� ���� ������ IdentifiedObject�� ����� �����
                    AssetDatabase.CreateFolder("Assets/Resources", type.Name);
                }

                // �ҷ��� or ������ Database�� Dictionary�� ����
                databasesByType[type] = database;
                // ScrollPosition Data ����
                scrollPositionsByType[type] = Vector2.zero;
                // SelectedObject Data ����
                selectedObjectsByType[type] = null;
            }

            databaseTypeNames = dataTypes.Select(x => x.Name).ToArray();
            databaseTypes = dataTypes;
        }
    }
    #endregion

    #region 3-5
    private void OnEnable()
    {
        SetupStyle();

        // ����Ʈ�� ������ ���Ⱑ �ָ��Ͽ� �����Ͽ����ϴ�..
        SetupDatabases(new[] { typeof(Category), typeof(Stat) });
    }

    private void OnDisable()
    {
        DestroyImmediate(cachedEditor);
        DestroyImmediate(selectedBoxTexture);
    }

    private void OnGUI()
    {
        // Database���� ���� ���� IdentifiedObject���� Type Name���� Toolbar�� �׷���
        toolbarIndex = GUILayout.Toolbar(toolbarIndex, databaseTypeNames);
        EditorGUILayout.Space(4f);
        CustomEditorUtility.DrawUnderline();
        EditorGUILayout.Space(4f);

        DrawDatabase(databaseTypes[toolbarIndex]);
    }
    #endregion

    #region 3-4
    private void DrawDatabase(Type dataType)
    {
        // Dictionary���� Type�� �´� Database�� ã�ƿ�
        var database = databasesByType[dataType];
        // Editor�� Caching�Ǵ� Preview Texture�� ���� �ּ� 32��, �ִ� database�� Count���� �ø�
        // �� �۾��� �����ָ� �׷����ϴ� IO ��ü�� Icon���� ���� ��� ����� �׷����� �ʴ� ������ �߻���
        AssetPreview.SetPreviewTextureCacheSize(Mathf.Max(32, database.Count));

        // Database�� Data ����� �׷��ֱ� ����
        // (1) ���� ���� ����
        EditorGUILayout.BeginHorizontal();
        {
            // (2) ���� ���� ����, Style�� HelpBox, ���̴� 300f
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(300f));
            {
                // ���ݺ��� �׸� GUI�� �ʷϻ�
                GUI.color = UnityEngine.Color.green;
                // ���ο� Data�� ����� Button�� �׷���
                if (GUILayout.Button($"Export Google Sheet"))
                {
                    GoogleSheetExport.Export(dataType.Name,database);
                }

                // ���ݺ��� �׸� GUI�� ������
                GUI.color = UnityEngine.Color.yellow;
                // ������ ������ Data�� �����ϴ� Button�� �׷���
                if (GUILayout.Button($"Import Google Sheet"))
                {
                    GoogleSheetImport.Import(dataType, database);
                    EditorUtility.SetDirty(database);
                    // Dirty flag ����� ������
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                // ���ݺ��� �׸� GUI�� �Ͼ��(=������)
                GUI.color = UnityEngine.Color.white;

                EditorGUILayout.Space(2f);
                CustomEditorUtility.DrawUnderline();
                EditorGUILayout.Space(4f);

                // ���ݺ��� Scroll ������ Box�� �׸�, UI �� ScrollView�� ������
                // ù��° ���ڴ� ���� Scroll Posiiton
                // �ι�° ���ڴ� ���� Scroll ���븦 �׸� ���ΰ�?, ����° ���ڴ� �׻� ���� Scroll ���븦 �׸� ���ΰ�?
                // �׹�° ���ڴ� �׻� ���� Scroll ������ Style, �ټ���° ���ڴ� ���� Scroll ������ Style
                // none�� �Ѱ��ְԵǸ� �ش� ����� �ƿ� ���ֹ���
                // ������° ���ڴ� Background Style
                // return ���� ������� ���ۿ� ���� �ٲ�Ե� Scroll Posiiton
                // ScrollView�� ũ��� ������ BeginVertical �Լ��� ���� ���� 300�� ������
                // BeginScrollView�� ���� Overloading�� �ֱ� ������ �׳� ���� Scroll Position�� �־ ScrollView�� �������
                // ���⼭�� ���� ���븦 ���� �������� ���ڰ� ���� �Լ��� ��

                scrollPositionsByType[dataType] = EditorGUILayout.BeginScrollView(scrollPositionsByType[dataType], false, true,
                    GUIStyle.none, GUI.skin.verticalScrollbar, GUIStyle.none);
                {
                    // Database�� ����� �׸�
                    foreach (var data in database.Datas)
                    {
                        // CodeName�� �׷��� ������ ����, ���� Icon�� �����Ѵٸ� Icon�� ũ�⸦ ����ϸ� ���� ���̸� ����
                        float labelWidth = data.Icon != null ? 200f : 245f;

                        // ���� Data�� ������ ������ Data�� selectedBoxStyle(=����� û��)�� ������
                        var style = selectedObjectsByType[dataType] == data ? selectedBoxStyle : GUIStyle.none;
                        // (3) ���� ���� ����
                        EditorGUILayout.BeginHorizontal(style, GUILayout.Height(listHeight));
                        {
                            // Data�� Icon�� �ִٸ� 40x40 ������� �׷���
                            if (data.Icon)
                            {
                                // Icon�� Preview Texture�� ������.
                                // �ѹ� ������ Texture�� Unity ���ο� Caching�Ǹ�, 
                                // Cache�� Texture ���� ������ ������ TextureCacheSize�� �����ϸ� ������ Texture���� ������
                                var preview = AssetPreview.GetAssetPreview(data.Icon);
                                GUILayout.Label(preview, GUILayout.Height(40f), GUILayout.Width(40f));
                            }
                            // Data�� CodeName�� �׷���
                            EditorGUILayout.LabelField(data.CodeName, GUILayout.Width(labelWidth), GUILayout.Height(40f));
                            // (4) ���� ���� ����, �̰� �׷��� Labe�� �߾� ������ �ϱ� ���ؼ���
                            GUI.color = UnityEngine.Color.white;
                        }
                        // (3) ���� ���� D����
                        EditorGUILayout.EndHorizontal();

                        // data�� �����Ǿ��ٸ� ��� Database ����� �׸��°� ���߰� ��������
                        if (data == null)
                            break;

                        // ���������� �׸� GUI�� ��ǥ�� ũ�⸦ ������
                        // �� ��� �ٷ� ���� �׸� GUI�� ��ǥ�� ��������(=BeginHorizontal)
                        var lastRect = GUILayoutUtility.GetLastRect();

                        // MosueDown Event�� mosuePosition�� GUI�ȿ� �ִٸ�(=Click) Data�� ������ ������ ó����
                        if (Event.current.type == EventType.MouseDown && lastRect.Contains(Event.current.mousePosition))
                        {
                            selectedObjectsByType[dataType] = data;
                            drawingEditorScrollPosition = Vector2.zero;
                            // Event�� ���� ó���� �ߴٰ� Unity�� �˸�
                            Event.current.Use();
                        }

                        // ���õ� �����̸� Ű �ٿ��� ������ ���
                        if (Event.current.type == EventType.KeyDown && selectedObjectsByType[dataType] != null)
                        {
                            IdentifiedObject tempData = null;
                            int index = database.IndexOf(selectedObjectsByType[dataType]);

                            if (Event.current.keyCode == KeyCode.DownArrow) tempData = database.GetNextData(index);
                            else if (Event.current.keyCode == KeyCode.UpArrow) tempData = database.GetPrevData(index);
                            drawingEditorScrollPosition = Vector2.zero;

                            if (tempData != null)
                            {
                                selectedObjectsByType[dataType] = tempData;

                                #region ��ġ�� ���� ��ũ�� �� ��ġ ��ȭ

                                // ��ũ�� ��ġ �ʱ�ȭ
                                scrollPositionsByType[dataType] = CalculateNewScrollPosition(scrollPositionsByType[dataType], database.Datas, selectedObjectsByType[dataType], (visibleTotalHeight - 30));
                                #endregion

                                // �̺�Ʈ ó���� �Ϸ��ߴٰ� �˸�
                                Event.current.Use();
                            }
                        }
                    }
                }
                // ScrollView ����
                EditorGUILayout.EndScrollView();
                if (GUILayoutUtility.GetLastRect().height > 1)
                {
                    visibleTotalHeight = GUILayoutUtility.GetLastRect().height;
                }
            }
            // (2) ���� ���� ����
            EditorGUILayout.EndVertical();

            // ���õ� Data�� �����Ѵٸ� �ش� Data�� Editor�� �׷���
            if (selectedObjectsByType[dataType])
            {
                // ScrollView�� �׸�, �̹����� Scroll Position ������ �Ѱ��༭ ����, ���� ���� �� �ִ� �Ϲ����� ScrollView�� �׸�
                // ��, always �ɼ��� �����Ƿ� ����, ���� ����� Scroll�� ������ ������ ���� ��Ÿ��
                drawingEditorScrollPosition = EditorGUILayout.BeginScrollView(drawingEditorScrollPosition);
                {
                    EditorGUILayout.Space(2f);
                    // ù��° ���ڴ� Editor�� ���� Target
                    // �ι�° ���ڴ� Target�� Type
                    // ���� �־����� ������ Target�� �⺻ Type�� �����
                    // ����°�� ���ο��� ������� Editor�� ���� Editor ����
                    // CreateCachedEditor�� ���ο��� �������� Editor�� cachedEditor�� ���ٸ�
                    // Editor�� ���� ������ �ʰ� �׳� cachedEditor�� �״�� ��ȯ��
                    // ���� ���ο��� �������� Editor�� cachedEditor�� �ٸ��ٸ�
                    // cachedEditor�� Destroy�ϰ� ���� ���� Editor�� ����
                    Editor.CreateCachedEditor(selectedObjectsByType[dataType], null, ref cachedEditor);
                    // Editor�� �׷���
                    cachedEditor.OnInspectorGUI();
                }
                // ScrollView ����
                EditorGUILayout.EndScrollView();
            }
        }
        // (1) ���� ���� ����
        EditorGUILayout.EndHorizontal();
    }
    #endregion

    #region ������ �������� ��ġ�� ���� ��ũ�� �� �̵�
    private Vector2 CalculateNewScrollPosition(Vector2 currentScrollPosition, IReadOnlyList<IdentifiedObject> datas, IdentifiedObject selectedData, float viewHeight)
    {
        float currentYPosition = 0;
        int index = 0;
        float dataHeight = 0.0f;
        float endPosition = 0.0f;
        float newScrollPosition = 0.0f;

        // �ϴ����� ������������ �̹� ���̴� (int)(visibleTotalHeight / listHeight)���� ���� �ε��� ��ŭ ���ش�.
        // ������� �ö󰥶����� �̹� �ϴܿ� (int)(visibleTotalHeight / listHeight)���� ���̹Ƿ� �ε����� ������ �ʴ´�.

        int count = (int)(visibleTotalHeight / listHeight);

        foreach (var data in datas)
        {
            index++;
            dataHeight = GetDataHeight(data); // ������ ���� ���
            if (data == selectedData)
            {
                #region ������
                //// ���õ� �������� ����� ��ũ�� �� �Ʒ��� �ִ� ���
                //if (currentYPosition < currentScrollPosition.y)
                //{
                //    return new Vector2(currentScrollPosition.x, currentYPosition);
                //}
                //// ���õ� �������� �ϴ��� ��ũ�� �� ���� �ִ� ���
                //else if (currentYPosition + dataHeight > currentScrollPosition.y + viewHeight)
                //{
                //    UnityEngine.Debug.Log($"currentYPosition({currentYPosition}) : dataHeight({dataHeight}) : currentScrollPosition.y({currentScrollPosition.y})");
                //    return new Vector2(currentScrollPosition.x, currentYPosition + dataHeight - viewHeight);
                //}
                //else
                //{

                //}
                #endregion
                endPosition = currentYPosition + dataHeight;
                // ���õ� �����Ͱ� ��ũ�� ���� �ϴ� ��躸�� �Ʒ��� �ִ� ���
                if (endPosition + (index - count) * 2 > currentScrollPosition.y + viewHeight)
                {
                    newScrollPosition = endPosition - viewHeight; // �������� �ϴ��� ��ũ�� ���� �ϴܰ� ��ġ�ϵ��� ����

                    if (index > count)
                        newScrollPosition += (index - count) * 2;

                    // �� ��ũ�� ��ġ�� ���� ��ġ���� ū ��츸 ������Ʈ (���ʿ��� ��ũ�� ��ġ �̵� ����)
                    if (newScrollPosition > currentScrollPosition.y)
                    {
                        return new Vector2(currentScrollPosition.x, newScrollPosition);
                    }
                }
                // ���õ� �����Ͱ� ��ũ�� ���� ��� ��躸�� ���� �ִ� ���
                else if (currentYPosition + index * 2 < currentScrollPosition.y)
                {
                    currentYPosition += index * 2;
                    return new Vector2(currentScrollPosition.x, currentYPosition);
                }
                break; // ���õ� �����Ϳ� ���� ó���� �Ϸ�Ǹ� �ݺ� ����
            }
            currentYPosition += dataHeight;
        }

        return currentScrollPosition; // ������ ���� ��� ���� ��ũ�� ��ġ ��ȯ
    }

    private float GetDataHeight(IdentifiedObject data)
    {
        return listHeight; // ��ũ���� ������ �� ����Ʈ�������� ���̸� 40���� ����������
    }
    #endregion
}

// Google Sheet Export
public class GoogleSheetExport
{
    public readonly static string[] Category_Category = { "ID", "CodeName", "DisPlayName", "Description", "SpritePath" };
    public readonly static string[] Category_Stat  = { "ID", "CodeName", "DisPlayName", "Description", "SpritePath","PercentType", "MaxValue", "MinValue", "DefaultValue"};

    static readonly string[] Scopes = { DriveService.Scope.DriveReadonly, SheetsService.Scope.Spreadsheets };
    static readonly string ApplicationName = "SkillModule";

    // �ڰ� ������ �˻��մϴ�.
    private static GoogleCredential GetCredential()
    {
        using (var stream = new FileStream("Assets/Credential/skillmodule-d9f09e9147c2.json", FileMode.Open, FileAccess.Read))
        {
            return GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }
    }

    // Google Drive���� Ư�� �̸��� ���������Ʈ�� �˻��Ͽ� ���������Ʈ ID�� ��ȯ�մϴ�.
    private static string GetSpreadsheetId(GoogleCredential credential)
    {
        // Google Drive API �ʱ�ȭ
        var driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // ���������Ʈ ���� �˻�
        var request = driveService.Files.List();
        request.Q = $"mimeType='application/vnd.google-apps.spreadsheet' and name='{ApplicationName}'";
        var result = request.Execute();
        var file = result.Files.FirstOrDefault();

        if (file == null)
        {
            UnityEngine.Debug.LogError("Spreadsheet not found.");
            return "";
        }

        return file.Id;
    }

    // Google Sheets API�� ����Ͽ� ���������Ʈ �����͸� �о�ɴϴ�.
    private static SheetsService CheckAndClearSheetAPI(GoogleCredential credential,string name, string spreadsheetid)
    {
        var sheetsService = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Define the request parameters.
        var range_clear = $"{name}!A1:Z1000"; // �ʱ�ȭ�� ���� (���÷� A1:Z1000)
        ClearValuesRequest requestBody = new ClearValuesRequest();

        // Clear values.
        SpreadsheetsResource.ValuesResource.ClearRequest request_clear = sheetsService.Spreadsheets.Values.Clear(requestBody, spreadsheetid, range_clear);

        try
        {
            request_clear.Execute();
            Console.WriteLine("Sheet cleared successfully.");
            return sheetsService;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error clearing sheet: " + e.Message);
            return sheetsService;
        }
    }

    // �� �Լ��� ���� ������ ���� �����͸� �������� ��Ʈ�� �������ϴ�.
    public static void Export(string name, IODatabase Data)
    {
        GoogleCredential credential;
        SheetsService sheetsService;

        // ���⿡ ��ũ���ͺ� ������Ʈ �����͸� ��ȯ�ϴ� �ڵ带 �߰��մϴ�.
        // ���÷� �����͸� �߰��մϴ�.

        credential = GetCredential();

        var spreadsheetId = GetSpreadsheetId(credential);

        sheetsService = CheckAndClearSheetAPI(credential, name,spreadsheetId);

        var values = new List<IList<object>>(); // ������ ����Ʈ

        if (name.CompareTo("Stat") == 0)
        {
            List<Stat> stats = new();

            values.Add(Category_Stat.Cast<object>().ToList());
            stats = Data.Datas.OfType<Stat>().ToList();

            foreach (var stat in stats)
            {
                string spritePath = AssetDatabase.GetAssetPath(stat.Icon);

                if (spritePath.CompareTo("") == 0)
                    spritePath = "EMPTY";

                values.Add(new List<object> { stat.ID, stat.CodeName, stat.DisplayName, stat.Description, spritePath, stat.IsPercentType, stat.MaxValue, stat.MinValue, stat.DefaultValue });
            }
        }
        else
        {
            List<Stat> stats = new();

            values.Add(Category_Category.Cast<object>().ToList());
            foreach (var category in Data.Datas)
            {
                string spritePath = AssetDatabase.GetAssetPath(category.Icon);

                if (spritePath.CompareTo("") == 0)
                    spritePath = "EMPTY";

                values.Add(new List<object> { category.ID, category.CodeName, category.DisplayName, category.Description, spritePath });
            }
        }

        var range = $"{name}!A1";
        var valueRange = new ValueRange { Values = values };

        var updateRequest = sheetsService.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
        updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        var updateResponse = updateRequest.Execute();

        UnityEngine.Debug.Log($"{name} exported successfully!");
    }
}

// Google Sheet Import
public class GoogleSheetImport
{
    static readonly string[] Scopes = { DriveService.Scope.DriveReadonly, SheetsService.Scope.SpreadsheetsReadonly };
    static readonly string ApplicationName = "SkillModule";

    // Google Drive���� Ư�� �̸��� ���������Ʈ�� �˻��Ͽ� ���������Ʈ ID�� ��ȯ�մϴ�.
    private static string GetSpreadsheetId(string spreadsheetName)
    {
        GoogleCredential credential;
        using (var stream = new FileStream("Assets/Credential/skillmodule-d9f09e9147c2.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        var driveService = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        // Google Drive���� ���������Ʈ ���� �˻�
        var request = driveService.Files.List();
        request.Q = $"mimeType='application/vnd.google-apps.spreadsheet' and name='{spreadsheetName}'";
        request.Fields = "files(id, name)";
        var result = request.Execute();
        var file = result.Files.FirstOrDefault();

        if (file == null)
        {
            UnityEngine.Debug.LogError($"Spreadsheet named '{spreadsheetName}' not found.");
            return null;
        }

        return file.Id;
    }

    // Google Sheets API�� ����Ͽ� ���������Ʈ �����͸� �о�ɴϴ�.
    private static List<IList<object>> ReadSheet(string spreadsheetId, string sheetName)
    {
        GoogleCredential credential;
        using (var stream = new FileStream("Assets/Credential/skillmodule-d9f09e9147c2.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        var service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });

        var range = $"{sheetName}!A1:Z"; // �а��� �ϴ� ���� ����
        var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
        var response = request.Execute();
        var values = response.Values;

        if (values == null || values.Count == 0)
        {
            UnityEngine.Debug.LogError("No data found.");
            return null;
        }

        return (List<IList<object>>)values;
    }

    // �� �Լ��� ���� ������ ���� ScriptableObject�� �����Ͽ� Database�� �־��ݴϴ�.
    public static void Import(Type name, IODatabase data)
    {
        // ���������Ʈ ID ��������
        string spreadsheetId = GetSpreadsheetId(ApplicationName);
        if (spreadsheetId == null)
        {
            UnityEngine.Debug.LogError("Failed to get spreadsheet ID.");
            return;
        }

        // ���������Ʈ ������ �о����
        List<IList<object>> sheetData = ReadSheet(spreadsheetId, name.Name);
        if (sheetData == null)
        {
            UnityEngine.Debug.LogError("Failed to read sheet data.");
            return;
        }

        // ������ �ʱ�ȭ
        data.Clear();

        // ù ��° ���� ����̹Ƿ� �ǳʶݴϴ�.
        for (int i = 1; i < sheetData.Count; i++)
        {
            var row = sheetData[i];
            string codeName = row[1].ToString();

            if (name.Name.CompareTo("Stat") == 0)
            {
                var stat = Resources.Load<Stat>($"{name}/{name.ToString().ToUpper()}_{codeName}");

                bool isNew = false;

                if (stat == null)
                {
                    // ScriptableObject�� ������ ���� ����
                    stat = ScriptableObject.CreateInstance<Stat>();
                    isNew = true;
                }

                // ScriptableObject �� ������Ʈ
                stat.SetID_(int.Parse(row[0].ToString()));
                stat.SetCodeName_(row[1].ToString());
                stat.SetDisplayName_(row[2].ToString());
                stat.SetDescription_(row[3].ToString());
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(row[4].ToString());
                stat.SetIcon_(sprite);
                stat.SetIsPercentType(bool.Parse(row[5].ToString()));
                stat.MaxValue = float.Parse(row[6].ToString());
                stat.MinValue = float.Parse(row[7].ToString());
                stat.DefaultValue = float.Parse(row[8].ToString());

                // ScriptableObject�� �������� �����մϴ�.
                if (isNew == true)
                {
                    string assetPath = $"Assets/Resources/{name}/{name.ToString().ToUpper()}_{codeName}.asset";
                    AssetDatabase.CreateAsset(stat, assetPath);
                }
                else
                {
                    EditorUtility.SetDirty(stat);
                }
                data.Add_Sheet(stat);
            }
            else
            {
                var category = Resources.Load<Category>($"{name}/{name.ToString().ToUpper()}_{codeName}");

                bool isNew = false;

                if (category == null)
                {
                    // ScriptableObject�� ������ ���� ����
                    category = ScriptableObject.CreateInstance<Category>();
                    isNew = true;
                }

                // ScriptableObject �� ������Ʈ
                category.SetID_(int.Parse(row[0].ToString()));
                category.SetCodeName_(row[1].ToString());
                category.SetDisplayName_(row[2].ToString());
                category.SetDescription_(row[3].ToString());
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(row[4].ToString());
                category.SetIcon_(sprite);

                // ScriptableObject�� �������� �����մϴ�.
                if (isNew == true)
                {
                    string assetPath = $"Assets/Resources/{name}/{name.ToString().ToUpper()}_{codeName}.asset";
                    AssetDatabase.CreateAsset(category, assetPath);
                }
                else
                {
                    EditorUtility.SetDirty(category);
                }
                data.Add_Sheet(category);
            }
        }

        // ���� �����ͺ��̽��� �����ϰ� �����մϴ�.
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        UnityEngine.Debug.Log($"{name} imported successfully!");
    }
}
