using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityUITable
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(HorizontalLayoutGroup))]
	public class Table : MonoBehaviour
	{

		bool isDirty;

		public void SetDirty()
		{
			isDirty = true;
		}

		public static bool IsCollection(System.Type type)
		{
			if (type == null || type == typeof(string) || type == typeof(Transform))
				return false;
			return typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType;
		}

		static bool IsCollection(MemberInfo memberInfo)
		{
			return memberInfo.MemberType == MemberTypes.Field && IsCollection(((FieldInfo)memberInfo).FieldType)
				|| memberInfo.MemberType == MemberTypes.Property && IsCollection(((PropertyInfo)memberInfo).PropertyType);
		}

		public static readonly Color DARK_BLUE = new Color(0f, 0f, 0.5f);
		public static readonly Color DARK_GRAY = new Color(0.4f, 0.4f, 0.4f);
		public static readonly Color SELECTION_BLUE = new Color(0.2f, 0.4f, 0.5f);

		public TableCellContainer cellContainerPrefab;
		public CellContainer emptyCellContainerPrefab;

		public HeaderCellContainer columnTitlePrefab;

		public List<TableColumnInfo> columns = new List<TableColumnInfo>();

		public bool rowDeleteButtons;

		public float deleteColumnWidth = 40f;

		public ButtonCell deleteCellPrefab;

		public ButtonCellStyle deleteCellStyle;
		public ButtonCellStyle deleteCellStyleTemplate;
		public ButtonCellStyle DeleteCellStyle { get { return (deleteCellStyle != null) ? deleteCellStyle : deleteCellStyleTemplate; } }
		public DeleteColumnInfo deleteColumnInfo { get { return new DeleteColumnInfo(this); } }

		public bool rowAddButton;

		public ButtonCell addCellPrefab;

		public ButtonCellStyle addCellStyle;
		public ButtonCellStyle addCellStyleTemplate;
		public ButtonCellStyle AddCellStyle { get { return (addCellStyle != null) ? addCellStyle : addCellStyleTemplate; } }
		public AddCellInfo addCellInfo { get { return new AddCellInfo(this); } }

		public enum RowsColorMode { Plain, Striped }
		public RowsColorMode rowsColorMode;
		public Color bgColor = Color.gray;
		public Color altBgColor = DARK_GRAY;

		public bool selectableRows;
		public Color selectedBgColor = SELECTION_BLUE;

		public Color lineColor = Color.white;
		public Color titleBGColor = DARK_BLUE;
		public Color titleFontColor = Color.white;
		public int titleFontSize = 14;
		public Font titleFont;
		public FontStyle titleFontStyle;

		public float rowHeight = 32f;
		public float spacing = -1f;

		public Sprite outlineSprite;

		public bool updateCellStyleAtRuntime;
		public bool updateCellContentAtRuntime;

		public UnityMemberInfo targetCollection = new UnityMemberInfo(IsCollection);

		SortingState _sortingState = new SortingState();
		public SortingState sortingState { get { return _sortingState; } private set { _sortingState = value; } }

		int selectedRow = -1;

		public int SelectedRow {  get { return selectedRow; } }

		List<TableColumn> tableColumns = new List<TableColumn>();

		public IEnumerable<object> Elements
		{
			get
			{
				return targetCollection.Collection.Cast<object>().ToList();
			}
		}

		public int ElementCount
		{
			get
			{
				if (targetCollection.Collection == null) return 0;
				return targetCollection.Collection.Count;
			}
		}

		public System.Type ElementType
		{
			get
			{
				if (!targetCollection.IsDefined)
					return null;
				return targetCollection.ElementType;
			}
		}

		public float GetAvailableWidth(int nbColumns)
		{
			return GetComponent<RectTransform>().rect.width - (rowDeleteButtons ? deleteColumnWidth : 0f) - spacing * (nbColumns - 1 + (rowDeleteButtons ? 1 : 0));
		}

		public float GetAvailableWidth()
		{
			return GetAvailableWidth(GetValidColumnsCount());
		}

		public List<TableColumnInfo> GetValidColumns()
		{
			return columns.Where(c => c.IsDefined).ToList();
		}

		public int GetValidColumnsCount()
		{
			return columns.Count(c => c.IsDefined);
		}

		public TableColumnInfo GetColumnAt(int index)
		{
			List<TableColumnInfo> validColumns = GetValidColumns();
			return (index == validColumns.Count && rowDeleteButtons) ? deleteColumnInfo : validColumns[index];
		}

		HorizontalLayoutGroup _hGroup;
		HorizontalLayoutGroup hGroup
		{
			get
			{
				if (_hGroup == null) _hGroup = GetComponent<HorizontalLayoutGroup>();
				return _hGroup;
			}
		}

		ContentSizeFitter _fitter;
		ContentSizeFitter fitter
		{
			get
			{
				if (_fitter == null) _fitter = GetComponent<ContentSizeFitter>();
				return _fitter;
			}
		}

		public Color GetRowBGColor(int rowIndex)
		{
			if (selectableRows && rowIndex == selectedRow)
				return selectedBgColor;
			if (rowsColorMode == RowsColorMode.Plain)
				return bgColor;
			else
				return rowIndex % 2 == 0 ? bgColor : altBgColor;
		}

		private void Start()
		{
			sortingState = new SortingState();
			Initialize();
		}

		void HandleInputs()
		{
			if (Input.GetMouseButton(0))
				selectedRow = -1;
			int elementCount = ElementCount;
			if (selectedRow >= 0 && selectedRow < elementCount)
			{
				if (Input.GetKeyUp(KeyCode.UpArrow))
					selectedRow = Mathf.Max(selectedRow - 1, 0);
				else if (Input.GetKeyUp(KeyCode.DownArrow))
					selectedRow = Mathf.Min(selectedRow + 1, elementCount - 1);
			}
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				HandleTabKey(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
			}
		}

		enum TabDirection { Forward, Backward }

		private void HandleTabKey(bool isNavigateBackward)
		{
			GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
			if (selectedObject != null && selectedObject.activeInHierarchy)
			{
				Selectable currentSelection = selectedObject.GetComponent<Selectable>();
				if (currentSelection != null)
				{
					if (currentSelection.GetComponentInParent<Table>() != this)
						return;
					Selectable nextSelection =  FindNextSelectable(currentSelection, isNavigateBackward ? TabDirection.Backward: TabDirection.Forward);
					if (nextSelection != null)
					{
						nextSelection.Select();
					}
				}
			}
		}

		Selectable FindNextSelectable(Selectable selectable, TabDirection direction)
		{
			int columnIndex = -1, rowIndex = - 1;
			CellContainer cellContainer = selectable.GetComponentInParent<CellContainer>();
			for (int i = 0; i < tableColumns.Count; i++)
			{
				int row = tableColumns[i].IndexOf(cellContainer);
				if (row >= 0)
				{
					columnIndex = i;
					rowIndex = row;
					break;
				}
			}
			if (columnIndex < 0 || rowIndex < 0)
				return null;
			if (direction == TabDirection.Forward)
			{
				for (int i = rowIndex; i < ElementCount; i++)
				{
					int startIndex = (i == rowIndex) ? columnIndex + 1 : 0;
					for (int j = startIndex; j < tableColumns.Count; j++)
					{
						Selectable s = GetSelectableFromCellAt(j, i);
						if (s != null)
							return s;
					}
				}
			}
			else
			{
				for (int i = rowIndex; i >= 0; i--)
				{
					int startIndex = (i == rowIndex) ? columnIndex - 1 : tableColumns.Count - 1;
					for (int j = startIndex; j >= 0; j--)
					{
						Selectable s = GetSelectableFromCellAt(j, i);
						if (s != null)
							return s;
					}
				}
			}
			return null;
		}

		Selectable GetSelectableFromCellAt(int columnIndex, int rowIndex)
		{
			TableColumn column = tableColumns[columnIndex];
			CellContainer cc = column.GetCellAt(rowIndex);
			return (cc == null) ? null : cc.GetComponentInChildren<Selectable>();
		}

		void UpdateColumnWidths(List<TableColumnInfo> validColumns)
		{
			float totalWidth = GetAvailableWidth(validColumns.Count);
			System.Func<TableColumnInfo, float> GetAbsoluteWidth = (c) => c.useRelativeWidth ? c.width * totalWidth : c.width;
			foreach (TableColumnInfo column in validColumns)
			{
				if (column.autoWidth)
				{
					float newWidth = (totalWidth - validColumns.Where(c => !c.autoWidth).Sum(c => GetAbsoluteWidth(c))) / validColumns.Count(c => c.autoWidth);
					if (column.useRelativeWidth)
						newWidth = newWidth / totalWidth;
					if (!Mathf.Approximately(newWidth, column.width))
					{
						column.width = newWidth;
					}
				}
				column.AbsoluteWidth = column.useRelativeWidth ? column.width * totalWidth : column.width;
			}
		}

		private void Update()
		{
			Canvas.ForceUpdateCanvases();
			HandleInputs();

			List<TableColumnInfo> validColumns = GetValidColumns();

			fitter.horizontalFit = (validColumns.Count == 0 || validColumns.Any(c => c.autoWidth || c.useRelativeWidth)) ? ContentSizeFitter.FitMode.Unconstrained : ContentSizeFitter.FitMode.PreferredSize;

			UpdateColumnWidths(validColumns);

			if (transform.childCount != validColumns.Count() + (rowDeleteButtons ? 1 : 0))
				SetDirty();

			if (isDirty)
			{
				Debug.Log("Reinitialize");
				Initialize();
			}
			else
			{
				if (updateCellContentAtRuntime || !Application.isPlaying)
					UpdateContent();
				if (updateCellStyleAtRuntime || !Application.isPlaying)
					UpdateStyle();
			}

			hGroup.spacing = spacing;

		}

		[ContextMenu("Initialize")]
		public void Initialize()
		{
			while (transform.childCount > 0)
				DestroyImmediate(transform.GetChild(0).gameObject);
			if (!targetCollection.IsDefined)
			{
				GameObject content = transform.CreateChildGameObject("Text");
				Text text = content.AddComponent<Text>();
				text.text = "Table - Target not defined.";
				text.alignment = TextAnchor.MiddleCenter;
				return;
			}
			foreach (TableColumnInfo column in columns)
			{
				column.table = this;
			}
			List<TableColumnInfo> validColumns = GetValidColumns();
			Debug.LogFormat ("columns: {0}", string.Join(",", columns.Select(tci => tci.fieldName).ToArray()));
			Debug.LogFormat("validColumns: {0}", string.Join(",", validColumns.Select(tci => tci.fieldName).ToArray()));
			if (validColumns.Count == 0)
			{
				GameObject content = transform.CreateChildGameObject("Text");
				Text text = content.AddComponent<Text>();
				text.text = "Table - No valid columns.";
				text.alignment = TextAnchor.MiddleCenter;
				return;
			}
			tableColumns.Clear(); 
			int columnIndex = 0;
			foreach (TableColumnInfo column in validColumns)
			{
				TableColumn tableColumn = CreateColumn<TableColumn>("Column_" + column.fieldName);
				tableColumn.Initialize(this, column, columnIndex);
				tableColumns.Add(tableColumn);
				columnIndex++;
			}
			if (rowDeleteButtons)
			{
				TableColumn tableColumn = CreateColumn<DeleteColumn>("Column_Delete");
				tableColumn.Initialize(this, deleteColumnInfo, columnIndex);
				tableColumns.Add(tableColumn);
				columnIndex++;
			}
			isDirty = false;
		}

		TableColumn CreateColumn<ColumnType>(string goName) where ColumnType : TableColumn
		{
			GameObject columnGO = transform.CreateChildGameObject(goName);
			columnGO.transform.parent = transform;
			TableColumn tableColumn = columnGO.AddComponent<ColumnType>();
			return tableColumn;
		}

		public List<object> GetSortedElements()
		{
			if (sortingState.sortMode == SortingState.SortMode.Ascending)
				return Elements.OrderBy(sortingState.KeySelector).ToList();
			else if (sortingState.sortMode == SortingState.SortMode.Descending)
				return Elements.OrderByDescending(sortingState.KeySelector).ToList();
			else
				return Elements.ToList();
		}

		public void ColumnTitleClicked(TableColumnInfo column)
		{
			sortingState.ClickOnColumn(column);
			SetDirty();
		}

		public void SetSelected(int rowIndex)
		{
			selectedRow = rowIndex;
		}

		public void UpdateContent()
		{
			foreach (TableColumn column in tableColumns)
				column.UpdateContent();
		}

		public void UpdateStyle()
		{
			foreach (TableColumn column in tableColumns)
				column.UpdateStyle();
		}

#if UNITY_EDITOR

		public ScrollRect scrollViewPrefab;

		public bool IsScrollable
		{
			get
			{
				return transform.parent != null && transform.parent.parent != null && transform.parent.parent.GetComponent<ScrollRect>() != null;
			}
		}

		public void MakeScrollable(bool scrollable)
		{
			if (scrollable)
			{
				if (IsScrollable)
					return;
				this.GetComponent<ContentSizeFitter>().enabled = false;
				ScrollRect scrollView = (ScrollRect)PrefabUtility.InstantiatePrefab(scrollViewPrefab);
				scrollView.transform.SetParent(this.transform.parent, false);
				RectTransform tableRT = GetComponent<RectTransform>();
				RectTransform scrollViewRT = scrollView.GetComponent<RectTransform>();
				UnityEditorInternal.ComponentUtility.CopyComponent(tableRT);
				UnityEditorInternal.ComponentUtility.PasteComponentValues(scrollViewRT);
				tableRT.SetParent(scrollView.viewport, false);
				tableRT.sizeDelta = Vector2.zero;
				scrollView.content = tableRT;
				this.GetComponent<ContentSizeFitter>().enabled = true;
				EditorGUIUtility.PingObject(gameObject);
			}
			else
			{
				if (!IsScrollable)
					return;
				this.GetComponent<ContentSizeFitter>().enabled = false;
				ScrollRect scrollView = this.transform.parent.parent.GetComponent<ScrollRect>();
				RectTransform tableRT = GetComponent<RectTransform>();
				RectTransform scrollViewRT = scrollView.GetComponent<RectTransform>();
				tableRT.parent = scrollViewRT.parent;
				UnityEditorInternal.ComponentUtility.CopyComponent(scrollViewRT);
				UnityEditorInternal.ComponentUtility.PasteComponentValues(tableRT);
				DestroyImmediate(scrollView.gameObject);
				this.GetComponent<ContentSizeFitter>().enabled = true;
				EditorGUIUtility.PingObject(gameObject);
			}
		}

#endif

	}

}