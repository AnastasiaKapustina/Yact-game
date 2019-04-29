using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

namespace UnityUITable
{

	[ExecuteInEditMode]
	[RequireComponent(typeof(VerticalLayoutGroup))]
	public class TableColumn : MonoBehaviour
	{

		public Table table;
		public TableColumnInfo info { get { return table.GetColumnAt(columnIndex); } }
		protected LayoutElement layoutElement;

		protected VerticalLayoutGroup columnLayout;
		public int columnIndex;

		protected List<CellContainer> cellContainers = new List<CellContainer>();

		protected CellContainer addButtonCellContainer;

		public void Initialize(Table table, TableColumnInfo info, int columnIndex)
		{
			transform.DestroyChildrenImmediate();
			cellContainers.Clear();
			this.table = table;
			this.columnIndex = columnIndex;
			columnLayout = gameObject.GetOrAddComponent<VerticalLayoutGroup>();
			columnLayout.childControlWidth = true;
			columnLayout.childControlHeight = true;
			columnLayout.childForceExpandWidth = true;
			columnLayout.childForceExpandHeight = true;
			columnLayout.spacing = -1f;
			ContentSizeFitter sizeFitter = gameObject.AddComponent<ContentSizeFitter>();
			sizeFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
			sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			layoutElement = gameObject.AddComponent<LayoutElement>();

			CreateTitleCell();

			CreateCells();

			if (table.rowAddButton)
			{
				if (columnIndex == 0)
					CreateAddButton();
				else
					AddEmptyCell();
			}

			Update();
		}

		void CreateAddButton()
		{
			addButtonCellContainer = GameObjectUtils.InstantiatePrefab(table.emptyCellContainerPrefab, transform);
			addButtonCellContainer.Initialize(table);
			ButtonCell cell = (ButtonCell)addButtonCellContainer.CreateCellContent(table.addCellPrefab);
			cell.InitializeSpecialAction(
				table,
				0,
				() =>
				{
					object obj = new PropertyOrFieldInfo(table.targetCollection.GetMember()).GetValue(table.targetCollection.GetComponent());
					MethodInfo addMethod = obj.GetType().GetMethod("Add");
					ConstructorInfo constructor = table.ElementType.GetConstructor(new System.Type[] { });
					if (constructor == null)
					{
						Debug.LogError("There is no default constructor on the collection's element type.");
						return;
					}
					object newElmt = constructor.Invoke(new object[] { });
					if (addMethod == null)
					{
						Debug.LogError("There is no Add method on this collection.");
						return;
					}
					addMethod.Invoke(obj, new object[] { newElmt });
				},
				table.addCellInfo);
		}

		protected virtual void CreateTitleCell()
		{
			HeaderCellContainer title = GameObjectUtils.InstantiatePrefab(table.columnTitlePrefab, transform);
			title.Initialize(table, this, info);
		}

		protected virtual void CreateCells()
		{
			int elmtCount = table.ElementCount;
			for (int rowIndex = 0; rowIndex < elmtCount; rowIndex++)
			{
				CreateCell(rowIndex);
			}
		}

		protected virtual CellContainer CreateCell(int rowIndex)
		{
			TableCellContainer cellContainer = GameObjectUtils.InstantiatePrefab(table.cellContainerPrefab, transform);
			cellContainer.Initialize(table, rowIndex);
			cellContainers.Add(cellContainer);
			TableCell contentInstance = cellContainer.CreateCellContent(info.CellPrefab);
			contentInstance.Initialize(table, rowIndex, info);
			return cellContainer;
		}

		protected void AddEmptyCell(string name)
		{
			GameObject emptyTitleGO = transform.CreateChildGameObject(name);
			CellContainer emptyTitle = emptyTitleGO.AddComponent<CellContainer>();
			emptyTitle.Initialize(table);
		}

		protected void AddEmptyCell()
		{
			AddEmptyCell("Empty Cell");
		}

		protected void Update()
		{
			if (table == null || info == null)
				return;
			layoutElement.preferredWidth = info.AbsoluteWidth;
			columnLayout.spacing = table.spacing;
			if (!table.targetCollection.IsDefined)
				return;
		}

		void UpdateRows(int expectedNbRows, int actualNbRows, int bottomRows)
		{
			if (actualNbRows < expectedNbRows)
			{
				int nbToAdd = expectedNbRows - actualNbRows;
				for (int i = 0; i < nbToAdd; i++)
				{
					int rowIndex = actualNbRows + i - 1 - bottomRows;
					CellContainer container = CreateCell(rowIndex);
					container.SetRowIndex(rowIndex);
				}
			}
			else if (actualNbRows > expectedNbRows)
			{
				int nbToRemove = actualNbRows - expectedNbRows;
				int removalIndex = expectedNbRows - 1 - bottomRows;
				for (int i = 0; i < nbToRemove; i++)
				{
					DestroyImmediate(cellContainers[removalIndex].gameObject);
					cellContainers.RemoveAt(removalIndex);
				}
			}
		}
		 
		public void TitleClicked()
		{
			table.ColumnTitleClicked(info);
		}

		public int IndexOf(CellContainer cellContainer)
		{
			return cellContainers.IndexOf(cellContainer);
		}

		public CellContainer GetCellAt(int index)
		{
			try
			{
				return cellContainers[index];
			}
			catch(System.IndexOutOfRangeException)
			{
				return null;
			}
		}

		public void UpdateContent()
		{
			int bottomRows = (table.rowAddButton ? 1 : 0);
			int expectedNbRows = table.ElementCount + 1 + bottomRows;
			int actualNbRows = transform.childCount;
			if (actualNbRows != expectedNbRows)
				UpdateRows(expectedNbRows, actualNbRows, bottomRows);
			foreach (CellContainer cell in cellContainers)
				cell.UpdateContent();
		}

		public void UpdateStyle()
		{
			foreach (CellContainer cell in cellContainers)
				cell.UpdateStyle();
			if (addButtonCellContainer != null)
				addButtonCellContainer.UpdateStyle();
		}

	}

}
