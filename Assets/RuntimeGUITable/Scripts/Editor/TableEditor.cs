using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using System.Linq;

namespace UnityUITable
{

	[CustomEditor(typeof(Table), true)]
	public class TableEditor : Editor
	{

		static readonly Color WARNING_COLOR = new Color(1f, 0.9f, 0.2f);

		SerializedProperty cellContainerPrefab;
		SerializedProperty columnTitlePrefab;
		SerializedProperty columns;
		SerializedProperty rowsColorMode;
		SerializedProperty bgColor;
		SerializedProperty altBgColor;
		SerializedProperty lineColor;
		SerializedProperty titleBGColor;
		SerializedProperty titleFontColor;
		SerializedProperty titleFontSize;
		SerializedProperty titleFont;
		SerializedProperty titleFontStyle;
		SerializedProperty rowHeight;
		SerializedProperty spacing;
		SerializedProperty targetCollection;
		SerializedProperty outlineSprite;
		SerializedProperty rowDeleteButtons;
		SerializedProperty deleteColumnWidth;
		SerializedProperty deleteCellStyle;
		SerializedProperty rowAddButton;
		SerializedProperty addCellStyle;
		SerializedProperty selectableRows;
		SerializedProperty selectedBgColor;
		SerializedProperty updateCellStyleAtRuntime;
		SerializedProperty updateCellContentAtRuntime;

		Table table;
		ReorderableList reorderableList;

		bool advancedExpanded;

		GUIStyle warningstyle;


		void OnEnable()
		{
			table = (Table)target;
			cellContainerPrefab = 			serializedObject.FindProperty("cellContainerPrefab");
			columnTitlePrefab = 			serializedObject.FindProperty("columnTitlePrefab");
			columns = 						serializedObject.FindProperty("columns");
			rowsColorMode = 				serializedObject.FindProperty("rowsColorMode");
			bgColor = 						serializedObject.FindProperty("bgColor");
			altBgColor = 					serializedObject.FindProperty("altBgColor");
			lineColor = 					serializedObject.FindProperty("lineColor");
			titleBGColor = 					serializedObject.FindProperty("titleBGColor");
			titleFontColor = 				serializedObject.FindProperty("titleFontColor");
			titleFontSize = 				serializedObject.FindProperty("titleFontSize");
			titleFont = 					serializedObject.FindProperty("titleFont");
			titleFontStyle = 				serializedObject.FindProperty("titleFontStyle");
			rowHeight = 					serializedObject.FindProperty("rowHeight");
			spacing = 						serializedObject.FindProperty("spacing");
			targetCollection = 				serializedObject.FindProperty("targetCollection");
			outlineSprite = 				serializedObject.FindProperty("outlineSprite");
			rowDeleteButtons = 				serializedObject.FindProperty("rowDeleteButtons");
			deleteColumnWidth = 			serializedObject.FindProperty("deleteColumnWidth");
			deleteCellStyle = 				serializedObject.FindProperty("deleteCellStyle");
			rowAddButton = 					serializedObject.FindProperty("rowAddButton");
			addCellStyle = 					serializedObject.FindProperty("addCellStyle");
			selectableRows = 				serializedObject.FindProperty("selectableRows");
			selectedBgColor = 				serializedObject.FindProperty("selectedBgColor");
			updateCellStyleAtRuntime = 		serializedObject.FindProperty("updateCellStyleAtRuntime");
			updateCellContentAtRuntime = 	serializedObject.FindProperty("updateCellContentAtRuntime");


			reorderableList = new ReorderableList(serializedObject, columns);
			reorderableList.drawHeaderCallback = rect =>
			{
				bool changed = GUI.changed;
				columns.isExpanded = EditorGUI.Foldout(new Rect(rect.x + 10f, rect.y, rect.width, rect.height), columns.isExpanded, "Columns:");
				GUI.changed = changed;
			};

			reorderableList.drawElementCallback =
				(Rect rect, int index, bool isActive, bool isFocused) =>
				{
					if (!columns.isExpanded)
					{
						GUI.enabled = false;
						return;
					}
					SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
					EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
				};
			reorderableList.elementHeightCallback = (index) => (!columns.isExpanded) ? 0 : EditorGUI.GetPropertyHeight(reorderableList.serializedProperty.GetArrayElementAtIndex(index));
			reorderableList.onAddCallback = (list) =>
			{
				list.serializedProperty.arraySize++;
				InitNewColumnInfo(list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1), table);
				EditorUtility.SetDirty(target);
			};
			reorderableList.onReorderCallback = (list) => table.SetDirty();
		}

		void Awake()
		{
			warningstyle = new GUIStyle(EditorStyles.label);
			warningstyle.normal.textColor = WARNING_COLOR;
		}

		static void InitNewColumnInfo(SerializedProperty columnInfoSP, Table table)
		{
			columnInfoSP.FindPropertyRelative("fieldName").stringValue = "";
			columnInfoSP.FindPropertyRelative("width").floatValue = 0f;
			columnInfoSP.FindPropertyRelative("useRelativeWidth").boolValue = true;
			columnInfoSP.FindPropertyRelative("autoWidth").boolValue = true;
			columnInfoSP.FindPropertyRelative("autoColumnTitle").boolValue = true;
			columnInfoSP.FindPropertyRelative("table").objectReferenceValue = table;
			columnInfoSP.FindPropertyRelative("cellPrefab").objectReferenceValue = null;
			columnInfoSP.FindPropertyRelative("cellStyle").objectReferenceValue = null;
		}

		void DrawWarning(string message)
		{
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(EditorGUIUtility.IconContent("Collab.Warning"), GUILayout.Width(20f));
			EditorGUILayout.LabelField(new GUIContent(message), warningstyle);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.Space();
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.LabelField("Data", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Collection:");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(targetCollection);
			if (EditorGUI.EndChangeCheck())
			{
				if (columns.arraySize > 0 && EditorUtility.DisplayDialog("You changed the target collection", "Your columns will likely become invalid.\nClear the selected columns?", "Clear", "Do not clear"))
					columns.ClearArray();
			}
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			if (table.targetCollection.IsDefined)
			{
				reorderableList.DoLayoutList();
				GUI.enabled = true;
			}

			List<TableColumnInfo> tableColumnInfos = (List<TableColumnInfo>)columns.GetTargetObjectOfProperty();
			if (tableColumnInfos.Count > 0 && tableColumnInfos.All(tci => !tci.autoWidth))
			{
				DrawWarning("No auto-width column.");
			}
			if (tableColumnInfos.Count > 0 && tableColumnInfos.Sum(tci => tci.autoWidth ? 0f : tci.AbsoluteWidth) > table.GetAvailableWidth())
			{
				DrawWarning("Columns don't fit in table's RectTransform.");
			}

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(rowDeleteButtons, new GUIContent("\"Delete\" Buttons"));
			if (rowDeleteButtons.boolValue)
			{
				EditorGUIUtility.labelWidth = EditorGUIUtility.fieldWidth = 40f;
				EditorGUILayout.PropertyField(deleteColumnWidth, new GUIContent("Width"), GUILayout.Width(80f));
				EditorGUIUtility.labelWidth = EditorGUIUtility.fieldWidth = -1f;
				TableColumnInfoDrawer.DrawCellStyleButton(
					EditorGUILayout.GetControlRect(false, GUILayout.Width(20f)),
					deleteCellStyle,
					table.deleteCellPrefab,
					string.Format("{0}_Delete_Button_Style", serializedObject.targetObject.name),
					table,
					table.deleteCellStyleTemplate);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PropertyField(rowAddButton, new GUIContent("\"Add\" Button"));
			if (rowAddButton.boolValue)
			{
				TableColumnInfoDrawer.DrawCellStyleButton(
					EditorGUILayout.GetControlRect(false, GUILayout.Width(20f)),
					addCellStyle,
					table.addCellPrefab,
					string.Format("{0}_Add_Button_Style", serializedObject.targetObject.name),
					table,
					table.addCellStyleTemplate);
			}
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Appearance", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(rowsColorMode, new GUIContent("Row Colors"));
			EditorGUI.indentLevel++;
			if (table.rowsColorMode == Table.RowsColorMode.Plain)
				EditorGUILayout.PropertyField(bgColor, new GUIContent("Color"));
			else
			{
				EditorGUILayout.PropertyField(bgColor, new GUIContent("Odd Rows"));
				EditorGUILayout.PropertyField(altBgColor, new GUIContent("Even Rows"));
			}
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();
			EditorGUILayout.PropertyField(selectableRows, new GUIContent("Selectable Lines"));
			if (selectableRows.boolValue)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(selectedBgColor, new GUIContent("Selected Color"));
				EditorGUI.indentLevel--;
			}
			EditorGUILayout.Space();
			bool oldOutline = lineColor.colorValue != Color.clear;
			bool newOutline = EditorGUILayout.Toggle(new GUIContent("Outline"), oldOutline);
			if (oldOutline && !newOutline)
			{
				lineColor.colorValue = Color.clear;
				spacing.floatValue = 0f;
			}
			else if (!oldOutline && newOutline)
			{
				lineColor.colorValue = Color.black;
				spacing.floatValue = -1f;
			}
			if (newOutline)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(lineColor, new GUIContent("Color"));
				EditorGUILayout.PropertyField(spacing);
				EditorGUI.indentLevel--;
				EditorGUILayout.Space();
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Headers");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(titleBGColor, new GUIContent("BG Color"));
			EditorGUILayout.PropertyField(titleFont, new GUIContent("Font"));
			EditorGUILayout.PropertyField(titleFontStyle, new GUIContent("Font Style"));
			EditorGUILayout.PropertyField(titleFontSize, new GUIContent("Font Size"));
			EditorGUILayout.PropertyField(titleFontColor, new GUIContent("Font Color"));
			EditorGUI.indentLevel--;
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(rowHeight);
			bool isScrollable = table.IsScrollable;
			if (EditorGUILayout.Toggle("Scrollable", isScrollable) != isScrollable)
			{
				table.MakeScrollable(!isScrollable);
			}
			EditorGUILayout.Space();

			advancedExpanded = EditorGUILayout.Foldout(advancedExpanded, "Advanced Settings");
			if (advancedExpanded)
			{
				EditorGUI.indentLevel++;
				EditorGUILayout.PropertyField(cellContainerPrefab);
				EditorGUILayout.PropertyField(columnTitlePrefab);
				EditorGUILayout.PropertyField(outlineSprite);
				EditorGUI.indentLevel--;
			}

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("Update at Runtime:");
			EditorGUI.indentLevel++;
			EditorGUILayout.PropertyField(updateCellStyleAtRuntime, new GUIContent("Cell Style", "Updates the cells style at runtime. Enable if you plan to modify these styles at runtime."));
			EditorGUILayout.PropertyField(updateCellContentAtRuntime, new GUIContent("Cell Content", "Updates the cells content at runtime from the target collection. Enable if the collection's elements might be modified outside of the table."));
			EditorGUI.indentLevel--;
			EditorGUI.indentLevel--;
			if (EditorGUI.EndChangeCheck())
				table.SetDirty();
			serializedObject.ApplyModifiedProperties();
		}

	}

}
